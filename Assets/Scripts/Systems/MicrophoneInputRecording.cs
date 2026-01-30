using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;
using Whisper.Utils;
[RequireComponent(typeof(MicrophoneRecord))]
public class MicrophoneInputRecording : MonoBehaviour
{
    MicrophoneRecord mic;

    private event Action onMicrophoneStartRecording;
    private event Action<AudioChunk> onMicrophoneEndRecording;
    
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
        if (mic.IsRecording) StartRecording();
        else StopRecording(); 
    }

    private void StopRecording()
    {
        mic.StopRecord();
    }

    private void StartRecording()
    {
        onMicrophoneStartRecording?.Invoke();
        mic.StartRecord();
    }

    public void AddListener(List<IMicrophoneEventListener> listeners)
    {
        foreach (var listener in listeners)
        {
            AddListener(listener);
        }
    }

    public void AddListener(IMicrophoneEventListener listener)
    {
        if (listener == null) return;
        onMicrophoneStartRecording += listener.onMicrophoneStartRecording;
        onMicrophoneEndRecording += listener.onMicrophoneStopRecording; 
    }

    public void RemoveListener(List<IMicrophoneEventListener> listeners)
    {
        foreach (var listener in listeners)
        {
            RemoveListener(listener);
        }
    }

    public void RemoveListener(IMicrophoneEventListener listener)
    {
        if(listener == null) return;
        onMicrophoneStartRecording -= listener.onMicrophoneStartRecording;
        onMicrophoneEndRecording -= listener.onMicrophoneStopRecording; 
    }

}
