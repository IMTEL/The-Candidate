using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.UI;
using Whisper;
using Whisper.Utils;

[RequireComponent(typeof(MicrophoneRecord))]
[RequireComponent(typeof(WhisperManager))]
public class ExtendedWhisperManager : MonoBehaviour, IEvent<IMicrophoneEventListener>, IButtonListener
{
    MicrophoneRecord mic;

    WhisperManager whisperManager;

    private event Action onMicrophoneStartRecording;
    private event Action<Dialog> onMicrophoneEndRecording;
    
    private void Awake()
    {
        mic = GetComponent<MicrophoneRecord>();
        whisperManager = GetComponent<WhisperManager>();
        mic.OnRecordStop += SendChunk;
    }

    private async void SendChunk(AudioChunk recordedAudio)
    { 
        WhisperResult result = await whisperManager.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        AudioClip test = AudioClip.Create("User",1,recordedAudio.Channels,recordedAudio.Frequency, false);
        Dialog newDialog = new(Environment.Actor.Candidate, result.Result, test);
        onMicrophoneEndRecording?.Invoke(newDialog);
    }

    [Button]
    public bool ToggleMicrophone()
    {
        if (!mic.IsRecording) StartRecording();
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
        foreach (IMicrophoneEventListener listener in listeners)
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
