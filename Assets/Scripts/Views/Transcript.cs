using UnityEngine;
using UnityEngine.UI;

public class Transcript : MonoBehaviour, IMicrophoneEventListener, IDialoguePlaying
{

    ExtendedWhisperManager extendedWhisperManager;
    DialogueScheduler dialogueSystem;

    public Text text;

    private ScrollRect scrollRect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        text = text != null ? text : GetComponentInChildren<Text>();
        scrollRect = GetComponentInChildren<ScrollRect>();
        extendedWhisperManager = BootStrap.Instance.GetComponentInChildren<ExtendedWhisperManager>();
        dialogueSystem = BootStrap.Instance.GetComponentInChildren<DialogueScheduler>();
        extendedWhisperManager.AddListener(this);
        dialogueSystem.AddListener(this);
    }

    public void onMicrophoneStartRecording()
    {
        return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        extendedWhisperManager.RemoveListener(this);
        dialogueSystem.RemoveListener(this);
    }

    public void OnDialoguePlaying(Dialog dialog)
    {
        text.text += $"[{dialog.actor.getName()}] <i>{dialog.dialog}</i>\n\n";
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content); 
    }
    
    public void onMicrophoneStopRecording(Dialog recordedaudio)
    {
        OnDialoguePlaying(recordedaudio);
    }
}
