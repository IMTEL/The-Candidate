using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour, IDialoguePlaying, IMicrophoneEventListener
{

    public float letterPerSecond = 8;
    public float AfterSeconds = 2;
    private float timer = 0;
    private string dialogue = "";
    public Toggle toggle;
    public TMP_Text text;

    private DialogueScheduler dialogueScheduler;

    private SpriteImageAnimation bufferAnimation;

    private int pages;

    public Button NextButton;
    public Button PreviousButton;

    private void Awake()
    {
        toggle = toggle != null ? toggle : GetComponentInChildren<Toggle>();
        text = text != null ? text : GetComponentInChildren<TMP_Text>();
        bufferAnimation = GetComponentInChildren<SpriteImageAnimation>();

    }

    private void Start()
    {
        ExtendedWhisperManager extendedWhisperManager = BootStrap.Instance.GetComponentInChildren<ExtendedWhisperManager>();
        DialogueScheduler dialogueScheduler = BootStrap.Instance.GetComponentInChildren<DialogueScheduler>();
        toggle.onValueChanged.AddListener(value => { extendedWhisperManager.ToggleMicrophone(value); if (!value) { text.SetText(""); } });
        dialogueScheduler.AddListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateText();
        Updatetoggles();
    }

    private void Updatetoggles()
    {
        NextButton.interactable = pages > text.pageToDisplay;
        PreviousButton.interactable = pages > 1 && text.pageToDisplay > 1;
    }

    public void SetText(string newText)
    {
        timer = 0;
        dialogue = newText;
        if (newText == null || newText.Replace(" ", "").Equals(""))
        {
            bufferAnimation.PlayAnimation();
        }
        else
        {
            bufferAnimation.StopAnimation();
        }
    }

    private void UpdateText()
    {
        string modifiedText = dialogue;
        pages = text.textInfo.pageCount;

        if (dialogue.Length / letterPerSecond + AfterSeconds > timer) timer += Time.deltaTime;

        if (dialogue.Length > letterPerSecond * timer)
        {
            modifiedText = $"{modifiedText[..(int)(letterPerSecond * timer)]}<color=#FFFFFF00>{modifiedText[(int)(letterPerSecond * timer)..]}</color>";
            //We are setting the text and reading the amount of pages it would give us. This way, we can't browse to the next page before it is visible
            text.text = modifiedText[..(int)(letterPerSecond * timer)];
            try { text.ForceMeshUpdate(); }
            catch { }
            pages = text.textInfo.pageCount;
        }
        text.text = modifiedText;
        if (pages < text.pageToDisplay && pages != 0)
        {
            text.pageToDisplay = text.textInfo.pageCount;
        }
    }

    public bool TextPlaying()
    {
        return dialogue.Length > letterPerSecond * timer + AfterSeconds;
    }

    public void NextPage()
    {
        text.pageToDisplay += 1;
        Updatetoggles();
    }

    public void PreviousPage()
    {
        text.pageToDisplay -= 1;
        Updatetoggles();
    }

    public Toggle GetToggle()
    {
        return toggle;
    }

    public void OnDialoguePlaying(Dialog dialog)
    {
        SetText(dialog.dialog);
    }

    private void OnDestroy()
    {
        dialogueScheduler.RemoveListener(this);
    }

    public void onMicrophoneStartRecording()
    {
        toggle.isOn = true;
    }

    public void onMicrophoneStopRecording(Dialog _)
    {
        toggle.isOn = false;
    }

}
