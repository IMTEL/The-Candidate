
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// A single instance manager that allows for prompting to a LLM system to fetch a response.
/// Uses the chatgpt structure of request
/// To access, call the GetInstance function on the class
/// </summary>
[Serializable]
public class RAGDOLLService : ILLMService
{

    /// <summary>
    /// A limiter used to only allow a certain amount of requests at the same time.
    /// This is to limit the amount of unnecessary traffic
    /// </summary>
    [NonSerialized]
    private readonly SemaphoreSlim simulationousRequests = new(1);

    [SerializeField]
    private List<OptionEntry<RagAgent>> Models = new();

    [SerializeField]
    [HideInInspector]
    private PickOption<RagAgent> OptionModel = new("Agents", new List<OptionEntry<RagAgent>>() { new(new("", ""), "No models :(") });

    /// <summary>
    /// The url that should be sent to
    /// </summary>
    [SerializeField]
    [HideInInspector]
    private string url;

    [SerializeField]
    [HideInInspector]
    private string apiKey;

    public RAGDOLLService() : this("")
    {

    }

    public RAGDOLLService(string apiKey) : this(apiKey, "https://iplvr.it.ntnu.no")
    { }

    public RAGDOLLService(string apiKey, string url)
    {
        this.apiKey ??= apiKey;
        this.url ??= url += "/backend/api/chat/ask";
    }

    public OptionSet GetLLMOptions()
    {
        return new(new List<OptionBase>()
        {
            OptionModel
        });
    }

    public async Task<LLMResponse> GetAIResponse(IEnumerable<Dialog> dialogs, OptionSet options, LLMToolRegistry lLMToolRegistry = null)
    {
        OptionSet set = GetLLMOptions().Merge(options);
        if (set.ContainsOption(OptionModel, out PickOption<RagAgent> value)) await simulationousRequests.WaitAsync();
        else { return null; }
        RAGDOLLPacket requestBody = new(apiKey, ((RagAgent)value.GetObjectValue()).name, dialogs);
        UnityWebRequest request = new(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JObject.FromObject(requestBody).ToString())),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        await request.SendWebRequest();
        simulationousRequests.Release();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            return null;
        }

        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        JToken response = json["response"];
        List<Action> actions = new();
        if (lLMToolRegistry != null)
        {
            foreach (JToken item in response["function_calls"])
            {
                Dictionary<string, string> arguments = item["arguments"].ToObject<Dictionary<string, string>>();
                if(lLMToolRegistry.IsMethodCallable(item["name"].ToString(), arguments, out Action callable))
                {
                    actions.Add(callable);
                }
            }
        }

        return new LLMResponse(response["response"].ToString(), actions);
    }


    public async Task SetAPIKey(string key)
    {
        this.apiKey = key;
        await RefreshModels();
        UpdateModelOption();
    }
    public string GetAPIKey()
    {
        return apiKey;
    }

    public async Task SetURL(string url)
    {
        this.url = url.EndsWith("/") ? url[..url.Length] : url;
        await RefreshModels();
        UpdateModelOption();
    }
    public string GetURL()
    {
        return url;
    }

    public async Task RefreshModels()
    {
        Models.Clear();
        UnityWebRequest request = CreateRequest("/agent-info-by-accesskey");
        await request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("API key or the service is not accessible");
            return;
        }
        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        JToken roles = json["roles"];
        string id = json["agent_id"].ToString();
        foreach (JToken role in roles)
        {
            string name = role["name"].ToString();
            Models.Add(new(new(id, name), name));
        }
    }

    private UnityWebRequest CreateRequest(string path)
    {
        UnityWebRequest request = new(url + "/backend" + path)
        {
            downloadHandler = new DownloadHandlerBuffer(),
        };
        request.SetRequestHeader("access-key", $"{apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private void UpdateModelOption()
    {

        if (Models.Count == 0)
        {
            Models = new() { new(new("", ""), "No Models :(") };
        }
        OptionModel = new("Model", Models);
    }

    [Serializable]
    public struct RagAgent
    {
        public string id;
        public string name;

        public RagAgent(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            if(obj is RagAgent ragAgent)
            {
                return id.Equals(ragAgent.id) && name.Equals(ragAgent.name);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, name);
        }
    }

    internal class RAGDOLLPacket
    {
        string access_key;
        IEnumerable<RagDialogue> chat_log;
        string agent_id;
        string active_role_id = "user";

        public RAGDOLLPacket(string access_key, string agent_id, IEnumerable<RagDialogue> ragDialogues)
        {
            this.access_key = access_key;
            this.agent_id = agent_id;
            chat_log = ragDialogues;
        }

        public RAGDOLLPacket(string access_key, string agent_id, IEnumerable<Dialog> Dialogues)
            : this(access_key, agent_id, RagDialogue.FromDialogues(Dialogues)) { }

    }

    internal class RagDialogue
    {
        string role;
        string content;

        public RagDialogue(Dialog dialog)
        {
            role = dialog.actor.getName();
            content = dialog.dialog;
        }

        static public IEnumerable<RagDialogue> FromDialogues(IEnumerable<Dialog> dialogs)
        {
            List<RagDialogue> ragDialogues = new();
            foreach (Dialog dialog in dialogs)
            {
                ragDialogues.Add(new(dialog));
            }
            return ragDialogues;
        }
    }
}

