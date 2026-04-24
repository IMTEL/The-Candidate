using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class NPC : MonoBehaviour, IEvent<INPCEventListener>
{

    public new string name;

    [LabelText("TTS Service")]
    [SerializeReference] public TTSserviceProvider TTService = null;
    [SerializeReference] private List<OptionBase> TTSOptions = new();

    [LabelText("AI Service")]
    [SerializeReference] public LLMServiceProvider LLMService = null;
    [SerializeReference] private List<OptionBase> LLMOptions = new();

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

    public void AddListener(INPCEventListener listener)
    {
        onNPCDestroy += listener.OnDestroyNPC;
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
            SetLLMOverrideValue(item);
        }
    }

    public void SetLLMOverrideValue(OptionBase optionBase)
    {
        OptionBase option = LLMOptions.Find(option => option.Equals(optionBase));
        if (option != null)
        {
            option.TrySetValue(optionBase.GetObjectValue());
        }
        else
        {
            LLMOptions.Add(optionBase);
        }
    }

    public void SetTTSOptions(IEnumerable<OptionBase> values)
    {
        TTSOptions.Clear();
        foreach (OptionBase item in values)
        {
            SetTTSOverrideValue(item);
        }
    }

    public void SetTTSOverrideValue(OptionBase optionBase)
    {
        OptionBase option = TTSOptions.Find(option => option.Equals(optionBase));
        if (option != null)
        {
            option.TrySetValue(optionBase.GetObjectValue());
        }
        else
        {
            TTSOptions.Add(optionBase);
        }
    }

    public IEnumerable<OptionBase> GetLLMOptions()
    {
        IEnumerable<OptionBase> baseline = new List<OptionBase>();
        if (LLMService != null)
        {
            baseline = LLMService.GetLLMOptions();
        }
        LLMOptions = CompareAndGetOptions(LLMOptions, baseline);
        return LLMOptions;
    }

    public IEnumerable<OptionBase> GetTTSOptions()
    {
        IEnumerable<OptionBase> baseline = new List<OptionBase>();
        if (TTService != null)
        {
            baseline = TTService.GetTTSOptions();
        }
        TTSOptions = CompareAndGetOptions(TTSOptions, baseline);
        return TTSOptions;
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


    private void OnValidate()
    {
        if (LLMService == null)
        {
            Debug.LogError("You need to set minium the LLM service");
        }
        GetLLMOptions();
        GetTTSOptions();
    }
}
