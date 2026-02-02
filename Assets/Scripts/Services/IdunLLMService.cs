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
public class IdunLLMService : LLMService
{

    /// <summary>
    /// A limiter used to only allow a certain amount of requests at the same time.
    /// This is to limit the amount of necassery traffic
    /// </summary>
    private readonly SemaphoreSlim simulationousRequests = new(1);

    /// <summary>
    /// The url that should be sent to
    /// </summary>
    private string url = "https:/llm.hpc.ntnu.no/completions";

    private string apiKey;

    public IdunLLMService(string apiKey)
    {
        this.apiKey ??= apiKey;
    }

    public async Task<string> getAIResponse(string startPrompt, List<Dialog> dialogs)
    {
        await simulationousRequests.WaitAsync();
        string requestBody = new OpenAICompletionRequest(startPrompt, dialogs, model: OpenAICompletionRequest.Model.GPT_OSS_120b).ToJson();
        UnityWebRequest request = new(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestBody)),
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

}
