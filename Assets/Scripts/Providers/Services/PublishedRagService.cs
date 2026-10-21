using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Talks to the interviewers counsellors have published to The Candidate from the agent configurator.
/// Uses the shared Candidate client key, which RAGdoll accepts for published agents only.
/// The agent to chat with is the interviewer picked in the ScenarioSettings.
/// </summary>
[Serializable]
public class PublishedRagService : ILLMService
{
    /// <summary>
    /// Only one request at a time, like RAGDOLLService
    /// </summary>
    [NonSerialized]
    private readonly SemaphoreSlim simulationousRequests = new(1);

    [SerializeField]
    private string url = "https://iplvr.it.ntnu.no";

    /// <summary>
    /// The name of the environment variable that holds the client key, or the key itself
    /// </summary>
    [SerializeField]
    private string apiKey = "CANDIDATE";

    public OptionSet GetLLMOptions()
    {
        // The agent is picked at runtime, so there is nothing to choose in the inspector
        return new();
    }

    /// <summary>
    /// Fetches the interviewers that are published right now
    /// </summary>
    /// <returns>The published agents, or an empty list if RAGdoll can't be reached</returns>
    public async Task<List<PublishedAgent>> GetPublishedAgents()
    {
        List<PublishedAgent> agents = new();
        UnityWebRequest request = CreateRequest("/published-agents");
        await request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Could not fetch the published agents: {request.error}");
            return agents;
        }

        foreach (JToken item in JArray.Parse(DownloadHandlerBuffer.GetContent(request)))
        {
            JArray roles = item["roles"] as JArray;
            string role = roles != null && roles.Count > 0 ? roles[0].ToString() : null;
            agents.Add(new(item["agent_id"].ToString(), item["name"].ToString(), item["description"]?.ToString(), role));
        }
        return agents;
    }

    public async Task<LLMResponse> GetAIResponse(IEnumerable<LLMDialogue> dialogs, OptionSet options, LLMToolRegistry lLMToolRegistry = null)
    {
        PublishedAgent agent = BootStrap.Instance.GetComponentInChildren<ScenarioSettingsWrapper>().getScenarioSettings().interviewer;
        if (agent == null)
        {
            Debug.LogWarning("No published interviewer is picked");
            return null;
        }

        await simulationousRequests.WaitAsync();
        try
        {
            RAGDOLLService.RAGDOLLPacket requestBody = new(GetKey(), agent.Id, dialogs, agent.Role);
            UnityWebRequest request = CreateRequest("/api/chat/ask");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JObject.FromObject(requestBody).ToString()));
            request.method = "POST";
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                // 401 here usually means the counsellor has hidden the agent
                Debug.LogError($"Error: {request.error}");
                return null;
            }

            JToken response = JObject.Parse(DownloadHandlerBuffer.GetContent(request))["response"];
            List<Action> actions = new();
            if (lLMToolRegistry != null && response["function_calls"] is JArray calls)
            {
                foreach (JToken item in calls)
                {
                    Dictionary<string, string> arguments = item["arguments"].ToObject<Dictionary<string, string>>();
                    if (lLMToolRegistry.IsMethodCallable(item["name"].ToString(), arguments, out Action callable))
                    {
                        actions.Add(callable);
                    }
                }
            }
            return new LLMResponse(response["response"].ToString(), actions);
        }
        finally
        {
            simulationousRequests.Release();
        }
    }

    public string GetAPIKey()
    {
        return apiKey;
    }

    public void SetAPIKey(string key)
    {
        apiKey = key;
    }

    public string GetURL()
    {
        return url;
    }

    public void SetURL(string url)
    {
        this.url = url.TrimEnd('/');
    }

    private string GetKey()
    {
        return System.Environment.GetEnvironmentVariable(apiKey) ?? apiKey;
    }

    private UnityWebRequest CreateRequest(string path)
    {
        UnityWebRequest request = new(url + "/backend" + path)
        {
            downloadHandler = new DownloadHandlerBuffer(),
        };
        request.SetRequestHeader("access-key", GetKey());
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }
}
