
using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using Unity.InferenceEngine;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Piper")]
public class PiperProvider : TTSServiceProvider
{
    [SerializeReference]
    public List<ESpeakVoiceContainer> values = new();

    public override ITTService CreateTypedService()
    {
        return PiperManager.GetInstance();
    }

    public override OptionSet GetTTSOptions()
    {
        OptionSet options = PiperManager.GetTTSOptions();

        List<OptionEntry<ESpeakVoiceContainer>> optionEntries = new();

        foreach (ESpeakVoiceContainer voiceContainer in values)
        {
            if (voiceContainer == null)
                continue;

            optionEntries.Add(
                new OptionEntry<ESpeakVoiceContainer>(
                    voiceContainer,
                    voiceContainer.GetName()
                )
            );
        }

        if (optionEntries.Count == 0)
        {
            Debug.LogError(
                "[PiperProvider] No Piper voices configured! " +
                "Add at least one voice in the Inspector."
            );

            return options;
        }

        options.AddOption(
            new PickOption<ESpeakVoiceContainer>(
                "voice",
                optionEntries,
                optionEntries[0]
            )
        );

        return options;
    }

    [Button]
    public void AddPipervoice(TextAsset textAsset, ModelAsset modelasset)
    {
        if (textAsset == null || modelasset == null)
        {
            Debug.LogError("[PiperProvider] TextAsset or ModelAsset is missing!");
            return;
        }

        values.Add(new ESpeakVoiceContainer(textAsset, modelasset));

        Debug.Log($"[PiperProvider] Voice added. Total voices: {values.Count}");
    }
}
