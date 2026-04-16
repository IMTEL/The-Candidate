using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour
{

    public float letterPerSecond = 8;
    public float AfterSeconds = 2;
    private float timer = 0;
    private string dialogue = "";
    public Button button;
    public TMP_Text text;

    private int pages;

    public Button NextButton;
    public Button PreviousButton;

    public Color OnrecordColor;
    public Color OnRecordHighlightColor;
    public Color OnRecordselectedColor;

    ColorBlock buttonColorDefualt = new();

    ColorBlock OnRecord = new();

    private void Awake()
    {
        button = button != null ? button : GetComponentInChildren<Button>();
        text = text != null ? text : GetComponentInChildren<TMP_Text>();
        buttonColorDefualt = button.colors;
        OnRecord = button.colors;
        OnRecord.normalColor = OnrecordColor;
        OnRecord.highlightedColor = OnRecordHighlightColor;
        OnRecord.selectedColor = OnRecordselectedColor;
    }

    private void Start()
    {
        ExtendedWhisperManager extendedWhisperManager = BootStrap.Instance.GetComponentInChildren<ExtendedWhisperManager>();
        button.onClick.AddListener(() =>
        {
            if (extendedWhisperManager.ToggleMicrophone())
            {
                button.colors = OnRecord;
            }
            else
            {
                button.colors = buttonColorDefualt;    
            }
            ;});
    } 

    // Update is called once per frame
    void Update()
    {
        UpdateText();
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        NextButton.interactable = pages > text.pageToDisplay;
        PreviousButton.interactable = pages > 1 && text.pageToDisplay > 1;
    }

    public void SetText(string newText)
    {
        timer = 0;
        dialogue = newText;
    }

    private void UpdateText()
    {
        string modifiedText = dialogue;
        pages = text.textInfo.pageCount;
        
        if(dialogue.Length/letterPerSecond + AfterSeconds > timer) timer += Time.deltaTime;
        
        if(dialogue.Length > letterPerSecond * timer)
        {
            modifiedText = $"{modifiedText[..(int)(letterPerSecond * timer)]}<color=#FFFFFF00>{modifiedText[(int)(letterPerSecond * timer)..]} </color>" ;
            //We are setting the text and reading the amount of pages it would give us. This way, we can't browse to the next page before it is visible
            text.text = modifiedText[..(int)(letterPerSecond * timer)];
            text.ForceMeshUpdate();
            pages = text.textInfo.pageCount;
        }
        text.text = modifiedText;
        if(pages < text.pageToDisplay && pages != 0)
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
        UpdateButtons();
    }

    public void PreviousPage()
    {
        text.pageToDisplay -= 1;
        UpdateButtons();
    }

    public Button GetButton()
    {
        return button;
    }
}
