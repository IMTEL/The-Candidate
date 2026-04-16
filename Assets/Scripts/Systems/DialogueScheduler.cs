using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueScheduler : MonoBehaviour, IMicrophoneEventListener, IInit, IDialogueReady, IEvent<IDialoguePlaying>, IPause
{
    private Queue<(NPC, Dialog)> voiceQueue = new();
    private NPC lastNPC;
    private bool micRecording = false;
    private DialogUI dialogUI;

    private Action<Dialog> OnDialoguePlaying;

    private readonly Queue<Action> AudioActionQueue = new();

    //So we need two PauseAudio. One to indicate that we are supposed to pause it
    //And one to ensure that we actually paused it
    private (bool, bool) PauseAudio = (false, false);

    void Start()
    {
        SceneManager.activeSceneChanged += (_, _) => { RefreshUIObject(); PauseAudio = (false, false); lastNPC = null; };
        RefreshUIObject();
    }

    void RefreshUIObject()
    {
        dialogUI = FindFirstObjectByType<DialogUI>();
    }

    [Init]
    private void Init(DialogueSystemComponent dialogueSystem, ExtendedWhisperManager extendedWhisperManager)
    {
        extendedWhisperManager.AddListener(this);
        dialogueSystem.AddListener(this);
    }

    void Update()
    {
        //Reset Audio stupidity
        while(AudioActionQueue.TryDequeue(out Action result))
        {
            result.Invoke();
        }
        

        (NPC, Dialog) nextVoiceLine = (null, null);
        //If the next voice line is not ready or if either the user or the npc's are talking, do not run anything
        if (!(voiceQueue.TryPeek(out nextVoiceLine) && (nextVoiceLine.Item1.TTService == null || nextVoiceLine.Item2.audioClip != null))
            || micRecording
            || (lastNPC != null && (lastNPC.audioSource.isPlaying || dialogUI.TextPlaying() || PauseAudio.Item1 || PauseAudio.Item2))) return;
        if (voiceQueue.TryDequeue(out _))
        {
            nextVoiceLine.Item1.audioSource.clip = nextVoiceLine.Item2.audioClip;
            nextVoiceLine.Item1.audioSource.Play();
            lastNPC = nextVoiceLine.Item1;
            dialogUI.SetText(nextVoiceLine.Item2.dialog);
            OnDialoguePlaying?.Invoke(nextVoiceLine.Item2);
        }
    }

    public void onMicrophoneStartRecording()
    {
        micRecording = true;
        if (lastNPC != null)
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
        voiceQueue.Enqueue(queueItem);
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
        PauseAudio.Item1 = paused;
        if (PauseAudio.Item1)
        {
            AudioActionQueue.Enqueue(() => { ModifyAudio(AudioActions.Pause); });
        }
        else if (PauseAudio.Item2 && lastNPC != null)
        {
            AudioActionQueue.Enqueue(() => { ModifyAudio(AudioActions.Resume); });
        }
    }

    private void ModifyAudio(AudioActions action)
    {
        switch (action)
        {
            case AudioActions.Resume:
                if (lastNPC != null && PauseAudio.Item2)
                {
                    lastNPC.audioSource.Play();
                }
                PauseAudio.Item2 = false;
                return;
                
            case AudioActions.Stop:
                lastNPC.audioSource.Stop();
                return;

            case AudioActions.Pause:
                if (lastNPC != null && lastNPC.audioSource.isPlaying)
                {
                    PauseAudio.Item2 = true;
                    lastNPC.audioSource.Pause();
                }
                return;
        }
    }

    private enum AudioActions
    {
        Pause,
        Stop,
        Resume
    }
}

