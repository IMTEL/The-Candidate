using UnityEngine;

[CreateAssetMenu(menuName = "Services/OpenAI LLM")]
public class OpenAiLlmProvider : ServiceProviderSO<LLMService>
{
    [SerializeField] private string apiKey;

    public override LLMService CreateTypedService()
        => new IdunLLMService(apiKey);
}


