using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


/// <summary>
/// Creates dialogues with the AI model and
/// </summary>
public class DialogueSystem : IEvent<IDialogueReady>
{
    private readonly ScenarioSettings scenario;
    private readonly NPCRegistry NPCRegistry;

    private readonly List<Dialog> dialogs = new();
    public Action<(NPC, Dialog)> onDialogueReady;

    private readonly Random random = new();

    private const int INTRODUCTION_LIMIT = 2;

    private const int MAIN_LIMIT = 5;

    public DialogueSystem(ScenarioSettings scenario, NPCRegistry nPCRegistry, string startingprompt = "")
    {
        this.scenario = scenario;
        this.NPCRegistry = nPCRegistry;
        this.startPrompt = startingprompt;
    }

    /// <summary>
    /// The starting prompt within the message
    /// </summary>
    private readonly string startPrompt;

    /// <summary>
    /// Builds the prompt
    /// </summary>
    /// <param name="job"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    string BuildPrompt(Environment.Job job, NPC npc)
    {
        string setting = job.ToString();
        string actors = string.Join("\n - ", NPCRegistry.GetNPCEnumerable().Select((npc) => $"{npc.actor.getName()}: {npc.name}"));
        return string.Format(startPrompt, setting, $"{npc.actor?.ToString()}\nYour name is: {npc.name}", actors);
    }

    public (Task, CancellationTokenSource) HandleUserDialog(Dialog userDialog, CancellationTokenSource tokenSource = null)
    {
        if (userDialog == null) throw new ArgumentException("Dialog can't be null");
        tokenSource ??= new();
        return (HandleUserDialog(userDialog, tokenSource.Token), tokenSource);
    }

    /// <summary>
    /// Task to process the audio and start creating dialogues for the NPC
    /// </summary>
    /// <param name="recordedAudio">The recorded audio from the microphone</param>
    /// <param name="token">A cancellation token that will be checked if it is suppose to stop</param>
    /// <returns></returns>
    private async Task HandleUserDialog(Dialog userDialog, CancellationToken token)
    {
        if (IntermidateDialog(out string directorDialog))
        {
            dialogs.Add(new(Environment.Actor.Director, directorDialog));
        }
        dialogs.Add(userDialog);

        List<NPC> listnpcs = NPCRegistry.GetNPCEnumerable().ToList().OrderBy((_) => random.Next()).ToList();
        IEnumerator<NPC> npcs = listnpcs.GetEnumerator();
        npcs.MoveNext();
        while (npcs.Current)
        {
            NPC npc = npcs.Current;
            Agent agent = npc.GetAgent();
            if (!agent.GetLLMSettings(out LLMServiceProvider provider, out OptionSet options)){ return; }
            List<Dialog> dialogsCopy = new(dialogs)
            {
                new(Environment.Actor.Director, npcs.MoveNext() ? $"DON'T YOU DARE ask a question. ${npcs.Current.name} is after you" : "Ask ONLY 1 question IF relevant")
            };

            IEnumerable<string> dialogues = await ((ILLMService)provider.CreateService()).GetAIResponse(BuildPrompt(scenario.job, npc), dialogsCopy, options.GetOptions());
            if(!agent.GetTTSSettings(out TTSServiceProvider TTSProvider, out OptionSet TTSOptions))
            foreach (string dialogue in dialogues)
            {
                Dialog dialog = new(npc.actor, dialogue, ((ITTService)npc.TTService.CreateService()).getAudioClip(dialogue, TTSOptions.GetOptions()));
                token.ThrowIfCancellationRequested();
                dialogs.Add(dialog);
                onDialogueReady?.Invoke((npc, dialog));
            }

        }
    }

    private bool IntermidateDialog(out string dialog)
    {
        dialog = "";
        if (dialogs.Count() == 0)
        {
            dialog = "Start introducing each other. Focus ONLY on subjective questions. NOT WORK RELATED. Examples are: How are you today?, How was it arriving here? Done anything excited lately?";
        }
        //The amount of the candidate has answered + 1 (The empty one in the beginning)
        else if (dialogs.Count(dialog => dialog.actor == Environment.Actor.Candidate) == INTRODUCTION_LIMIT)
        {
            dialog = $"Time to start the interview. Focus ONLY on job related questions";
        }
        //Same thing as above 
        else if (dialogs.Count(dialog => dialog.actor == Environment.Actor.Candidate) == MAIN_LIMIT + INTRODUCTION_LIMIT)
        {
            dialog = $"Start wrapping up the interview by allowing the candidate to ask some questions";
        }
        return dialog != "";
    }

    public void AddListener(IDialogueReady listener)
    {
        onDialogueReady += listener.onDialogueReady;
    }

    public void RemoveListener(IDialogueReady listener)
    {
        onDialogueReady -= listener.onDialogueReady;
    }
}