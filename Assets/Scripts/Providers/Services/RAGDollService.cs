
using System;
using System.Collections.Generic;
using System.Linq;
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
public class RAGDOLLService : ILLMService
{

    /// <summary>
    /// A limiter used to only allow a certain amount of requests at the same time.
    /// This is to limit the amount of unnecessary traffic
    /// </summary>
    private readonly SemaphoreSlim simulationousRequests = new(1);

    private PickOption<string> pickOption = new("Agents", new List<OptionEntry<string>>(){new("hello", "hel")});

    /// <summary>
    /// The url that should be sent to
    /// </summary>
    private readonly string url;

    private readonly string apiKey;

    public RAGDOLLService() : this("")
    {
        
    }

    public RAGDOLLService(string apiKey) : this(apiKey, "https://iplvr.it.ntnu.no/")
    {}

    public RAGDOLLService(string apiKey, string url)
    {
        this.apiKey ??= apiKey;
        this.url ??= url += "/backend/api/chat/ask";
    }

    public async Task<IEnumerable<string>> GetAIResponse(List<Dialog> dialogs, OptionSet options)
    {
        OptionSet set = GetLLMOptions().Merge(options);
        if(set.ContainsOption(pickOption, out PickOption<string> value))
        await simulationousRequests.WaitAsync();
        RAGDOLLPacket requestBody = new(apiKey,value.GetObjectValue() as string, dialogs);
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
        string response = json["response"]["response"].ToObject<string>();

        return new List<string>() { response };
    }

    public OptionSet GetLLMOptions()
    {
        return new();
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
            : this(access_key, agent_id, RagDialogue.FromDialogues(Dialogues)){ }

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

