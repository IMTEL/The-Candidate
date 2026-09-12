using System.Collections.Generic;

public class LLMResponse
{
    readonly List<FunctionCall> functions = new();
    public string Dialogue {get; private set;}

    LLMResponse(string response, IEnumerable<FunctionCall> lLMFunctions)
    {
        Dialogue = Dialogue;
        functions = new(lLMFunctions);
    }


    internal class FunctionCall
    {
        public string name;
        public Dictionary<string, object> arguemnts;
    }
}