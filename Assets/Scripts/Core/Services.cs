using UnityEngine.Rendering.Universal;
using Whisper;

public static class Services {

    //Pure service
    public static AIService AIService;
    public static TTService TTService;

    //Pure C# Systems
    public static DialogueScheduler dialogueScheduler;
    public static DialogueSystem dialogueSystem;

    //Monobehaviour Systems
    public static NPCRegistry nPCRegistry;

    //Data C# Classes
    public static ScenarioSettings scenarioSettings;


    public static void InitializeGlobals(AIService ai, TTService tts)
    {
        AIService = ai;
        TTService = tts;
    }

    public static void StartNewSession(ScenarioSettings scenario, DialogueSystem dialogueSystem, DialogueScheduler dialogueScheduler, NPCRegistry nPCRegistry)
    {
        Services.scenarioSettings = scenario;
        Services.dialogueSystem = dialogueSystem;
        Services.dialogueScheduler = dialogueScheduler;
        Services.nPCRegistry = nPCRegistry;
    }
}