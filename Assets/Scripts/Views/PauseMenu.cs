using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour, IEvent<IPauseMenuEventListener>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button ResumeButton;
    public Button SettingsButton;
    public Button ExitButton;
    /// <summary>
    /// Ends the interview and shows the observer's feedback. Hidden where there is no interview.
    /// </summary>
    public Button EndInterviewButton;

    void OnValidate()
    {
        if(EndInterviewButton == null)
        {
            Debug.LogError("EndInterviewButton is missing");
        }
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
        EndInterviewButton.onClick.AddListener(listener.OnEndInterviewButton);
    }

    public void RemoveListener(IPauseMenuEventListener listener)
    {
        ResumeButton.onClick.RemoveListener(listener.OnResumeButton);
        SettingsButton.onClick.RemoveListener(listener.OnSettingButton);
        ExitButton.onClick.RemoveListener(listener.OnExitButton);
        EndInterviewButton.onClick.RemoveListener(listener.OnEndInterviewButton);
    }
}
