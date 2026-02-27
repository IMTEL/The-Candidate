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
    [SerializeField] private TTSserviceProvider TTService = null;
    [SerializeReference] private List<OptionBase> TTSOptions = new(); 

    [LabelText("AI Service")]
    [SerializeField]private LLMServiceProvider LLMService = null;
    [SerializeReference]private List<OptionBase> LLMOptions = new();

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

    public void SetLLMOptions(IEnumerable<OptionBase> values)
    {
        LLMOptions = new(values);
    }
    
    public void SetTTSOptions(IEnumerable<OptionBase> values)
    {
        TTSOptions = new(values);
    }

    public IEnumerable<OptionBase> GetLLMOptions()
    {
        return LLMOptions;
    }
    
    public IEnumerable<OptionBase> GetTTSOptions()
    {
        return TTSOptions;
    }


    private void OnValidate()
    {
        if(LLMService == null || TTService == null)
        {
            throw new ArgumentException("You need to set the services needed within the NPC component");
        }
    }
}
