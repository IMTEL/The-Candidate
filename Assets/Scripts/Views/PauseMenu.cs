using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour, IEvent<IPauseMenuEventListener>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button ResumeButton;
    public Button SettingsButton;
    public Button ExitButton;

    void OnValidate()
    {
        if(ResumeButton == null)
        {
            Debug.LogError("ResumeButton is missing");
        }
        if(SettingsButton == null)
        {
            Debug.LogError("SettingsButton is missing");
        }
        if(ExitButton == null)
        {
            Debug.LogError("ExitButton is missing");
        }
    }

    public void AddListener(IPauseMenuEventListener listener)
    {
        ResumeButton.onClick.AddListener(listener.OnResumeButton);
        SettingsButton.onClick.AddListener(listener.OnSettingButton);
        ExitButton.onClick.AddListener(listener.OnExitButton);
    }

    public void RemoveListener(IPauseMenuEventListener listener)
    {
        ResumeButton.onClick.RemoveListener(listener.OnResumeButton);
        SettingsButton.onClick.RemoveListener(listener.OnSettingButton);
        ExitButton.onClick.RemoveListener(listener.OnExitButton);
    }
}
