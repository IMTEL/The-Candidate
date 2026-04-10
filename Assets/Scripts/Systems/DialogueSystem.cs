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
    private ScenarioSettings scenario;
    private NPCRegistry NPCRegistry;

    private List<Dialog> dialogs = new();
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
    string BuildPrompt(Environment.Job job, Environment.Actor actor)
    {
        string setting = job.ToString();
        string actors = string.Join("\n - ", job.actors.Select(actor => actor.GetString()));
        return string.Format(startPrompt, setting, actor?.GetString(), actors);
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
            Dialog dialog = new(npc.actor, await ((ILLMService)npc.LLMService.CreateService()).GetAIResponse(BuildPrompt(scenario.job, npc.actor), dialogs));
            token.ThrowIfCancellationRequested();
            dialogs.Add(dialog);
            onDialogueReady?.Invoke((npc, dialog));
            createAudioClip(npc, dialog, token).Start();
        }
    }

    /// <summary>
    /// Create a Audioclip trough the TTService and links it to the dialog in question.
    /// 
    /// Limited trough the use a simulatnousVoiceRequests attribute to x amount a the same time and can be cancelled trough a cancellationToken
    /// </summary>
    /// <param name="nPC">The NPC that is gonna talk</param>
    /// <param name="dialog">The dialog object to be created</param>
    /// <param name="token">A cancellation token if the it should stop</param>
    /// <returns></returns>
    private async Task createAudioClip(NPC npc, Dialog dialog, CancellationToken token)
    {
        await simulatnousVoiceRequests.WaitAsync();
        try
        {
            if (token.IsCancellationRequested)
            {
                simulatnousVoiceRequests.Release();
                return;
            }
            dialog.audioClip = await ((ITTService)npc.TTService.CreateService()).getAudioClip(dialog.dialog, npc.GetTTSOptions());
        }
        finally { simulatnousVoiceRequests.Release(); }
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