using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shows the observer's feedback when the job seeker ends the interview from the pause menu.
/// Place it in the interview scene, inactive. Nothing is kept: the conversation and the feedback
/// go away with the scene when the job seeker goes back to the start.
/// </summary>
public class ObserverFeedbackPanel : MonoBehaviour
{
    /// <summary>
    /// The provider with the observer agent's id (the same asset the interviewer picker uses)
    /// </summary>
    public PublishedRagProvider observer;

    /// <summary>
    /// The text in the scroll view, like in the transcript
    /// </summary>
    public Text text;

    /// <summary>
    /// Shown only when the feedback could not be fetched
    /// </summary>
    public Button retryButton;

    public Button backButton;

    public string startScene = "Start";

    private List<Dialog> dialogs = new();

    private ScrollRect scrollRect;

    void Awake()
    {
        scrollRect = GetComponentInChildren<ScrollRect>();
        retryButton.onClick.AddListener(() => _ = Load());
        backButton.onClick.AddListener(() => SceneManager.LoadScene(startScene));
    }

    void OnValidate()
    {
        if (text == null) Debug.LogError("text is missing");
        if (retryButton == null) Debug.LogError("retryButton is missing");
        if (backButton == null) Debug.LogError("backButton is missing");
    }

    /// <summary>
    /// Shows the panel and asks the observer about the interview so far
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        // A copy: a late reply from the interviewer may still arrive while the panel is open
        dialogs = new(BootStrap.Instance.GetComponentInChildren<DialogueSystemComponent>().GetDialogs());
        _ = Load();
    }

    private async Task Load()
    {
        retryButton.gameObject.SetActive(false);
        if (!ObserverFeedback.HasEnoughToJudge(dialogs))
        {
            SetText(ObserverFeedback.TooShortReason);
            return;
        }
        string transcript = ObserverFeedback.FormatTranscript(dialogs);
        if (transcript.Length > ObserverFeedback.MaxTranscriptLength)
        {
            // English translation: The interview is too long to give feedback on.
            SetText("Intervjuet er for langt til å gi tilbakemelding på.");
            return;
        }

        // English translation: The observer is reading the interview …
        SetText("Observatøren leser intervjuet …");
        string reply = await observer.GetService().AskObserver(transcript);
        // The job seeker may have left the scene while waiting
        if (this == null) return;

        // Only quotes the job seeker really said are shown. If none are left, it is an error,
        // like in the agent configurator.
        ObserverFeedback feedback = reply == null ? null : ObserverFeedback.Parse(reply)?.KeepGrounded(dialogs);
        if (feedback == null || (feedback.TooShort == null && !feedback.HasPoints))
        {
            // English translation: Could not make feedback. Try again.
            SetText("Kunne ikke lage en tilbakemelding. Prøv igjen.");
            retryButton.gameObject.SetActive(true);
            return;
        }
        SetText(feedback.TooShort ?? Render(feedback));
    }

    private void SetText(string value)
    {
        text.text = value;
        if (scrollRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
    }

    private static string Render(ObserverFeedback feedback)
    {
        StringBuilder builder = new();
        // English translation: This went well
        AppendPoints(builder, "Dette gikk bra", feedback.WentWell);
        // English translation: This can be better
        AppendPoints(builder, "Dette kan bli bedre", feedback.ToImprove);
        return builder.ToString().TrimEnd();
    }

    private static void AppendPoints(StringBuilder builder, string title, IReadOnlyList<ObserverPoint> points)
    {
        if (points.Count == 0) return;
        builder.Append($"<b>{title}</b>\n\n");
        foreach (ObserverPoint point in points)
        {
            builder.Append($"<i>«{point.Quote}»</i>\n");
            builder.Append($"{point.Comment}\n");
            // English translation: Tip:
            builder.Append($"Tips: {point.Tip}\n\n");
        }
    }
}
