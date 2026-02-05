using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour
{

    public int letterPerSecond = 3;
    private float timer;
    private string dialogue;
    private Button button;
    private Text text;


    private void Awake()
    {
        if(button == null) button = GetComponentInChildren<Button>();
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
