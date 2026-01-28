using System;
using Alchemy.Inspector;
using UnityEngine;
using Whisper.Utils;
[RequireComponent(typeof(MicrophoneRecord))]
public class MicrophoneInputRecording : MonoBehaviour
{
    MicrophoneRecord mic;

    public static event Action onMicrophoneStartRecording;
    public static event Action<AudioChunk> onMicrophoneEndRecording;
    
    void Awake()
    {
        mic = GetComponent<MicrophoneRecord>();
        mic.OnRecordStop += sendChunk;
    }

    private void sendChunk(AudioChunk recordedAudio)
    {
        onMicrophoneEndRecording.Invoke(recordedAudio);
    }

    [Button]
    public void ToggleMicrophone()
    {
        if (mic.IsRecording) onMicrophoneStartRecording?.Invoke();
        else this.StopRecording(); 
    }

    [Button]
    private void StopRecording()
    {
        mic.StopRecord();
    }
}
