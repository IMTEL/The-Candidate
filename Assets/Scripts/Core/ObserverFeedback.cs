using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// One thing the observer noticed: the candidate's own words, what it means, and what to do.
/// </summary>
public class ObserverPoint
{
    public string Quote { get; }
    public string Comment { get; }
    public string Tip { get; }

    public ObserverPoint(string quote, string comment, string tip)
    {
        Quote = quote;
        Comment = comment;
        Tip = tip;
    }
}

/// <summary>
/// Feedback from the observer: a shared agent from the agent configurator (KFI) that reads a
/// finished interview and tells the job seeker what went well and what to improve, each point
/// tied to something they actually said.
///
/// The rules are the same as in KFI (src/domain/observerFeedback.ts), which owns the observer's
/// prompt. Change them there first, then here.
/// </summary>
public class ObserverFeedback
{
    /// <summary>
    /// How many answers the candidate must give before there is anything to give feedback on
    /// </summary>
    public const int MinCandidateAnswers = 2;

    /// <summary>
    /// The longest transcript the observer reads, in characters: about 10 000 tokens
    /// </summary>
    public const int MaxTranscriptLength = 40000;

    /// <summary>
    /// What the job seeker reads when the interview is too short for feedback.
    /// English translation: The conversation is too short to give feedback. Answer at least 2
    /// questions first.
    /// </summary>
    public static readonly string TooShortReason =
        $"Samtalen er for kort til å gi tilbakemelding. Svar på minst {MinCandidateAnswers} spørsmål først.";

    // A quote must be at least this many words. Shorter ones, like «jeg», are in almost every
    // answer, so they would let an invented point through.
    private const int MinQuoteWords = 3;

    private static readonly Regex TooShortLine = new(@"^[-*\s]*for kort\**:\**\s*(.+)$", RegexOptions.IgnoreCase);
    private static readonly Regex PointLine = new(@"^[-*\s]*(bra|bedre)\**:\**\s*(.+)$", RegexOptions.IgnoreCase);
    // The observer may name each part, as in «Sitat: … | Kommentar: … | Tips: …»
    private static readonly Regex Label = new(@"^\s*(sitat|kommentar|tips)\s*:\s*", RegexOptions.IgnoreCase);
    private static readonly Regex QuoteMarks = new(@"^[""«“']+|[""»”']+$");

    public IReadOnlyList<ObserverPoint> WentWell { get; }
    public IReadOnlyList<ObserverPoint> ToImprove { get; }

    /// <summary>
    /// Why there is no feedback, when the observer found the interview too short. Null otherwise.
    /// </summary>
    public string TooShort { get; }

    public bool HasPoints => WentWell.Count > 0 || ToImprove.Count > 0;

    private ObserverFeedback(IReadOnlyList<ObserverPoint> wentWell, IReadOnlyList<ObserverPoint> toImprove, string tooShort)
    {
        WentWell = wentWell;
        ToImprove = toImprove;
        TooShort = tooShort;
    }

    /// <summary>
    /// True if the candidate has answered enough times for the observer to judge
    /// </summary>
    public static bool HasEnoughToJudge(IEnumerable<Dialog> dialogs)
    {
        return Turns(dialogs).Count(dialog => dialog.actor == Environment.Actor.Candidate) >= MinCandidateAnswers;
    }

    /// <summary>
    /// The interview as one text, one line per turn, as the observer expects it. It is sent as a
    /// single message, so it doesn't matter how RAGdoll treats the roles of the turns. Every NPC
    /// is the interviewer, and the director's instructions are left out.
    /// </summary>
    public static string FormatTranscript(IEnumerable<Dialog> dialogs)
    {
        return string.Join("\n", Turns(dialogs).Select(dialog =>
        {
            string speaker = dialog.actor == Environment.Actor.Candidate ? "Kandidat" : "Intervjuer";
            return $"{speaker}: {Regex.Replace(dialog.Dialogue, @"\s+", " ").Trim()}";
        }));
    }

    /// <summary>
    /// Reads the observer's reply: one line per point, «Bra: sitat | kommentar | tips» for what
    /// went well and «Bedre: …» for what to improve, or one line «For kort: grunn». A line that
    /// doesn't have all three parts is skipped.
    /// </summary>
    /// <returns>The feedback, or null if nothing is left</returns>
    public static ObserverFeedback Parse(string reply)
    {
        List<ObserverPoint> wentWell = new();
        List<ObserverPoint> toImprove = new();
        foreach (string line in reply.Split('\n').Select(line => line.Trim()))
        {
            // Markdown bold or a list marker around the label is allowed: «- **Bra:** …»
            Match tooShort = TooShortLine.Match(line);
            if (tooShort.Success) return new(new List<ObserverPoint>(), new List<ObserverPoint>(), tooShort.Groups[1].Value.Trim());

            Match point = PointLine.Match(line);
            if (!point.Success) continue;
            string[] parts = point.Groups[2].Value.Split('|').Select(part => Label.Replace(part, "").Trim()).ToArray();
            if (parts.Length != 3 || parts.Any(part => part.Length == 0)) continue;
            List<ObserverPoint> list = point.Groups[1].Value.ToLowerInvariant() == "bra" ? wentWell : toImprove;
            list.Add(new(QuoteMarks.Replace(parts[0], ""), parts[1], parts[2]));
        }
        return wentWell.Count > 0 || toImprove.Count > 0 ? new(wentWell, toImprove, null) : null;
    }

    /// <summary>
    /// Drops the points whose quote is not something the candidate said, word for word. Case,
    /// spacing and punctuation don't matter, but a quote must have at least three whole words.
    /// Only the candidate's turns count, so a quote of the interviewer is dropped too.
    /// </summary>
    public ObserverFeedback KeepGrounded(IEnumerable<Dialog> dialogs)
    {
        if (TooShort != null) return this;

        // Spaces around every text, so only whole words match: «kunde» is not in «kunder»
        List<string> said = Turns(dialogs)
            .Where(dialog => dialog.actor == Environment.Actor.Candidate)
            .Select(dialog => $" {string.Join(" ", Words(dialog.Dialogue))} ")
            .ToList();
        bool IsGrounded(ObserverPoint point)
        {
            string[] quoted = Words(point.Quote);
            return quoted.Length >= MinQuoteWords && said.Any(answer => answer.Contains($" {string.Join(" ", quoted)} "));
        }
        return new(WentWell.Where(IsGrounded).ToList(), ToImprove.Where(IsGrounded).ToList(), null);
    }

    /// <summary>
    /// The turns that were said out loud: no director instructions and no empty recordings
    /// </summary>
    private static IEnumerable<Dialog> Turns(IEnumerable<Dialog> dialogs)
    {
        return dialogs.Where(dialog =>
            dialog != null
            && dialog.actor != Environment.Actor.Director
            && !string.IsNullOrWhiteSpace(dialog.Dialogue));
    }

    /// <summary>
    /// The words of a text in lower case, without punctuation or quote marks
    /// </summary>
    private static string[] Words(string text)
    {
        return Regex.Split(text.ToLowerInvariant(), @"[^\p{L}\p{N}]+").Where(word => word.Length > 0).ToArray();
    }
}
