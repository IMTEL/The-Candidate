using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
[RequireComponent(typeof(MicrophoneRecord))]
public class MicrophoneInputRecording : MonoBehaviour, IEvent<IMicrophoneEventListener>, IButtonListener
{
    MicrophoneRecord mic;

    private event Action onMicrophoneStartRecording;
    private event Action<AudioChunk> onMicrophoneEndRecording;
    
    void Awake()
    {
        mic = GetComponent<MicrophoneRecord>();
        mic.OnRecordStop += SendChunk;
    }

    private void SendChunk(AudioChunk recordedAudio)
    {
        onMicrophoneEndRecording.Invoke(recordedAudio);
    }

    [Button]

    public bool ToggleMicrophone()
    {
        if (mic.IsRecording) StartRecording();
        else StopRecording();
        return mic.IsRecording; 
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

    public void AddListener(IEnumerable<IMicrophoneEventListener> listeners)
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

    public void RemoveListener(IEnumerable<IMicrophoneEventListener> listeners)
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

    public void OnClick()
    {
        ToggleMicrophone();
    }
}
