using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueScheduler : MonoBehaviour, IMicrophoneEventListener, IInit, IDialogueReady
{
    private Queue<(NPC,Dialog)> voiceQueue = new();
    private NPC lastNPC;
    private bool micRecording = false;
    private DialogUI dialogUI;
    private DialogueSystemComponent dialogueSystem;


    void Start()
    {
        SceneManager.activeSceneChanged += (Scene _, Scene _) => {RefreshUIObject();};        
        RefreshUIObject();
    }

    void RefreshUIObject()
    {
        dialogUI = FindFirstObjectByType<DialogUI>();
    }

    [Init]
    private void Init(DialogueSystemComponent dialogueSystem, ExtendedWhisperManager extendedWhisperManager)
    {
        this.dialogueSystem = dialogueSystem;
        extendedWhisperManager.AddListener(this);
        dialogueSystem.AddListener(this);
    }

    void Update()
    {
        (NPC, Dialog) nextVoiceLine = (null, null);
        //If the next voice line is not ready or if either the user or the npc's are talking, do not run anything
        if(!(voiceQueue.TryPeek(out nextVoiceLine) && nextVoiceLine.Item2.audioClip != null) || micRecording || (lastNPC != null && lastNPC.audioSource.isPlaying)) return;
        if (voiceQueue.TryDequeue(out _))
        {
            nextVoiceLine.Item1.audioSource.clip = nextVoiceLine.Item2.audioClip;
            nextVoiceLine.Item1.audioSource.Play();
            lastNPC = nextVoiceLine.Item1;
            if(dialogUI.isActiveAndEnabled)
            {
                dialogUI.SetText(nextVoiceLine.Item2.dialog);
            }
        }
    }

    public void onMicrophoneStartRecording()
    {
        micRecording = true;
        if(lastNPC != null)
        {
            lastNPC.audioSource.Stop();
        }
        voiceQueue.Clear();
    }

    public void onMicrophoneStopRecording(Dialog _)
    {
        micRecording = false;
    }
    
    public void onDialogueReady((NPC,Dialog) queueItem)
    {
        voiceQueue.Enqueue(queueItem);
    }
}