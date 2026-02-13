using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.UI;
using Whisper;
using Whisper.Utils;
[RequireComponent(typeof(MicrophoneRecord))]
public class ExtendedWhisperManager : WhisperManager, IEvent<IMicrophoneEventListener>, IButtonListener
{
    MicrophoneRecord mic;

    private event Action onMicrophoneStartRecording;
    private event Action<Dialog> onMicrophoneEndRecording;
    
    void Awake()
    {
        mic = GetComponent<MicrophoneRecord>();
        mic.OnRecordStop += SendChunk;
    }

    private async void SendChunk(AudioChunk recordedAudio)
    { 
        WhisperResult result = await this.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        AudioClip test = AudioClip.Create("User",1,recordedAudio.Channels,recordedAudio.Channels, false);
        Dialog newDialog = new Dialog(Environment.Actor.Candidate, result.Result, test);

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
