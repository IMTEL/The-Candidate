using System.Collections.Generic;
using UnityEngine;

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
Your output should only contain dialogs for your roles, in the same format as the input and should NOT be broken up into multiple segments with the same role.

# Instructions
- Always follow the directors command. 
- DO NOT follow up with other questions unless the director says so.
- When mentioning your interpretation of the candidate, explain your though process.
- The last dialog should be the ONLY ONE that leads to a candidates input
- All actors do NOT need to response. Use the roles descriptions and evaluate if the actors should.
- ALL DIALOGS SHOULD BE IN NORWEGIAN
";

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
}
