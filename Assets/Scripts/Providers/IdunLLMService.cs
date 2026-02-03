using UnityEngine;

[CreateAssetMenu(menuName = "Services/IDUN")]
public class OpenAiLlmProvider : ServiceProviderSO<LLMService>
{
    [SerializeField] private string apiKey;

    public override LLMService CreateTypedService()
        => new IdunLLMService(apiKey);
}


