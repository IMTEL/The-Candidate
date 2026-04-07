public static class Services {

    //Pure service
    public static ILLMService AIService {get; private set;}
    public static ITTService ITTService {get; private set;}

    //Pure C# Systems
    public static DialogueScheduler dialogueScheduler {get; private set;}
    public static DialogueSystem dialogueSystem {get; private set;}
    public static NPCRegistry nPCRegistry {get; private set;}

    //Data C# Classes
    public static ScenarioSettings scenarioSettings {get; private set;}


    public static void InitializeGlobals(ILLMService ai, ITTService tts)
    {
        AIService = ai;
        ITTService = tts;
    }

    public static void StartNewSession(ScenarioSettings scenario, DialogueSystem dialogueSystem, DialogueScheduler dialogueScheduler, NPCRegistry nPCRegistry)
    {
        Services.scenarioSettings = scenario;
        Services.dialogueSystem = dialogueSystem;
        Services.dialogueScheduler = dialogueScheduler;
        Services.nPCRegistry = nPCRegistry;
    }
}