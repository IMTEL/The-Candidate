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
        IEnumerator<NPC> npcs = NPCRegistry.GetNPCEnumerable().GetEnumerator();
        npcs.MoveNext();
        while(npcs.Current)
        {
            NPC npc = npcs.Current;
            List<Dialog> dialogsCopy = new(dialogs);
            if (npcs.MoveNext()) dialogsCopy.Add(new(Environment.Actor.Director, "Say your thoughts, DON'T Ask a question"));
            else dialogsCopy.Add(new(Environment.Actor.Director, "Ask a question after your response"));
            string text = await ((ILLMService)npc.LLMService.CreateService()).GetAIResponse(BuildPrompt(scenario.job, npc), dialogsCopy);
            Dialog dialog = new(npc.actor, text, ((ITTService)npc.TTService.CreateService()).getAudioClip(text, npc.GetTTSOptions()));
            token.ThrowIfCancellationRequested();
            dialogs.Add(dialog);
            onDialogueReady?.Invoke((npc, dialog));
        }
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