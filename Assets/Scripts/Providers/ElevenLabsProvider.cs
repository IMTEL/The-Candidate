using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Elevenlabs")]
public class ElevenLabsProvider : TTSServiceProvider
{
    [ShowInInspector]
    public string APIKey { get { return instance.GetAPIKey(); } set { instance.SetAPIKey(value); } }

    private List<VoiceProfile> voices = new();

    [SerializeField]
    private ElevenLabsService instance = new();

    public override ITTService CreateTypedService()
        => instance;


    public override OptionSet GetTTSOptions()
    {
        return new OptionSet(ElevenLabsService.GetTTSOptions());
    }


}