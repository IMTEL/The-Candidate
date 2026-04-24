using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


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

    private SemaphoreSlim simulatnousVoiceRequests = new(2);


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
    string BuildPrompt(Environment.Job job, Environment.Actor actor, string name = "")
    {
        if(name == "")
        {
            name = "Dict up a name for yourself";
        }
        string setting = job.ToString();
        string actors = string.Join("\n - ", job.actors.Select(actor => actor.GetString()));
        return string.Format(startPrompt, setting, $"{actor?.GetString()}\nYour name is: {name}", actors);
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
        foreach (NPC npc in NPCRegistry.GetNPCEnumerable())
        {
            string text = await ((ILLMService)npc.LLMService.CreateService()).GetAIResponse(BuildPrompt(scenario.job, npc.actor, npc.name), dialogs);
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