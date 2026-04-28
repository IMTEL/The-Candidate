using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour, IPauseMenuEventListener, IEvent<IPause>
{
    public List<GameObject> pauseMenues;

    private readonly List<GameObject> instatiesMenues = new();
    public List<GameObject> DeactiveOnPause;

    public InputActionReference pause;

    private Action<bool> pauseListeners;

    bool paused = false;


    void Awake()
    {
        foreach (GameObject item in pauseMenues)
        {
            GameObject menu = Instantiate(item, transform, false);
            menu.transform.GetLocalPositionAndRotation(out Vector3 location, out Quaternion rotation);
            int qoutent = Math.DivRem(instatiesMenues.Count, 2, out int rem);
            float xchange = qoutent;
            float zchange = qoutent;
            if (rem == 1)
            {
                xchange += 1;
                zchange += 1;
                xchange *= -1;
                zchange *= -1;
            }
            float rotationamount = (qoutent+rem) * (rem == 0 ? 1 : -1)/2f;
            xchange *= (transform.localPosition.z+0.5f) / 2f * (float)Math.Cos(rotationamount);
            zchange *= (transform.localPosition.z+0.5f) / 2f * -(float)Math.Sin(rotationamount);
            location.x += xchange;
            location.z += zchange;
            rotation.y += rotationamount;
            if (menu.TryGetComponent(out RectTransform reachTrans))
            {
                reachTrans.SetLocalPositionAndRotation(location, rotation);
            }
            else
            {
                menu.transform.SetLocalPositionAndRotation(location, rotation);
            }
            menu.SetActive(false);
            if(menu.TryGetComponent(out PauseMenu pauseMenu))
            {
                pauseMenu.AddListener(this);
            }
            instatiesMenues.Add(menu);
        }
        pause.action.performed += OnPauseClick;
       
    }
    void Start()
    {
        this.AddListener(BootStrap.Instance.GetComponentInChildren<DialogueScheduler>());
    }

    void OnPauseClick(InputAction.CallbackContext action)
    {
       OnPauseClick();
    }

    void OnPauseClick()
    {
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
        instatiesMenues.ForEach(menu => menu.SetActive(paused));
        DeactiveOnPause.ForEach(menu => menu.SetActive(!paused));
        pauseListeners.Invoke(paused);
    }

    void OnDestroy()
    {
        pause.action.performed -= OnPauseClick;        
    }

    public void OnResumeButton()
    {
        OnPauseClick();
    }

    public void OnExitButton()
    {
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
        SceneManager.LoadScene("Start");
    }

    public void OnSettingButton()
    {
        throw new NotImplementedException();
    }

    public void AddListener(IPause listener)
    {
        pauseListeners += listener.OnPauseClick;
    }

    public void RemoveListener(IPause listener)
    {
         pauseListeners -= listener.OnPauseClick;
    }
}

    public interface IPause
    {
        void OnPauseClick(bool paused);
    }