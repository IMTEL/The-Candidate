using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/IDUN")]
public class IdunProvider : LLMServiceProvider
{
    [SerializeField] private string apiKey;

    public override ILLMService CreateTypedService()
        => new IdunLLMService(apiKey);

    public override IEnumerable<OptionBase> GetLLMOptions()
    {
        return IdunLLMService.GetLLMOptions();
    }
}


