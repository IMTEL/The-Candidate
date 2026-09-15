
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
public class IdunLLMService : ILLMService
{
    [SerializeField]
    private List<OptionEntry<string>> Models = new() {new("", "No Models :(")};

    [NonSerialized]
    private TextOption prompt = new("Prompt");
    
    [SerializeField]
    private PickOption<string> OptionModel;

    [NonSerialized]
    private static readonly FloatBoundOption Temperature = new("Temperature", 1, 0, 2);

    /// <summary>
    /// A limiter used to only allow a certain amount of requests at the same time.
    /// This is to limit the amount of unnecessary traffic
    /// </summary>
    [NonSerialized]
    private readonly SemaphoreSlim simulationousRequests = new(1);

    /// <summary>
    /// The url that should be sent to
    /// </summary>
    [NonSerialized]
    private readonly string url = "https://llm.hpc.ntnu.no";

    [SerializeField]
    private string apiKey = "";

    public IdunLLMService(string apiKey)
    {
        SetAPIKey(apiKey);
    }

    public IdunLLMService()
    {
        UpdateModelOption();
    }

    public async Task<LLMResponse> GetAIResponse(IEnumerable<Dialog> dialogs, OptionSet options, LLMToolRegistry lLMToolRegistry = null)
    {
        OptionSet set = GetLLMOptions().Merge(options);
        await simulationousRequests.WaitAsync();
        set.ContainsOption(OptionModel, out PickOption<string> option);
        set.ContainsOption(prompt, out TextOption promptValue);
        OpenAICompletionRequest requestBody = new(promptValue.GetObjectValue() as string, model: option.GetCurrentValue(), dialogs);
        UnityWebRequest request = CreateRequest("/v1/chat/completions");
        request.method = "POST";
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestBody.ToJson()));
        await request.SendWebRequest();
        simulationousRequests.Release();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            return null;
        }

        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        OpenAICompletionResponseMessage openAICompletionResponseMessage = json["choices"][0]["message"].ToObject<OpenAICompletionResponseMessage>();

        return new LLMResponse(openAICompletionResponseMessage.content, null);
    }

    public async Task SetAPIKey(string key)
    {
        this.apiKey = key;
        await RefreshModels();
        UpdateModelOption();
    }

    public async Task RefreshModels()
    {
        Models.Clear();
        UnityWebRequest request = CreateRequest("/models");
        await request.SendWebRequest();
        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("API key or the service is not accessible");
            return;
        }
        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        JToken data = json["data"];
        foreach (JToken Jmodel in data)
        {
            Models.Add(new(Jmodel["id"].ToString(), Jmodel["id"].ToString()));
        }
    }

    private void UpdateModelOption()
    {

        if (Models.Count == 0)
        {
            Models = new() {new("", "No Models :(")};
        }
        OptionModel = new("Model", Models);
    }

    private UnityWebRequest CreateRequest(string path)
    {
        UnityWebRequest request = new(url + path)
        {
            downloadHandler = new DownloadHandlerBuffer(),
        };
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    public OptionSet GetLLMOptions()
    {
        OptionSet optionSet = new();
        optionSet.AddOption(prompt);
        optionSet.AddOption(OptionModel.Clone());
        optionSet.AddOption(Temperature.Clone());
        return optionSet;
    }

    internal string getAPIKey()
    {
        return this.apiKey;
    }


    public readonly struct Model
    {
        public readonly string id;
        public readonly string name;

        Model(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

    }

}
