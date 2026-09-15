using Alchemy.Inspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/RAGDOLL")]
public class RAGDOLLProvider : LLMServiceProvider
{
    [ShowInInspector]
    public string ApiKey {get {return service.GetAPIKey();} set {service.SetAPIKey(value);}}

    [ShowInInspector]
    public string url {get {return service.GetURL();} set {service.SetURL(value);}}
    [SerializeField] private RAGDOLLService service = new();

    public override ILLMService CreateTypedService()
        => service;

    public override OptionSet GetLLMOptions()
    {
        OptionSet set = new(service.GetLLMOptions());
        return set;
    }
}

