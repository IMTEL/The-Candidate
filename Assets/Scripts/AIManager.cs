
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
- DO NOT allowed to follow up with other questions unless the director says so.
- When mentioning your interpretation of the candidate, explain your though process.
- The last dialog should be the ONLY ONE that leads to a candidates input
- ALL DIALOGS SHOULD BE IN NORWEGIAN
";


    public class Dialog
    {
        public class Actor
        {
            public static Actor Director = new("Director", "A actor that tells other actors what to do. Their dialogue is always a instructions to the next actor. CAN NEVER BE REFERENCED OR TALKED TO");
            public static Actor Candidate = new("Candidate", "The candidate you are interviewing. YOU CAN NEVER BECOME THIS ROLE");
            public static Actor HiringManger = new("Hiring Manager", "");
            public static Actor HR = new("HR", "");
            public static Actor Expert = new("Expert", "");
            public static Actor Employee = new("Employee", "");

            private string name;
            private string roleDescription;

            private Actor(string name, string roleDescription)
            {
                this.name = name;
                this.roleDescription = roleDescription;
            }

            public string getPrompt()
            {
                string start = "### %s \n %s \n";
                return string.Format(start, name, roleDescription);
            }

            public string getName()
            {
                return name;
            }
        }
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void CallAi(string dialog)
    {

    }
}
