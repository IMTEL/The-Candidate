using System;
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

    public OpenAICompletionRequest(string instructions, Model model, List<Dialog> previousConversation = null )
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
            Model.IDUN_GPT_OSS_120b => "openai/gpt-oss-120b",
            Model.IDUN_Mistral_Large_3_675B_Instruct_2512_NVFP4 => "mistralai/Mistral-Large-3-675B-Instruct-2512-NVFP4",
            Model.IDUN_NorwAI_Magistral_24B_reasoning => "NorwAI/NorwAI-Magistral-24B-reasoning",
            Model.GPT_OSS_120b => "gpt-oss-120b",
            Model.GPT4 => "gpt-4.1",
            Model.GPT5 => "gpt-5",
            _ => throw new NotImplementedException("The model has no translation to plaintext. Please add it to the list"),
        };
    }

    public enum Model
    {
        GPT4,
        GPT5,
        GPT_OSS_120b,
        IDUN_GPT_OSS_120b,
        IDUN_Mistral_Large_3_675B_Instruct_2512_NVFP4,
        IDUN_NorwAI_Magistral_24B_reasoning
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