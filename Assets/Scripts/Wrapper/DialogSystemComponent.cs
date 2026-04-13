using System;
using System.Threading;
using UnityEngine;

public class DialogueSystemComponent : MonoBehaviour, IMicrophoneEventListener, IEvent<IDialogueReady>, IInit, IDialogueReady
{
    [SerializeField] private DialogueSystem dialogueSystem;

    private CancellationTokenSource cancellationToken;

    private Action<(NPC, Dialog)> buffer;

    [TextArea(10,30)]
    public string prompt;

    
    [Init]
    protected void Init(SpawnManager nPCRegistry, ScenarioSettingsWrapper scenarioSettingsWrapper, ExtendedWhisperManager extendedWhisperManager)
    {
        dialogueSystem = new DialogueSystem(scenarioSettingsWrapper.getScenarioSettings(), nPCRegistry.GetNPCRegistry(), prompt);
        extendedWhisperManager.AddListener(this);
        dialogueSystem.AddListener(this);
    }


    public void onMicrophoneStartRecording()
    {
        cancellationToken?.Cancel();
    }

    public void onMicrophoneStopRecording(Dialog recordedaudio)
    {
        (_, cancellationToken) = dialogueSystem.HandleUserDialog(recordedaudio);
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
        buffer?.BeginInvoke(dialog, null, null);
    }
}