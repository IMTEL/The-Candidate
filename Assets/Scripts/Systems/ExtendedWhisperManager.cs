using System.Linq;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whisper;
using Whisper.Utils;

[RequireComponent(typeof(MicrophoneRecord))]
[RequireComponent(typeof(WhisperManager))]
public class ExtendedWhisperManager : MonoBehaviour, IEvent<IMicrophoneEventListener>
{
    MicrophoneRecord mic;

    WhisperManager whisperManager;

    private event UnityAction onMicrophoneStartRecording;
    private event UnityAction<Dialog> onMicrophoneEndRecording;
    
    private void Awake()
    {
        mic = GetComponent<MicrophoneRecord>();
        whisperManager = GetComponent<WhisperManager>();
        mic.OnRecordStop += SendChunk;
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    private void OnSceneChange(Scene _, Scene __)
    {
        //Just a precaution if the microphone should be on or not;
        if (mic.IsRecording)
        {
            mic.OnRecordStop -= SendChunk;
            ToggleMicrophone();
            mic.OnRecordStop += SendChunk;
        }
    }

    private async void SendChunk(AudioChunk recordedAudio)
    { 
        WhisperResult result = await whisperManager.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        AudioClip test = AudioClip.Create("User",recordedAudio.Data.Count()/recordedAudio.Channels,recordedAudio.Channels,recordedAudio.Frequency, false);
        test.SetData(recordedAudio.Data,0);
        Dialog newDialog = new(Environment.Actor.Candidate, result.Result, test);
        onMicrophoneEndRecording?.Invoke(newDialog);
    }

    [Button]
    public bool ToggleMicrophone()
    {
        return ToggleMicrophone(!mic.IsRecording);
    }

    public bool ToggleMicrophone(bool value)
    {
        if(value != mic.IsRecording)
        {
            if (value) StartRecording();
            else StopRecording(); 
        }
        return mic.IsRecording; 
    }

    private void StopRecording()
    {
        mic.StopRecord();
    }

    private void StartRecording()
    {
        foreach (UnityAction item in onMicrophoneStartRecording.GetInvocationList().Cast<UnityAction>())
        {
            item.BeginInvoke(null,null);
        }
        mic.StartRecord();
    }


    public void AddListener(IMicrophoneEventListener listener)
    {
        if (listener == null) return;
        onMicrophoneStartRecording += listener.onMicrophoneStartRecording;
        onMicrophoneEndRecording += listener.onMicrophoneStopRecording;
    }

    public void RemoveListener(IMicrophoneEventListener listener)
    {
        if(listener == null) return;
        onMicrophoneStartRecording -= listener.onMicrophoneStartRecording;
        onMicrophoneEndRecording -= listener.onMicrophoneStopRecording; 
    }

}
