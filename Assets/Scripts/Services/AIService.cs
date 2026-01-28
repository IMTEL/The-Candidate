using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Alchemy.Inspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// A single instance manager that allows for prompting to a LLM system to fetch a response.
/// Uses the chatgpt structure of request
/// To access, call the GetInstance function on the class
/// </summary>
public class AIService
{

    /// <summary>
    /// A limiter used to only allow a certain amount of requests at the same time.
    /// This is to limit the amount of necassery traffic
    /// </summary>
    private readonly SemaphoreSlim simulationousRequests = new(1);
    /// <summary>
    /// The url that should be sent to
    /// </summary>
    private string url = "https://ai.hpc.ntnu.no/api/gpt-oss-120b/v1";

    private string apiKey = "";

    public AIService(string url, string apiKey)
    {
        this.url ??= url;
        this.apiKey ??= apiKey;
    }

    /// <summary>
    /// The starting prompt within the message
    /// </summary>
    private readonly string startPrompt = @"
# Context
You are an actor within a movie where you are with different other people to interview a potential candidate. 


## Setting
%s

## Your Role
%s

## Actors
%s

# Structure
## Input
The input of the user will be a list of dialogs of what has been said since your last prompt, following this format:

```
[Actor]{Their dialog}
```

## Output
Your output should only contain dialogs for your roles, in the same format as the input and NOT broken up into multiple segments with the same role.

# Instructions
- Always follow the directors command. 
- You are ONLY allowed up to 2 follow up question for a subject. NOTHING MORE
- When mentioning your interpretation of the candidate, explain your though process.
- The last dialog should be the ONLY ONE that leads to a candidates input
- All actors do NOT response. Use the roles descriptions and evaluate if the actors should.
- ALL DIALOGS SHOULD BE IN NORWEGIAN
";



    /// <summary>
    /// Builds the prompt
    /// </summary>
    /// <param name="job"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    [Button]
    string BuildPrompt(Environment.Job job, Environment.Actor actor)
    {
        string setting = job.ToString();
        string actors = string.Join("\n - ", job.actors.Select(actor => actor.ToString()));
        return string.Format(startPrompt, setting, actor.getPrompt(), actors);
    }

    /// <summary>
    /// Fetches a response from a AI trough the use
    /// </summary>
    /// <param name="job"></param>
    /// <param name="aiActor"></param>
    /// <param name="dialogs"></param>
    /// <returns></returns>
    public async Awaitable<string> getAIResponse(Environment.Job job, Environment.Actor aiActor, List<Dialog> dialogs)
    {
        await simulationousRequests.WaitAsync();
        string dialogHistory = "[";
        dialogHistory += string.Format("{{\"role\": \"%s\", \"content\": \"%s\"}}", "developer", BuildPrompt(job, aiActor));
        foreach (Dialog dialog in dialogs){
            dialogHistory += string.Format("{{\"role\": \"%s\", \"content\": \"%s\"}}", Environment.Actor.Candidate.Equals(dialog.actor) ? "user" : "assistant", dialog.dialog);
        }
        dialogHistory += "]";
        UnityWebRequest request = new(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(string.Format("{\"model\": \"gpt-5-nano\", \"messages\": %s}", dialogHistory))),
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

        //TODO: Need to change from string to a dialogue at then end. We will see what we are gonna do with the prompting
        return DownloadHandlerBuffer.GetContent(request);
    }

}
