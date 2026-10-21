using Alchemy.Inspector;
using UnityEngine;

/// <summary>
/// Provider for the interviewers published to The Candidate from the agent configurator.
/// Give it to the interviewer NPC (see SpawnManager) and to the interviewer picker.
/// </summary>
[CreateAssetMenu(menuName = "Services/RAGDOLL Published")]
public class PublishedRagProvider : LLMServiceProvider
{
    #if UNITY_EDITOR
    [ShowInInspector]
    public string ApiKey {get {return service.GetAPIKey();} set {service.SetAPIKey(value); _ = ChangeAndWrite(System.Threading.Tasks.Task.CompletedTask);}}

    [ShowInInspector]
    public string Url {get {return service.GetURL();} set {service.SetURL(value); _ = ChangeAndWrite(System.Threading.Tasks.Task.CompletedTask);}}
    #endif

    [SerializeField]
    [HideInInspector]
    private PublishedRagService service = new();

    public override ILLMService CreateTypedService()
        => service;

    public PublishedRagService GetService()
        => service;

    public override OptionSet GetLLMOptions()
        => service.GetLLMOptions();
}
