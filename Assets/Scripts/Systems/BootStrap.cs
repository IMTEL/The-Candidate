using System;
using System.Collections.Generic;
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
    [LabelText("AI Service")]
    [ShowInInspector, SerializeReference]
    public ServiceProviderSO<LLMService> LLMService;

    void Awake()
    {
        //Ensure only one instance at a time
        if(instance && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        LLMService aIService = Services.AIService;
        TTService tTService = Services.TTService;

        //Perservce in between scenes
        DontDestroyOnLoad(this);
        //Ensuring the AI and TTService is created and functional
        if (aIService == null || tTService == null)
        {
            aIService = (LLMService) LLMService.CreateService();
            tTService = (TTService) TTService.CreateService();
            Services.InitializeGlobals(aIService, tTService);
        }
        
        WhisperManager whisper = GetComponentInChildren<WhisperManager>();
        
        MicrophoneInputRecording microphoneInput = GetComponentInChildren<MicrophoneInputRecording>();
        
        DialogueScheduler dialogueScheduler = GetComponentInChildren<DialogueScheduler>();

        //If the scene is the main menu, aka we wanna configure the application again redo configurations
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            RemoveAndDisconnectSessionServices(microphoneInput);
            ScenarioSettings scenarioSettings = new();
            NPCRegistry nPCRegistry = new();
            DialogueSystem dialogueSystem = new(Services.AIService, scenarioSettings, Services.TTService, nPCRegistry);
            Services.StartNewSession(scenarioSettings, dialogueSystem, dialogueScheduler, nPCRegistry);
            dialogueSystem.onDialogueReady += dialogueScheduler.onDialogueReady;
            microphoneInput.AddListener(new List<IMicrophoneEventListener>()
            {
                dialogueScheduler
            });
        }
    }

    private void RemoveAndDisconnectSessionServices(MicrophoneInputRecording microphoneInput )
    {
        microphoneInput.RemoveListener(new List<IMicrophoneEventListener>()
        {
           Services.dialogueScheduler
        });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        DialogUI dialogUI = player.GetComponentInChildren<DialogUI>();
        if(dialogUI != null)
        {
            MicrophoneInputRecording microphoneInput = GetComponentInChildren<MicrophoneInputRecording>();
            dialogUI.GetButton().onClick.AddListener(microphoneInput.OnClick);
        }
        
    }
}
