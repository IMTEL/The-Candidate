using System.Collections.Generic;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using Alchemy.Inspector;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using NUnit.Framework.Interfaces;
using UnityEditor.Playables;
using System;
[RequireComponent(typeof(WhisperManager))]
[RequireComponent(typeof(MicrophoneRecord))]
[DisallowMultipleComponent]
public class Logic : MonoBehaviour
{
    WhisperManager whisper;
    MicrophoneRecord microphoneRecord;
    AIManager aIManager {get;} = AIManager.GetInstance();
    TTService ttsService {get;} = TTService.GetInstance();
    
    private int dialogueCounter = 0;
    private int playedCounter = 0;
    private int userCounter = 0;

    [ShowInInspector]
    [LabelText("NPCs")]
    public HashSet<NPC> NPCs {get;} = new();
    
    [ShowInInspector]
    public HashSet<Vector3> spawnLocations = new();

    ConcurrentQueue<(NPC, Dialog)> voiceQueue = new();

    ConcurrentStack<Dialog> dialogs {get;} = new();
    
    SemaphoreSlim semaphore = new(1);

    public Environment.Job job;

    private CancellationTokenSource tokenSource;

    private void Awake()
    {
        whisper = GetComponent<WhisperManager>();
        microphoneRecord = GetComponent<MicrophoneRecord>();
        microphoneRecord.OnRecordStop += OnRecordStop;
    }

    /// <summary>
    /// Event for when we are done recording.
    /// Creates a new cancellationtokensource and passes it to the tasks
    /// </summary>
    /// <param name="recordedAudio">The audio from the microphone</param>
    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        tokenSource = new CancellationTokenSource();
        ProcessAudio(recordedAudio, tokenSource.Token).Start();
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
        test.SetData(recordedAudio.Data,0);
        dialogs.Append(new(Environment.Actor.Candidate, userDialog.Result, test));
        foreach(NPC npc in NPCs)
        {
            string npcDialogue = await aIManager.getAIResponse(job, npc.actor, dialogs.ToList());
            Dialog dialog = new(npc.actor, npcDialogue);
            token.ThrowIfCancellationRequested();
            dialogs.Append(dialog);
            dialogueCounter++;
            createAudioClip(npc,dialog, token).Start();
        }
    }

    private async Task createAudioClip(NPC nPC, Dialog dialog, CancellationToken token)
    {
        await semaphore.WaitAsync();
        if (token.IsCancellationRequested)
        {
            semaphore.Release();
            return;
        }
        dialog.audioClip = await ttsService.getAudioClip(nPC.voice, dialog.dialog);
        semaphore.Release();
        token.ThrowIfCancellationRequested();
        voiceQueue.Enqueue((nPC,dialog));
    }

    private async void OnRecordStart()
    {
        tokenSource?.Cancel();
        microphoneRecord.StartRecord();
        Thread.Sleep(10);
        int difference = dialogueCounter-playedCounter;
        (dialogueCounter, playedCounter) = (0,0);
        dialogs.TryPopRange(null, 0, difference);
        voiceQueue = new();
    }

    void Update()
    {
        if(voiceQueue.Count < 1 || microphoneRecord.IsRecording || NPCs.Any(npc => npc.audioSource.isPlaying)) return;
        if (voiceQueue.TryDequeue(out (NPC, Dialog) nextVoiceLine))
        {
            playedCounter++;
            nextVoiceLine.Item1.audioSource.clip = nextVoiceLine.Item2.audioClip;
            nextVoiceLine.Item1.audioSource.Play();
        }
    }
}


