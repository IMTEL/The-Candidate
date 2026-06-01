
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
public class IdunLLMService : ILLMService
{

    /// <summary>
    /// A limiter used to only allow a certain amount of requests at the same time.
    /// This is to limit the amount of unecassery traffic
    /// </summary>
    private readonly SemaphoreSlim simulationousRequests = new(1);
 
    /// <summary>
    /// The url that should be sent to
    /// </summary>
    private readonly string url = "https://llm.hpc.ntnu.no/v1/chat/completions";

    private readonly string apiKey;

    public IdunLLMService(string apiKey)
    {
        this.apiKey ??= apiKey;
    }

    public async Task<string> GetAIResponse(string startingPrompt, List<Dialog> dialogs, IEnumerable<OptionBase> options)
    {   OpenAICompletionRequest.Model model;
        try
        {
            model = (OpenAICompletionRequest.Model)options.First(option => option.GetName().Equals("Model")).GetObjectValue();
        }
        catch
        {
            model = OpenAICompletionRequest.Model.IDUN_GPT_OSS_120b;
        }
        await simulationousRequests.WaitAsync();
        OpenAICompletionRequest requestBody = new(startingPrompt, model: model, dialogs);
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
        
        return openAICompletionResponseMessage.content;
    }

    public static IEnumerable<OptionBase> GetLLMOptions()
    {
        List<OptionEntry<OpenAICompletionRequest.Model>> models = new(){
            new (OpenAICompletionRequest.Model.IDUN_GPT_OSS_120b, "120b, OpenAI"),
            new (OpenAICompletionRequest.Model.IDUN_Mistral_Large_3_675B_Instruct_2512_NVFP4, "Minstral 3"),
            new (OpenAICompletionRequest.Model.IDUN_NorwAI_Magistral_24B_reasoning, "NorwAI 24B")
        };

        return new List<OptionBase>
        {
            new FloatBoundOption("Temperature",1,0,2),
            // supply explicit display names for clarity
            new PickOption<OpenAICompletionRequest.Model>(
                "Model",
                models)
        };
    }
}
