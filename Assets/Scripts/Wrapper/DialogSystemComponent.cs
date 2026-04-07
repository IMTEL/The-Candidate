using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class DialogueSystemComponent : MonoBehaviour, IMicrophoneEventListener, IEvent<IDialogueReady>, IDialogueReady, IInit
{
    [SerializeField] private DialogueSystem dialogueSystem;

    private Task workingThread;
    private CancellationTokenSource cancellationToken;

    private Action<(NPC, Dialog)> onDialogueReadyAbstract;

    private void Awake()
    {
    }
    
    [Init]
    protected void Init(SpawnManager nPCRegistry, ScenarioSettingsWrapper scenarioSettingsWrapper, ExtendedWhisperManager extendedWhisperManager)
    {
        dialogueSystem = new DialogueSystem(scenarioSettingsWrapper.getScenarioSettings(), nPCRegistry.GetNPCRegistry());
        extendedWhisperManager.AddListener(this);
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