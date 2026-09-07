using System;
using UnityEngine;

[Serializable]
public class Agent
{
    [SerializeReference]
    LLMServiceProvider LLMServiceProvider;
    OptionSet LLMOverrides;

    [SerializeReference]
    TTSServiceProvider TTServiceProvider;
    OptionSet TTSOVerrides;

    public bool GetLLMSettings(out LLMServiceProvider provider, out OptionSet options)
    {
        provider = LLMServiceProvider;
        options = LLMOverrides;
        return LLMServiceProvider != null;
    }
    public bool GetLLMSettings(out TTSServiceProvider provider, out OptionSet options)
    {
        provider = TTServiceProvider;
        options = TTSOVerrides;
        return TTServiceProvider != null;
    }

    public void SetLLMServiceProvider(LLMServiceProvider lLMService)
    {
        LLMServiceProvider = lLMService;
        LLMOverrides = lLMService != null ? lLMService.GetLLMOptions() : new();
    }

    public void SetTTSServiceProvider(TTSServiceProvider tTSService)
    {
        TTServiceProvider = tTSService;
        TTSOVerrides = tTSService != null ? tTSService.GetTTSOptions() : new();
    }


}