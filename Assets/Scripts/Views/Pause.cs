using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Pause : MonoBehaviour
{
    public Canvas menu;

    public InputActionReference pause;

    bool paused = false;

    [ShowInInspector]
    public Button exit;

    [ShowInInspector]
    public Button resume;


    void Awake()
    {
        pause.action.performed += context => OnPauseClick();
    } 

    public void OnClickQuit()
    {
        SceneManager.LoadScene("Start");
    } 

    public void OnClickResume()
    {
        OnPauseClick();
    }

    void OnPauseClick()
    {
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
        menu.gameObject.SetActive(paused);
    }
}
