using UnityEngine;

[CreateAssetMenu(menuName = "Services/IDUN")]
public class IdunProvider : LLMServiceProvider
{
    [SerializeField] private string apiKey;

    public override ILLMService CreateTypedService()
        => new IdunLLMService(apiKey);

    public override OptionSet GetLLMOptions()
    {
        return new OptionSet(IdunLLMService.GetLLMOptions());
    }
}


