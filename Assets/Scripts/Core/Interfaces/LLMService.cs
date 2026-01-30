using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface that allows for sending
/// </summary>
public interface LLMService
{
    /// <summary>
    /// Sends a request to the service and gives back the response for the text
    /// </summary>
    /// <param name="startingPrompt">the starting prompt</param>
    /// <param name="dialogs">the previous dialogs</param>
    /// <returns>The next dialog from the AI</returns>
    public Task<string> getAIResponse(string startingPrompt, List<Dialog> dialogs);
}