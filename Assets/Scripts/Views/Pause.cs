using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public Canvas menu;

    public InputActionReference pause;

    bool paused = false;


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
