using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueScheduler : MonoBehaviour, IMicrophoneEventListener, IInit, IDialogueReady, IEvent<IDialoguePlaying>, IPause
{
    private readonly Queue<(NPC npc, Dialog dialog)> voiceQueue = new();
    private (NPC npc, Dialog dialog) lastDialogue = (null, null);
    private bool micRecording = false;

    private bool _fetchingClip = false;

    private Action<Dialog> OnDialoguePlaying;

    private readonly Queue<Action> AudioActionQueue = new();

    //So we need two PauseAudio. One to indicate that we are supposed to pause it
    //And one to ensure that we actually paused it
    private (bool paused, bool audiopaused) PauseAudio = (false, false);

    void Start()
    {
        SceneManager.activeSceneChanged += (_, _) => { PauseAudio = (false, false); lastDialogue = (null, null); };
    }

    [Init]
    private void Init(DialogueSystemComponent dialogueSystem, ExtendedWhisperManager extendedWhisperManager)
    {
        extendedWhisperManager.AddListener(this);
        dialogueSystem.AddListener(this);
    }

    private void Update()
    {
        //Reset Audio stupidity
        while (AudioActionQueue.TryDequeue(out Action result))
        {
            result.Invoke();
        }
        // 1. If we are paused, the microphone is recording or the the last npc. Don't do jack shit
        if (micRecording || (lastDialogue.npc && (lastDialogue.npc.audioSource.isPlaying || PauseAudio.paused || PauseAudio.audiopaused))) return;
        bool setText = false;

        //Next, we will check if we latest is finished and if not, we are fetching the next one
        if ((lastDialogue.dialog == null || lastDialogue.dialog.IsFinished) && voiceQueue.TryDequeue(out lastDialogue))
        {
            setText = true;
        }
        //I am unsure about the behaviour, so I am adding this as a precaution
        if (lastDialogue.dialog != null && !lastDialogue.dialog.IsFinished)
        {
            SetupVoiceline(setText);
        }
    }

    private async void SetupVoiceline(bool first = false)
    {
        if (_fetchingClip) return;
        _fetchingClip = true;
        if (first)
        {
            OnDialoguePlaying?.Invoke(lastDialogue.dialog);
        }
        try
        {
            if (lastDialogue.npc)
            {
                lastDialogue.npc.audioSource.clip = await lastDialogue.dialog.Next();
                if (lastDialogue.npc.audioSource.clip) AudioActionQueue.Enqueue(() => { ModifyAudio(AudioActions.Play); });
            }
        }
        finally
        {
            _fetchingClip = false;
        }
    }

    public void onMicrophoneStartRecording()
    {
        micRecording = true;
        if (lastDialogue.npc)
        {
            AudioActionQueue.Enqueue(() => ModifyAudio(AudioActions.Stop));
        }
        voiceQueue.Clear();
    }

    public void onMicrophoneStopRecording(Dialog _)
    {
        micRecording = false;
    }

    public void onDialogueReady((NPC, Dialog) queueItem)
    {
        if (!micRecording) voiceQueue.Enqueue(queueItem);
    }

    public void AddListener(IDialoguePlaying listener)
    {
        OnDialoguePlaying += listener.OnDialoguePlaying;
    }

    public void RemoveListener(IDialoguePlaying listener)
    {
        OnDialoguePlaying -= listener.OnDialoguePlaying;
    }

    public void OnPauseClick(bool paused)
    {
        PauseAudio.paused = paused;
        if (PauseAudio.paused)
        {
            AudioActionQueue.Enqueue(() => { ModifyAudio(AudioActions.Pause); });
        }
        else if (PauseAudio.audiopaused && lastDialogue.npc)
        {
            AudioActionQueue.Enqueue(() => { ModifyAudio(AudioActions.Resume); });
        }
    }

    private void ModifyAudio(AudioActions action)
    {
        switch (action)
        {
            case AudioActions.Play:
                if (lastDialogue.npc)
                {
                    lastDialogue.npc.audioSource.Play();
                }
                return;
            case AudioActions.Resume:
                if (lastDialogue.npc && PauseAudio.audiopaused)
                {
                    lastDialogue.npc.audioSource.Play();
                }
                PauseAudio.audiopaused = false;
                return;

            case AudioActions.Stop:
                lastDialogue.npc.audioSource.Stop();
                lastDialogue = (null, null);
                return;

            case AudioActions.Pause:
                if (lastDialogue.npc && lastDialogue.npc.audioSource.isPlaying)
                {
                    PauseAudio.audiopaused = true;
                    lastDialogue.npc.audioSource.Pause();
                }
                return;
        }
    }

    private enum AudioActions
    {
        Play,
        Pause,
        Stop,
        Resume
    }
}

