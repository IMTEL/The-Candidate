using Alchemy.Inspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/IDUN")]
public class IdunProvider : LLMServiceProvider
{

    [SerializeField] private IdunLLMService service = new();

    #if UNITY_EDITOR
    [ShowInInspector]
    public string ApiKey {get {return service.getAPIKey();} set { _ = ChangeAndWrite(service.SetAPIKey(value)); }}
    #endif
    
    public override ILLMService CreateTypedService()
        => service;

    public override OptionSet GetLLMOptions()
    {
        return new OptionSet(service.GetLLMOptions());
    }
}


