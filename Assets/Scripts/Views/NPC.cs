using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class NPC : MonoBehaviour, IEvent<INPCEventListener>, IMicrophoneEventListener
{
    private static readonly int BlinkingBlinkHash = Animator.StringToHash("Blinking.Blink");
    private static readonly int NoddingHash = Animator.StringToHash("Actions.Nodding");

    private bool recording = false;
    
    public new string name;

    public GameObject head;

    [SerializeField]
    private Agent agent;
    
    private System.Random random = new();

    public Environment.Actor actor = Environment.Actor.Employee;
    public AudioSource audioSource { get; private set; }

    private float timeForBlink;
    private float timeForNodding;

    public Action<NPC> onNPCDestroy;
    
    private Animator animator;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        agent.Initialize(this);
        animator = transform.parent.GetComponentInChildren<Animator>();
        timeForBlink = random.Next(1, 3);
    }
    void Start()
    {
        BootStrap.Instance.GetComponentInChildren<ExtendedWhisperManager>().AddListener(this);
    }

    void Update()
    {
        audioSource.transform.position = head.transform.position;
        timeForBlink -= Time.deltaTime;
        timeForNodding -= Time.deltaTime;
        if(timeForBlink <= 0){
            animator.Play(BlinkingBlinkHash);
            timeForBlink = random.Next(1, 3);
        }
        if (recording && timeForNodding <= 0)
        {
            animator.Play(NoddingHash);
            timeForNodding = random.Next(1, 5);
        }
    }

    public void Talk(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void AddListener(INPCEventListener listener)
    {
        onNPCDestroy += listener.OnDestroyNPC;
    }

    public void RemoveListener(INPCEventListener listener)
    {
        onNPCDestroy -= listener.OnDestroyNPC;
    }

    public List<OptionBase> CompareAndGetOptions(List<OptionBase> localOptionBases, IEnumerable<OptionBase> options)
    {
        List<OptionBase> optionBases = new();
        foreach (OptionBase item in options)
        {
            OptionBase optionBase = localOptionBases.Find(option => option.Equals(item));
            if (optionBase != null)
            {
                try
                {
                    item.TrySetValue(optionBase.GetObjectValue());
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            optionBases.Add(item);
        }
        return optionBases;
    }

    public Agent GetAgent()
    {
        return agent;
    }

    public void onMicrophoneStartRecording()
    {
        audioSource.Stop();
        audioSource.clip = null;
        timeForNodding = random.Next(1, 7);
        recording = true;
    }

    public void onMicrophoneStopRecording(Dialog recordedaudio)
    {
        recording = false;
    }

    private void OnDestroy()
    {
        onNPCDestroy?.Invoke(this);
        BootStrap.Instance.GetComponentInChildren<ExtendedWhisperManager>().RemoveListener(this);
    }

}
