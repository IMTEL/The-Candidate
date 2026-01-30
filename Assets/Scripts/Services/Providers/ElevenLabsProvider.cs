using UnityEngine;

[CreateAssetMenu(menuName = "Services/Elevenlabs")]
public class ElevenLabsProvider : ServiceProviderSO<TTService>
{
    [SerializeField] private string apiKey;

    public override TTService CreateTypedService()
        => new ElevenLabsService(apiKey);
}