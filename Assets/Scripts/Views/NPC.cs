using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
[Serializable]
public class NPC : MonoBehaviour, IEvent<INPCEventListener>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [LabelText("TTService")]
    [SerializeField] public ServiceProviderSO<TTService> TTService = null;

    [LabelText("AI Service")]
    [SerializeField ]public ServiceProviderSO<LLMService> LLMService = null;

    public Environment.Actor actor = Environment.Actor.Employee;
    public AudioSource audioSource {get; private set;}
    public GameObject head;

    public VoiceProfile voiceProfile;

    public Action<NPC> onNPCDestroy;




    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        audioSource.transform.position = head.transform.position;
    }

    public void Talk(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void OnDestroy()
    {
        onNPCDestroy?.Invoke(this);
    }

    public void AddListener(IEnumerable<INPCEventListener> listeners)
    {
        foreach (INPCEventListener item in listeners)
        {
            AddListener(item);
        }
    }

    public void AddListener(INPCEventListener listener)
    {
        onNPCDestroy += listener.OnDestroyNPC;
    }

    public void RemoveListener(IEnumerable<INPCEventListener> listeners)
    {
        foreach (INPCEventListener item in listeners)
        {
            AddListener(item);
        }
    }

    public void RemoveListener(INPCEventListener listener)
    {
        onNPCDestroy -= listener.OnDestroyNPC;
    }

    void OnGUI()
    {
    }

    private void OnValidate()
    {
        if(LLMService == null || TTService == null)
        {
            throw new ArgumentException("You need to set the services needed within the DialogSystemComponent");
        }
    }
}
