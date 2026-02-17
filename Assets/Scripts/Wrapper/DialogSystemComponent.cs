using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Alchemy.Inspector;
using UnityEngine;

public class DialogueSystemComponent : MonoBehaviour, IMicrophoneEventListener, IEvent<IDialogueReady>, IDialogueReady, IInit
{
    [SerializeField] private DialogueSystem dialogueSystem;

    private Task workingThread;
    private CancellationTokenSource cancellationToken;

    [LabelText("TTService")]
    public ServiceProviderSO<TTService> TTService;
    
    [LabelText("AI Service")]
    public ServiceProviderSO<LLMService> LLMService;

    private Action<(NPC, Dialog)> onDialogueReadyAbstract;

    TTService TTSInstance;

    LLMService LLMInstance;

    private void Awake()
    {
        this.TTSInstance = (TTService)TTService.CreateService();
        this.LLMInstance = (LLMService)LLMService.CreateService();
    }
    
    [Init]
    protected void Init(SpawnManager nPCRegistry, ScenarioSettingsWrapper scenarioSettingsWrapper, ExtendedWhisperManager extendedWhisperManager)
    {
        dialogueSystem = new DialogueSystem(LLMInstance, TTSInstance, scenarioSettingsWrapper.getScenarioSettings(), nPCRegistry.GetNPCRegistry());
        extendedWhisperManager.AddListener(this);
    }

    private void OnValidate()
    {
        if(LLMService == null || TTService == null)
        {
            throw new ArgumentException("You need to set the services needed within the DialogSystemComponent");
        }
    }

    public void onMicrophoneStartRecording()
    {
        cancellationToken?.Cancel();
    }

    public void onMicrophoneStopRecording(Dialog recordedaudio)
    {
        (workingThread, cancellationToken) = dialogueSystem.HandleUserDialog(recordedaudio);
    }

    public void onDialogueReady((NPC, Dialog) dialog)
    {
        onDialogueReadyAbstract?.Invoke(dialog);
    }

    public void AddListener(IEnumerable<IDialogueReady> listeners)
    {
        foreach (var item in listeners)
        {
            AddListener(item);
        }
    }

    public void AddListener(IDialogueReady listener)
    {
        onDialogueReadyAbstract += listener.onDialogueReady;
    }

    public void RemoveListener(IEnumerable<IDialogueReady> listeners)
    {
        foreach (var item in listeners)
        {
            RemoveListener(item);
        }
    }

    public void RemoveListener(IDialogueReady listener)
    {
        onDialogueReadyAbstract += listener.onDialogueReady;
    }
}