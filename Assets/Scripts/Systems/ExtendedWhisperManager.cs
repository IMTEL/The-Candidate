using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Alchemy.Inspector;
using NUnit.Framework;
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
    private CancellationTokenSource cancellationToken = new();

    private readonly List<Task<WhisperResult>> set = new();

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
        cancellationToken.Cancel();
        cancellationToken = new();
        CancellationTokenSource thisCancellationToken = cancellationToken;
        string dialog = "";
        Task<WhisperResult> task = whisperManager.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        set.Add(task);
        await Task.WhenAll(set.ToArray());
        if (thisCancellationToken.IsCancellationRequested || mic.IsRecording)
        {
            return;
        }
        lock (set)
        {
            foreach (var item in set)
            {
                dialog += item.Result.Result;
            }
            set.Clear();            
        }
        Dialog newDialog = new(Environment.Actor.Candidate, dialog);
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
