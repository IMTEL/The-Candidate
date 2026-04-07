using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Alchemy.Inspector;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
[Serializable]
public class NPC : MonoBehaviour, IEvent<INPCEventListener>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [LabelText("ITTService")]
    [SerializeReference] public TTSserviceProvider TTService = null;
    [SerializeReference] private Dictionary<string, object> TTSOptions = new();

    [LabelText("AI Service")]
    [SerializeReference] public LLMServiceProvider LLMService = null;
    [SerializeReference] private Dictionary<string, object> LLMOptions = new();

    public Environment.Actor actor = Environment.Actor.Employee;
    public AudioSource audioSource { get; private set; }
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

    public void SetLLMOptions(IEnumerable<OptionBase> values)
    {
        LLMOptions.Clear();
        foreach (OptionBase item in values)
        {
            LLMOptions[item.GetName()] = item.GetObjectValue();
        }
    }

    public void SetLLMOverrideValue(string name, object value)
    {
        if (LLMOptions.TryGetValue(name, out _))
        {
            LLMOptions[name] = value;
        }
    }

    public void SetTTSOptions(IEnumerable<OptionBase> values)
    {
        TTSOptions.Clear();
        foreach (OptionBase item in values)
        {
            TTSOptions[item.GetName()] = item.GetObjectValue();
        }
    }

    public void SetTTSOverrideValue(string name, object value)
    {
        if (TTSOptions.TryGetValue(name, out _))
        {
            TTSOptions[name] = value;
        }
    }

    public IEnumerable<OptionBase> GetLLMOptions()
    {
        IEnumerable<OptionBase> options = LLMService.GetLLMOptions();
        foreach (OptionBase item in options)
        {
            if (LLMOptions.TryGetValue(item.GetName(), out object overridevalue))
            {
                try
                {
                    item.TrySetValue(overridevalue);
                }
                catch (System.Exception)
                {
                    LLMOptions[item.GetName()] = item.GetObjectValue();
                }
            }
            else
            {
                LLMOptions[item.GetName()] = item.GetObjectValue();
            }
        }
        return options;
    }

    public IEnumerable<OptionBase> GetTTSOptions()
    {
        IEnumerable<OptionBase> options = TTService.GetTTSOptions();
        foreach (OptionBase item in options)
        {
            if (TTSOptions.TryGetValue(item.GetName(), out object overridevalue))
            {
                try
                {
                    item.TrySetValue(overridevalue);
                }
                catch (System.Exception)
                {
                    TTSOptions[item.GetName()] = item.GetObjectValue();
                }
            }
            else
            {
                TTSOptions[item.GetName()] = item.GetObjectValue();
            }
        }
        return options;
    }


    private void OnValidate()
    {
        if (LLMService == null || TTService == null)
        {
            throw new ArgumentException("You need to set the services needed within the NPC component");
        }
        GetLLMOptions();
        GetTTSOptions();
    }
}
