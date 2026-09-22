using Alchemy.Inspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/RAGDOLL")]
public class RAGDOLLProvider : LLMServiceProvider
{
    #if UNITY_EDITOR
    [ShowInInspector]
    public string ApiKey {get {return service.GetAPIKey();} set {ChangeAndWrite(service.SetAPIKey(value));}}

    [ShowInInspector]
    public string Url {get {return service.GetURL();} set {ChangeAndWrite(service.SetURL(value));}}

    #endif
    [SerializeField]
    [HideInInspector]
    private RAGDOLLService service = new();

    public override ILLMService CreateTypedService()
        => service;

    public override OptionSet GetLLMOptions()
    {
        OptionSet set = new(service.GetLLMOptions());
        return set;
    }
}

