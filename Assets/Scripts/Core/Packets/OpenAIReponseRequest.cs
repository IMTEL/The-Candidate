using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Wrapper class for the OpenAI Completion request
/// </summary>
public class OpenAICompletionRequest
{
    public string model {get; private set;}
    public List<OpenAIMessage> messages {get; private set;} = new();
    public bool store {get; private set;} = false;

    public OpenAICompletionRequest(string instructions, List<Dialog> previousConversation = null, Model model = Model.GPT_OSS_120b)
    {
        messages.Add(new OpenAIMessage("developer", instructions));
        foreach(Dialog dialog in previousConversation)
        {
            messages.Add(new OpenAIMessage(dialog.actor == Environment.Actor.Candidate ? "user" : "system", dialog.dialog, dialog.actor.getName()));
        }
        this.model = ModeltoString(model);
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }


    private static string ModeltoString(Model model)
    {
        return model switch
        {
            Model.GPT_OSS_120b => "openai/gpt-oss-120b",
            Model.GPT4 => "openai/gpt-4.1",
            _ => "openai/gpt-oss-120b",
        };
    }

    public enum Model
    {
        GPT4,
        GPT5,
        GPT_OSS_120b
    }

    public struct OpenAIMessage
    {
        public string role {get; private set;}
        public string content {get; private set;}
        public string name {get; private set;}

        public OpenAIMessage(string role, string content, string name = "")
        {
            this.role = role;
            this.content = content;
            this.name = name;
        }
    }
}

/// <summary>
/// Wrapper class for the responsemessage from the OpenAI
/// </summary>
public class OpenAICompletionResponseMessage
{
    public string role;
    public string content;
    public string refusal;
}