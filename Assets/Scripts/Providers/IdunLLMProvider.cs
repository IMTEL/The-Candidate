using Alchemy.Inspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/IDUN")]
public class IdunProvider : LLMServiceProvider
{

    [SerializeField] private IdunLLMService service = new();

    [ShowInInspector]
    public string ApiKey {get {return service.getAPIKey();} set { ChangeAndWrite(service.SetAPIKey(value));}}

    public override ILLMService CreateTypedService()
        => service;

    public override OptionSet GetLLMOptions()
    {
        return new OptionSet(service.GetLLMOptions());
    }
}


