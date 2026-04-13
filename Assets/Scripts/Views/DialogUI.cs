using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour
{

    public float letterPerSecond = 8;
    private float timer = 0;
    private string dialogue = "";
    public Button button;
    public TMP_Text text;

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
    }

    public void SetText(string newText)
    {
        timer = 0;
        dialogue = newText;
    }

    private void UpdateText()
    {
        string modifiedText = dialogue;
        
        if(dialogue.Length > letterPerSecond * timer) timer += Time.deltaTime;
        
        if(dialogue.Length > letterPerSecond * timer)
        {
            modifiedText = modifiedText[..(int)(letterPerSecond * timer)];
        }
        text.text = modifiedText;   
    }

    public Button GetButton()
    {
        return button;
    }
}
