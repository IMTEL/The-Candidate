using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Elevenlabs")]
public class ElevenLabsProvider : TTSserviceProvider
{
    [SerializeField] private string apiKey;

    public override TTService CreateTypedService()
        => new ElevenLabsService(apiKey, voiceproviderids);

    public override IEnumerable<OptionBase> GetTTSOptions()
    {
        return ElevenLabsService.GetTTSOptions();
    }
}