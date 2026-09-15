using System;
using System.Collections.Generic;

public class LLMResponse
{
    readonly List<Action> functions = new();
    public string Dialogue {get; private set;}

    public LLMResponse(string response, IEnumerable<Action> lLMFunctions)
    {
        Dialogue = Dialogue;
        functions = new(lLMFunctions);
    }
    
}