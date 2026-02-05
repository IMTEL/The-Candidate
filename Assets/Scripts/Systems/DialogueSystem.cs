using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alchemy.Inspector;
using UnityEngine;
using Whisper;
using Whisper.Utils;


/// <summary>
/// Creates dialogues with the AI moduel and
/// </summary>
public class DialogueSystem : IMicrophoneEventListener
{
    private int userCounter;
    private LLMService aIService;
    private ScenarioSettings scenario;
    private WhisperManager whisper;
    private TTService TTService;
    private NPCRegistry NPCRegistry;

    private List<Dialog> dialogs;
    public Action<(NPC, Dialog)> onDialogueReady;

    public DialogueSystem(LLMService aIService, ScenarioSettings scenario, WhisperManager whisper, TTService ttService, NPCRegistry nPCRegistry)
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
    /// The starting prompt within the message
    /// </summary>
    private readonly string startPrompt = @"
# Context
You are an actor within a movie where you are with different other people to interview a potential candidate. 


## Setting
%s

## Your Role
%s

## Actors
%s

# Instructions
- Always follow the directors command. 
- You are ONLY allowed up to 2 follow up question for a subject. NOTHING MORE
- When mentioning your interpretation of the candidate, explain your though process.
- The last dialog should be the ONLY ONE that leads to a candidates input
- All actors do NOT response. Use the roles descriptions and evaluate if the actors should.
- ALL DIALOGS SHOULD BE IN NORWEGIAN
";



    /// <summary>
    /// Builds the prompt
    /// </summary>
    /// <param name="job"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    [Button]
    string BuildPrompt(Environment.Job job, Environment.Actor actor)
    {
        string setting = job.ToString();
        string actors = string.Join("\n - ", job.actors.Select(actor => actor.ToString()));
        return string.Format(startPrompt, setting, actor.getPrompt(), actors);
    }


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
        foreach(NPC npc in NPCRegistry.GetNPCEnumerable())
        {
            Dialog dialog = new Dialog(npc.actor, await aIService.getAIResponse(BuildPrompt(scenario.job, npc.actor), dialogs));
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
        tokenSource.Cancel();
    }


    public void onMicrophoneStopRecording(AudioChunk audioChunk)
    {
        tokenSource = new CancellationTokenSource();
        ProcessAudio(audioChunk, tokenSource.Token).Start();
    }
}