using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AIManager : MonoBehaviour
{



    private const string startPrompt = @"
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

    public string apiKey;

    public string url = "https://ai.hpc.ntnu.no/api/gpt-oss-120b/v1";

    public List<Dialog> dialogs;

    public class Dialog
    {
        public Environment.Actor actor;
        public string dialog;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void CallAi(string dialog)
    {

    }


    string BuildPrompt(Environment environment, Environment.Job job, List<Dialog> dialogHistory)
    {
        string setting = environment.ToString();
        string role = job.ToString();
        string actors = string.Join("\n", job.Actors.Select(actor => actor.ToString()));
        string dialogHistoryStr = string.Join("\n", dialogHistory.Select(d => $"[{d.actor.Name}]{{{d.dialog}}}"));

        string prompt = startPrompt
            .Replace("%s", setting)
            .Replace("%s", role)
            .Replace("%s", actors)
            .Replace("%s", dialogHistoryStr);

        return prompt;
    }

    async Task<string> getAIResponse(string prompt)
    {
        UnityWebRequest request = new(url, "POST");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(prompt);
        request.downloadHandler = new DownloadHandlerBuffer();
        await request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            return null;
        }
    }
