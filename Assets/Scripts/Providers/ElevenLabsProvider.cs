using Alchemy.Inspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Elevenlabs")]
public class ElevenLabsProvider : TTSServiceProvider
{
    #if UNITY_EDITOR
    
    [ShowInInspector]
    public string APIKey { get { return instance.GetAPIKey(); } set { _ = ChangeAndWrite(instance.SetAPIKey(value)); } }
    
    #endif

    [SerializeField]
    private ElevenLabsService instance = new();

    public override ITTService CreateTypedService()
        => instance;


    public override OptionSet GetTTSOptions()
    {
        return new OptionSet(instance.GetTTSOptions());
    }


}