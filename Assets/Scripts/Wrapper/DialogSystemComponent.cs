using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueSystemComponent : MonoBehaviour, IMicrophoneEventListener, IEvent<IDialogueReady>, IInit, IDialogueReady
{
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private Agent AdSummerazation;
    [SerializeField] private Agent Cordinator;


    private CancellationTokenSource cancellationToken;

    private Action<(NPC, Dialog)> buffer;

    private readonly object _lock = new();

    ScenarioSettingsWrapper scenarioSettings;

    SpawnManager spawnManager;

    [Init]
    protected void Init(SpawnManager nPCRegistry, ScenarioSettingsWrapper scenarioSettingsWrapper, ExtendedWhisperManager extendedWhisperManager)
    {
        spawnManager = nPCRegistry;
        scenarioSettings = scenarioSettingsWrapper;
        dialogueSystem = new DialogueSystem(scenarioSettings.getScenarioSettings(), spawnManager.GetNPCRegistry(), Cordinator, AdSummerazation);
        dialogueSystem.AddListener(this);
        SceneManager.activeSceneChanged += (_, Scene2) => { spawnManager.UpdateScene(); ResetDialogueSystem(Scene2); };
        ResetDialogueSystem(SceneManager.GetActiveScene());
        extendedWhisperManager.AddListener(this);
    }

    private void ResetDialogueSystem(Scene scene2)
    {
        if (!scene2.name.Equals("Start") && dialogueSystem != null)
        {
            onMicrophoneStopRecording(new(Environment.Actor.Director, "Introduce yourself"));
        }
        else
        {
            dialogueSystem?.RemoveListener(this);
            dialogueSystem = new DialogueSystem(scenarioSettings.getScenarioSettings(), spawnManager.GetNPCRegistry(), Cordinator, AdSummerazation);
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
            if (cancellationToken != null && !cancellationToken.IsCancellationRequested) cancellationToken.Cancel();
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