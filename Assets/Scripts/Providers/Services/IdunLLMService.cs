
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

    private List<OptionEntry<OpenAICompletionRequest.Model>> Models = new(){
            new (OpenAICompletionRequest.Model.IDUN_GPT_OSS_120b, "120b, OpenAI"),
            new (OpenAICompletionRequest.Model.IDUN_Mistral_Large_3_675B_Instruct_2512_NVFP4, "Minstral 3"),
            new (OpenAICompletionRequest.Model.IDUN_NorwAI_Magistral_24B_reasoning, "NorwAI 24B")
        };
    
    private TextOption prompt = new("Prompt");

    private PickOption<OpenAICompletionRequest.Model> OptionModel;

    private static readonly FloatBoundOption Temperature = new("Temperature", 1, 0, 2);

    /// <summary>
    /// A limiter used to only allow a certain amount of requests at the same time.
    /// This is to limit the amount of unnecessary traffic
    /// </summary>
    private readonly SemaphoreSlim simulationousRequests = new(1);

    /// <summary>
    /// The url that should be sent to
    /// </summary>
    private string url = "https://llm.hpc.ntnu.no/v1/chat/completions";

    private readonly string apiKey;

    public IdunLLMService(string apiKey) : this()
    {
        this.apiKey ??= apiKey;
    }

    public IdunLLMService()
    {
        OptionModel = new("Model", Models);
    }

    public async Task<IEnumerable<string>> GetAIResponse(List<Dialog> dialogs, OptionSet options)
    {
        OptionSet set = GetLLMOptions().Merge(options);
        await simulationousRequests.WaitAsync();
        set.ContainsOption(OptionModel, out PickOption<OpenAICompletionRequest.Model> option);
        set.ContainsOption(prompt, out TextOption promptValue);
        OpenAICompletionRequest requestBody = new(promptValue.GetObjectValue() as string, model: option.GetCurrentValue(), dialogs);
        UnityWebRequest request = new(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestBody.ToJson())),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        await request.SendWebRequest();
        simulationousRequests.Release();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            return null;
        }

        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        OpenAICompletionResponseMessage openAICompletionResponseMessage = json["choices"][0]["message"].ToObject<OpenAICompletionResponseMessage>();

        return new List<string>() { openAICompletionResponseMessage.content };
    }

    public void SetAPIKey(string key)
    {
        
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
}
