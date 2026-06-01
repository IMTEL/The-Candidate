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

    private const int MAIN_LIMIT = 12;

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
        dialogs.Add(userDialog);
        if (IntermidateDialog(out string directorDialog))
        {
            dialogs.Add(new(Environment.Actor.Director, directorDialog));
        }
        List<NPC> listnpcs = NPCRegistry.GetNPCEnumerable().ToList().OrderBy((_) => random.Next()).ToList();
        IEnumerator<NPC> npcs = listnpcs.GetEnumerator();
        npcs.MoveNext();
        while(npcs.Current)
        {
            NPC npc = npcs.Current;
            List<Dialog> dialogsCopy = new(dialogs)
            {
                new(Environment.Actor.Director, npcs.MoveNext() ? $"Don't ask a question. ${npcs.Current.name} is after you" : "Ask a question if relevant")
            };

            string text = await ((ILLMService)npc.LLMService.CreateService()).GetAIResponse(BuildPrompt(scenario.job, npc), dialogsCopy, npc.GetLLMOptions());
            Dialog dialog = new(npc.actor, text, ((ITTService)npc.TTService.CreateService()).getAudioClip(text, npc.GetTTSOptions()));
            token.ThrowIfCancellationRequested();
            dialogs.Add(dialog);
            onDialogueReady?.Invoke((npc, dialog));
        }
    }

    private bool IntermidateDialog(out string dialog)
    {
        dialog = "";
        if(dialogs.Count() == 1)
        {
            dialog = "Focus on introducing yourself and the candidate. These can be things about how you/they are doing and names";
        }  
        //The amount of the candidate has answered + 2 (one for the last intermediate and another for reaching the limit)
        else if(dialogs.Count(dialog => dialog.actor == Environment.Actor.Candidate) == INTRODUCTION_LIMIT+2)
        {
            dialog = $"Time to start the interview";
        }
        //Same thing as above 
        else if (dialogs.Count(dialog => dialog.actor == Environment.Actor.Candidate) == MAIN_LIMIT+INTRODUCTION_LIMIT+3)
        {
            dialog = $"The interview is coming to a close, start wrapping it up by allowing the candidate to ask some questions";
        }
        return dialog == "";
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