using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(Canvas))]
public class Transcript : MonoBehaviour, IMicrophoneEventListener, IDialoguePlaying
{

    ExtendedWhisperManager extendedWhisperManager;
    DialogueScheduler dialogueSystem;

    TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        text = GetComponent<ScrollRect>().content.GetComponent<TMP_Text>();
        extendedWhisperManager = BootStrap.Instance.GetComponentInChildren<ExtendedWhisperManager>();
        dialogueSystem = BootStrap.Instance.GetComponentInChildren<DialogueScheduler>();
        extendedWhisperManager.AddListener(this);
        dialogueSystem.AddListener(this);
    }

    void Start()
    {

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
        text.text += $"[{dialog.actor.getName()}] {dialog.dialog}\n";
    }
    
    public void onMicrophoneStopRecording(Dialog recordedaudio)
    {
        OnDialoguePlaying(recordedaudio);
    }
}
