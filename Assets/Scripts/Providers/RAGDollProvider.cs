using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/RAGDOLL")]
public class RAGDOLLProvider : LLMServiceProvider
{
    [SerializeField] private string apiKey;

    private List<RAGRole> roles = new();

    public override ILLMService CreateTypedService()
        => new RAGDOLLService(apiKey);

    public override IEnumerable<OptionBase> GetLLMOptions()
    {
        return RAGDOLLService.GetLLMOptions();
    }
}

internal class RAGRole
{
    string name;
    string description;

    public RAGRole(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}