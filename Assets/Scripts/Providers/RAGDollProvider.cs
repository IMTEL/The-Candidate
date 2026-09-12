using System;
using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Services/RAGDOLL")]
public class RAGDOLLProvider : LLMServiceProvider
{
    [SerializeField] private string apiKey;
    [SerializeField] private string url = "https://iplvr.it.ntnu.no/";
    private RAGDOLLService service = new();

    [Unity.Collections.ReadOnly]
    private readonly List<RAGRole> roles = new();

    public override ILLMService CreateTypedService()
        => service;

    public override OptionSet GetLLMOptions()
    {
        OptionSet set = new(service.GetLLMOptions());
        return set;
    }
    
    [Button]
    private async void RefreshRoles(string apiKey)
    {
        Debug.Log($"Trying to fetch roles with {apiKey}");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentException("The apikey needs to be set for the settings to be correct");
        }

        string url = this.url + "/backend/agent-info-by-accesskey";
        UnityWebRequest request = new(url)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("access-key", apiKey);
        await request.SendWebRequest();
        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        JArray roles = (JArray)json["roles"];
        this.roles.Clear();
        foreach (JObject role in roles.Cast<JObject>())
        {
            RAGRole castRole = new(role["name"].ToString(), role["description"].ToString());
            this.roles.Add(castRole);
        }
        
    }

    private void OnValidate()
    {
        url = url.EndsWith("/") ? url = url[..url.Length] : url;
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

        public string GetName()
        {
            return name;
        }

        public string getDescription()
        {
            return description;
        }

        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            RAGRole castObject = (RAGRole)obj;

            return name.Equals(castObject.name) && description.Equals(castObject.description) 
                    && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, description);
        }
    }
}

