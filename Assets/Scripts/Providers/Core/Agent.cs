using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro.EditorUtilities;
using UnityEngine;

/// <summary>
/// Agent class that is used for containing the providers and settings for a specific agent
/// </summary>
[Serializable]
public class Agent
{
    /// <summary>
    /// Provider used to LLMs
    /// </summary>
    [SerializeField]
    LLMServiceProvider LLMServiceProvider;

    /// <summary>
    /// LLM override settings
    /// </summary>
    [SerializeReference]
    OptionSet LLMOverrides = new();

    /// <summary>
    /// Provider user for TTS  
    /// </summary>
    [SerializeField]
    TTSServiceProvider TTServiceProvider;

    /// <summary>
    /// TTS override settings
    /// </summary>
    [SerializeReference]
    OptionSet TTSOverrides = new();

    [SerializeField]
    LLMToolRegistry lLMToolRegistry;

    /// <summary>
    /// Initializes the methods for the agent to be used 
    /// </summary>
    /// <param name="owner">The object that has the tools</param>
    /// <param name="name">Role name for the filter</param>
    /// <exception cref="ArgumentException">If the owner is null</exception>
    public void Initialize(object owner, string name = "")
    {
        if (owner == null)
        {
            throw new ArgumentException(nameof(owner));
        }
        lLMToolRegistry = new(owner, name);
    }

    /// <summary>
    /// Gives out the provider and options for the LLM
    /// </summary>
    /// <param name="provider">The provider to be used</param>
    /// <param name="options">The options that are saved. Changed by CombineOptions. </param>
    /// <param name="CombineOptions">Bool if the options should be merged with the already existing on the LLMProvider ś</param>
    /// <returns>If there is a provider or options to be given</returns>
    public bool GetLLMSettings(out LLMServiceProvider provider, out OptionSet options, bool CombineOptions = true)
    {
        provider = LLMServiceProvider;
        options = CombineOptions && LLMServiceProvider != null ? LLMServiceProvider.GetLLMOptions().Merge(LLMOverrides) : LLMOverrides;
        return LLMServiceProvider != null;
    }

    /// <summary>
    /// Gives out the provider and options for the LLM
    /// </summary>
    /// <param name="provider">The provider to be used</param>
    /// <param name="options">The options that are saved. Changed by CombineOptions. </param>
    /// <param name="CombineOptions">Bool if the options should be merged with the already existing on the LLMProvider ś</param>
    /// <returns>If there is a provider or options to be given</returns>
    public bool GetTTSSettings(out TTSServiceProvider provider, out OptionSet options, bool CombineOptions = true)
    {
        provider = TTServiceProvider;
        options = CombineOptions && TTServiceProvider != null ? TTServiceProvider.GetTTSOptions().Merge(TTSOverrides) : TTSOverrides;
        return TTServiceProvider != null;
    }

    /// <summary>
    /// Sets the provider LLM on the agent. Will overwrite the options to match and save options that are identical
    /// </summary>
    /// <param name="lLMService">A LLMService to be used</param>
    public void SetLLMServiceProvider(LLMServiceProvider lLMService)
    {
        LLMServiceProvider = lLMService;
        LLMOverrides = lLMService != null ? lLMService.GetLLMOptions().Merge(LLMOverrides, OptionSet.MergeMode.Intersect) : new();
    }

    /// <summary>
    /// Sets the TTS provider on the Agent. Will overwrite the options to match provider and 
    /// </summary>
    /// <param name="tTSService"></param>
    public void SetTTSServiceProvider(TTSServiceProvider tTSService)
    {
        TTServiceProvider = tTSService;
        TTSOverrides = TTServiceProvider != null ? TTServiceProvider.GetTTSOptions().Merge(TTSOverrides, OptionSet.MergeMode.Intersect) : new();
    }

    public OptionSet SetOptionsLLM(OptionSet optionSet)
    {
        if (LLMServiceProvider != null)
        {
            LLMOverrides = LLMServiceProvider.GetLLMOptions().Merge(optionSet, OptionSet.MergeMode.Intersect);
        }
        return LLMOverrides;
    }
    public OptionSet SetOptionsTTS(OptionSet optionSet)
    {
        if (TTServiceProvider != null)
        {
            TTSOverrides = TTServiceProvider.GetTTSOptions().Merge(optionSet, OptionSet.MergeMode.Intersect);
        }
        return TTSOverrides;
    }



    /// <summary>
    /// Sends request to the LLM with the agent settings
    /// </summary>
    /// <param name="dialogs">The list of dialogs</param>
    /// <returns>The LLMResponse</returns>
    public Task<LLMResponse> CallLLM(IEnumerable<LLMDialogue> dialogs)
    {
        if (LLMServiceProvider != null)
        {
            return LLMServiceProvider.CreateTypedService().GetAIResponse(dialogs, LLMOverrides, lLMToolRegistry);
        }
        return null;
    }

    public IAsyncEnumerator<AudioClip> CallTTS(string dialog)
    {
        if (TTServiceProvider != null)
        {
            return TTServiceProvider.CreateTypedService().getAudioClip(dialog, TTSOverrides);
        }
        return null;
    }
}