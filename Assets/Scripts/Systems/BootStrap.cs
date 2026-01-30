using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Whisper;

/// <summary>
/// Iniates the requirments for running the game
/// </summary>
///
public class BootStrap : MonoBehaviour
{
    private BootStrap instance;
    [LabelText("TTService")]
    public ServiceProviderSO<TTService> TTService;
    [LabelText("TTS Api Key")]
    public string TTSApiKey;
    [LabelText("AI Service")]
    [ShowInInspector, SerializeReference]
    public ServiceProviderSO<LLMService> LLMService;
    [LabelText("AI Api Key")]
    public string AIApiKey;

    void Awake()
    {
        //Ensure only one instance at a time
        if(instance && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        //Perservce in between scenes
        DontDestroyOnLoad(this);

        //Ensuring the AI and TTService is created and functional
        if (Services.AIService == null || Services.TTService == null)
        {
            //ILLMService aIService =  new(AIApiKey);
            //ITTService tTService = new TTService(TTSApiKey);
            //Services.InitializeGlobals(aIService, tTService);
        }
        
        WhisperManager whisper = GetComponentInChildren<WhisperManager>();
        DialogueScheduler dialogueScheduler = GetComponentInChildren<DialogueScheduler>();
        //If the scene is the main menu, aka we wanna configure the application again redo configurations
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            ScenarioSettings scenarioSettings = new();
            NPCRegistry nPCRegistry = new();
            DialogueSystem dialogueSystem = new(Services.AIService, scenarioSettings, whisper, Services.TTService, nPCRegistry);
            Services.StartNewSession(scenarioSettings, dialogueSystem, dialogueScheduler, nPCRegistry);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
