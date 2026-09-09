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
        => PiperManager.GetInstance();

    public override OptionSet GetTTSOptions()
    {
        OptionSet options = new(PiperManager.GetTTSOptions());
        List<OptionEntry<ESpeakVoiceContainer>> optionEntries = new();
        foreach(ESpeakVoiceContainer voiceContainer in values)
        {
            optionEntries.Add(new(voiceContainer, voiceContainer.GetName()));
        } 
        options.AddOption(new PickOption<ESpeakVoiceContainer>("voice", optionEntries,optionEntries[0] ));
        return options;
    }

    [Button]
    public void addPipervoice(TextAsset textAsset, ModelAsset modelasset)
    {
        values.Add(new ESpeakVoiceContainer(textAsset, modelasset));
    }
}


