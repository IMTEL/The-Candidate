using System;
using UnityEngine;

[Serializable]
public class Agent 
{
    [SerializeField]
    LLMServiceProvider LLMServiceProvider;
    
    [SerializeReference]
    OptionSet LLMOverrides = new();

    [SerializeField]
    TTSServiceProvider TTServiceProvider;
    
    [SerializeReference]
    OptionSet TTSOVerrides = new();

    public bool GetLLMSettings(out LLMServiceProvider provider, out OptionSet options, bool CombineOptions = true)
    {
        provider = LLMServiceProvider;
        options = CombineOptions && LLMServiceProvider != null? LLMServiceProvider.GetLLMOptions().MergeOnTop(LLMOverrides) : LLMOverrides;
        return LLMServiceProvider != null;
    }
    public bool GetTTSSettings(out TTSServiceProvider provider, out OptionSet options, bool CombineOptions = true)
    {
        provider = TTServiceProvider;
        options = CombineOptions && TTServiceProvider != null ? TTServiceProvider.GetTTSOptions().MergeOnTop(TTSOVerrides) : TTSOVerrides;
        return TTServiceProvider != null;
    }

    public void SetLLMServiceProvider(LLMServiceProvider lLMService)
    {
        LLMServiceProvider = lLMService;
        LLMOverrides = lLMService != null ? lLMService.GetLLMOptions().MergeOnTop(LLMOverrides, OptionSet.MergeMode.Intersect) : new();
    }

    public void SetTTSServiceProvider(TTSServiceProvider tTSService)
    {
        TTServiceProvider = tTSService;
        TTSOVerrides = TTServiceProvider != null ? TTServiceProvider.GetTTSOptions().MergeOnTop(TTSOVerrides, OptionSet.MergeMode.Intersect) : new();
    }

    public OptionSet SetOptionsLLM(OptionSet optionSet)
    {
        if(LLMServiceProvider != null)
        {
            LLMOverrides = LLMServiceProvider.GetLLMOptions().MergeOnTop(optionSet, OptionSet.MergeMode.Intersect);
        }
        return LLMOverrides;
    }
    public OptionSet SetOptionsTTS(OptionSet optionSet)
    {
        if(TTServiceProvider != null)
        {
            TTSOVerrides = TTServiceProvider.GetTTSOptions().MergeOnTop(optionSet, OptionSet.MergeMode.Intersect);
        }
        return TTSOVerrides;
    }
}