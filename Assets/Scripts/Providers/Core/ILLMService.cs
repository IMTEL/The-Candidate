using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface that allows for sending
/// The function GetLLMOPtions need to be overw
/// </summary>
public interface ILLMService
{
    /// <summary>
    /// Sends a request to the service and gives back the response for the text
    /// </summary>
    /// <param name="startingPrompt">the starting prompt</param>
    /// <param name="dialogs">the previous dialogs</param>
    /// <returns>The next dialog from the AI</returns>
    public Task<LLMResponse> GetAIResponse(List<Dialog> dialogs, OptionSet options, LLMToolRegistry lLMToolRegistry = null);

    /// <summary>
    /// Returns the options within the LLMService
    /// Needs to be overwritten with the different settings for the model
    /// </summary>
    /// <returns>The options within the service</returns>
    public OptionSet GetLLMOptions();

    
}