using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Whisper.Utils;

public class DialogueScheduler : MonoBehaviour, IMicrophoneEventListener
{
    private Queue<(NPC,Dialog)> voiceQueue = new();
    private NPC lastNPC;
    private bool micRecording = false;

    void Update()
    {
        //If the next voice line is not ready or if either the user or the npc's are talking, do not run anything
        if(!(voiceQueue.TryPeek(out (NPC, Dialog) nextVoiceLine) && nextVoiceLine.Item2.audioClip) || micRecording || (lastNPC && lastNPC.audioSource.isPlaying)) return;
        if (voiceQueue.TryDequeue(out _))
        {
            nextVoiceLine.Item1.audioSource.clip = nextVoiceLine.Item2.audioClip;
            nextVoiceLine.Item1.audioSource.Play();
            lastNPC = nextVoiceLine.Item1;
        }
    }

    public void onMicrophoneStartRecording()
    {
        micRecording = true;
        lastNPC.audioSource.Stop();
    }

    public void onMicrophoneStopRecording(AudioChunk audioChunk)
    {
        micRecording = false;
    }
    
    public void onDialogueReady((NPC,Dialog) queueItem)
    {
        voiceQueue.Append(queueItem);
    }
}