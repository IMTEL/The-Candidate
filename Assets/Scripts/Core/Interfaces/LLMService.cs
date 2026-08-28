using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface that allows for sending
/// </summary>
public interface ILLMService
{
    /// <summary>
    /// Sends a request to the service and gives back the response for the text
    /// </summary>
    /// <param name="startingPrompt">the starting prompt</param>
    /// <param name="dialogs">the previous dialogs</param>
    /// <returns>The next dialog from the AI</returns>
    public Task<IEnumerable<string>> GetAIResponse(string startingPrompt, List<Dialog> dialogs, IEnumerable<OptionBase> options);

    /// <summary>
    /// Returns the options within the LLMService
    /// Needs to be overwritten with the different settings for the model
    /// </summary>
    /// <returns>The options within the service</returns>
    static IEnumerable<OptionBase> GetLLMOptions() => throw new NotImplementedException("The implementation for option is not implemented within this instance. Add it :)");
}