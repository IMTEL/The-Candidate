using OVR.OpenVR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour
{

    public int letterPerSecond = 3;
    private float timer = 0;
    private string dialogue = "";
    public Button button;
    public TMP_Text text;


    private void Awake()
    {
        button ??= GetComponentInChildren<Button>();
        text ??= GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        ExtendedWhisperManager extendedWhisperManager = BootStrap.Instance.GetComponentInChildren<ExtendedWhisperManager>();
        button.onClick.AddListener(() => extendedWhisperManager.ToggleMicrophone());
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
        
        if(dialogue.Length < letterPerSecond * timer) timer += Time.deltaTime;
        
        if(dialogue.Length < letterPerSecond * timer)
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
