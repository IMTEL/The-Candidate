using System.Collections.Generic;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using Alchemy.Inspector;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
/// <summary>
/// The logic system. Currently contains the dialogue system, so should probably be renamed and changed to something else later.s
/// </summary>
[RequireComponent(typeof(WhisperManager))]
[RequireComponent(typeof(MicrophoneRecord))]
[DisallowMultipleComponent]
public class Logic : MonoBehaviour
{
    WhisperManager whisper;
    MicrophoneRecord microphoneRecord;
    AIManager AIManager {get;} = AIManager.GetInstance();
    TTService TTService {get;} = TTService.GetInstance();
    
    /// <summary>
    /// Counter to keep track of the amount of dialogues since last user input
    /// </summary>
    private int dialogueCounter = 0;
    /// <summary>
    /// The amount of played dialogues since last user input
    /// </summary>
    private int playedCounter = 0;
    /// <summary>
    /// Changed to the 
    /// </summary>
    private int userCounter = 0;

    [ShowInInspector]
    [LabelText("NPCs")]
    public HashSet<NPC> NPCs {get;} = new();
    
    [ShowInInspector]
    public HashSet<Vector3> spawnLocations = new();

    ConcurrentQueue<(NPC, Dialog)> voiceQueue = new();

    ConcurrentStack<Dialog> dialogs {get;} = new();
    
    SemaphoreSlim simulatnousVoiceRequests = new(2);

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
        userCounter++;
        test.SetData(recordedAudio.Data,0);
        dialogs.Append(new(Environment.Actor.Candidate, userDialog.Result, test));
        foreach(NPC npc in NPCs)
        {
            string npcDialogue = await AIManager.getAIResponse(job, npc.actor, dialogs.ToList());
            Dialog dialog = new(npc.actor, npcDialogue);
            token.ThrowIfCancellationRequested();
            dialogs.Append(dialog);
            voiceQueue.Enqueue((npc,dialog));
            dialogueCounter++;
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

    /// <summary>
    /// Function to be called when the recording start. 
    /// It will clear out the voice queue as well as remove dialogs that did not get played.
    /// </summary>
    [Button]
    private async void OnRecordStart()
    {
        tokenSource?.Cancel();
        microphoneRecord.StartRecord();
        Thread.Sleep(10);
        int difference = dialogueCounter-playedCounter;
        (dialogueCounter, playedCounter) = (0,0);
        for(int i=0; i< difference; i++)
        {
            dialogs.TryPop(out _);
        }
        voiceQueue = new();
    }

    void Update()
    {
        //If the next voice line is not ready or if either the user or the npc's are talking, do not run anything
        if(!(voiceQueue.TryPeek(out (NPC, Dialog) nextVoiceLine) && nextVoiceLine.Item2.audioClip) || microphoneRecord.IsRecording || NPCs.Any(npc => npc.audioSource.isPlaying)) return;
        if (voiceQueue.TryDequeue(out _))
        {
            playedCounter++;
            nextVoiceLine.Item1.audioSource.clip = nextVoiceLine.Item2.audioClip;
            nextVoiceLine.Item1.audioSource.Play();
        }
    }
}


