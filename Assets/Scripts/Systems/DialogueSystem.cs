using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Whisper;
using Whisper.Utils;


/// <summary>
/// Creates dialogues with the AI moduel and
/// </summary>
public class DialogueSystem : IMicrophoneEventListener
{
    private int userCounter;
    private AIService aIService;
    private ScenarioSettings scenario;
    private WhisperManager whisper;
    private TTService TTService;
    private NPCRegistry NPCRegistry;

    private List<Dialog> dialogs;
    public static Action<(NPC, Dialog)> onDialogueReady;

    public DialogueSystem(AIService aIService, ScenarioSettings scenario, WhisperManager whisper, TTService ttService, NPCRegistry nPCRegistry)
    {
        this.aIService = aIService;
        this.scenario = scenario;
        this.whisper = whisper;
        this.TTService = ttService;
        this.NPCRegistry = nPCRegistry;
    }

    private SemaphoreSlim simulatnousVoiceRequests = new(2);

    private CancellationTokenSource tokenSource = new();

    /// <summary>
    /// Task to process the audio and start creating dialogues for the NPC
    /// </summary>
    /// <param name="recordedAudio">The recorded audio from the microphone</param>
    /// <param name="token">A cancellation token that will be checked if it is suppose to stop</param>
    /// <returns></returns>
    private async Task ProcessAudio(AudioChunk recordedAudio, CancellationToken token)
    {
        WhisperResult userDialog = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        //TODO: Double check that the audio for this is not mangled to bits
        AudioClip test = AudioClip.Create("User-"+userCounter,1,recordedAudio.Channels,recordedAudio.Channels, false);
        userCounter++;
        test.SetData(recordedAudio.Data,0);
        dialogs.Add(new(Environment.Actor.Candidate, userDialog.Result, test));
        foreach(NPC npc in NPCRegistry.GetNPCs())
        {
            string npcDialogue = await Services.AIService.getAIResponse(scenario.job, npc.actor, dialogs);
            Dialog dialog = new(npc.actor, npcDialogue);
            token.ThrowIfCancellationRequested();
            dialogs.Add(dialog);
            onDialogueReady.Invoke((npc,dialog));
            createAudioClip(npc,dialog, token).Start();
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
        if (token.IsCancellationRequested)
        {
            simulatnousVoiceRequests.Release();
            return;
        }
        dialog.audioClip = await TTService.getAudioClip(npc.voice, dialog.dialog);
        simulatnousVoiceRequests.Release();
    }

    public void onMicrophoneStartRecording()
    {
        throw new System.NotImplementedException();
    }


    public void onMicrophoneStopRecording(AudioChunk audioChunk)
    {
        tokenSource = new CancellationTokenSource();
        ProcessAudio(audioChunk, tokenSource.Token).Start();
    }
}