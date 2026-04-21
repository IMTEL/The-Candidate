using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueSystemComponent : MonoBehaviour, IMicrophoneEventListener, IEvent<IDialogueReady>, IInit, IDialogueReady
{
    [SerializeField] private DialogueSystem dialogueSystem;

    private CancellationTokenSource cancellationToken;

    private Action<(NPC, Dialog)> buffer;

    private readonly object _lock = new();

    [TextArea(10,30)]
    public string prompt;

    ScenarioSettingsWrapper scenarioSettings;

    SpawnManager spawnManager;
    
    [Init]
    protected void Init(SpawnManager nPCRegistry, ScenarioSettingsWrapper scenarioSettingsWrapper, ExtendedWhisperManager extendedWhisperManager)
    {
        spawnManager = nPCRegistry;
        scenarioSettings = scenarioSettingsWrapper;
        SceneManager.activeSceneChanged += ResetDialogueSystem;
        ResetDialogueSystem(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
        extendedWhisperManager.AddListener(this);
    }

    private void ResetDialogueSystem(Scene scene1, Scene scene2)
    {
        spawnManager.UpdateScene();
        if (!scene2.name.Equals("Start") && dialogueSystem != null)
        {
            onMicrophoneStopRecording(new(Environment.Actor.Director, "Introduce yourself"));
        }    
        else
        {
            dialogueSystem?.RemoveListener(this);
            dialogueSystem = new DialogueSystem(scenarioSettings.getScenarioSettings(), spawnManager.GetNPCRegistry(), prompt);
            dialogueSystem.AddListener(this);    
        }

        }


    public void onMicrophoneStartRecording()
    {
        cancellationToken?.Cancel();
    }

    public void onMicrophoneStopRecording(Dialog recordedaudio)
    {
        lock (_lock)
        {
            if(cancellationToken != null && !cancellationToken.IsCancellationRequested) cancellationToken.Cancel();
            (_, cancellationToken) = dialogueSystem.HandleUserDialog(recordedaudio);    
        }
        
    }

    public void AddListener(IDialogueReady listener)
    {
        buffer += listener.onDialogueReady;
    }


    public void RemoveListener(IDialogueReady listener)
    {
        buffer -= listener.onDialogueReady;
    }

    public void onDialogueReady((NPC, Dialog) dialog)
    {
        buffer?.Invoke(dialog);
    }
}