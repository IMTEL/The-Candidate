using UnityEngine;

[CreateAssetMenu(menuName = "Services/Elevenlabs")]
public class ElevenLabsProvider : TTSserviceProvider
{
    [SerializeField] private string apiKey;

    public override TTService CreateTypedService()
        => new ElevenLabsService(apiKey, voiceproviderids);
}