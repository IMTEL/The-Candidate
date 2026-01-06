using System.Collections.Generic;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using Alchemy.Inspector;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
[RequireComponent(typeof(WhisperManager))]
[RequireComponent(typeof(MicrophoneRecord))]
[DisallowMultipleComponent]
public class Logic : MonoBehaviour
{
    WhisperManager whisper;
    MicrophoneRecord microphoneRecord;
    AIManager aIManager {get;} = AIManager.GetInstance();
    TTService ttsService {get;} = TTService.GetInstance();
    
    private int userCounter = 0;

    [ShowInInspector]
    [LabelText("NPCs")]
    public HashSet<NPC> NPCs {get;} = new();
    
    [ShowInInspector]
    public HashSet<Vector3> spawnLocations = new();

    ConcurrentQueue<(NPC, Dialog)> voiceQueue = new();

    List<Dialog> dialogs {get;} = new();

    public Environment.Job job;

    private void Awake()
    {
        whisper = GetComponent<WhisperManager>();
        microphoneRecord = GetComponent<MicrophoneRecord>();
        microphoneRecord.OnRecordStop += OnRecordStop;
    }

    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        
    }

    private async Task ProcessAudio(AudioChunk recordedAudio)
    {
        WhisperResult userDialog = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        //TODO: Double check that the audio for this is not mangled to bits
        AudioClip test = AudioClip.Create("User-"+userCounter,1,recordedAudio.Channels,recordedAudio.Channels, false);
        test.SetData(recordedAudio.Data,0);
        dialogs.Add(new(Environment.Actor.Candidate, userDialog.Result, test));
        foreach(NPC npc in NPCs)
        {
            string npcDialogue = await aIManager.getAIResponse(job, npc.actor, dialogs.ToList());
            Dialog dialog = new(npc.actor, npcDialogue);
            dialogs.Add(dialog);
            createAudioClip(npc,dialog).Start();
        }
    }

    private async Task createAudioClip(NPC nPC, Dialog dialog)
    {
        dialog.audioClip = await ttsService.getAudioClip(nPC.voice, dialog.dialog);
        voiceQueue.Enqueue((nPC,dialog));
    }

    private async void OnRecordStart()
    {
        microphoneRecord.StartRecord();
        dialogs.RemoveAll(dialog => dialog.audioClip == null);
        foreach((NPC npc, Dialog dialog) in voiceQueue)
        {
            npc.audioSource.Stop();
            dialogs.Remove(dialogs.Last(dialog => dialog.Equals(dialog)));
        }
    }

    void Update()
    {
        if(voiceQueue.Count < 1 || microphoneRecord.IsRecording || NPCs.Any(npc => npc.audioSource.isPlaying)) return;
        if (voiceQueue.TryDequeue(out (NPC, Dialog) nextVoiceLine))
        {
            nextVoiceLine.Item1.audioSource.clip = nextVoiceLine.Item2.audioClip;
            nextVoiceLine.Item1.audioSource.Play();
        }
    }
}


