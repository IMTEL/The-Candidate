using System.Threading;
using UnityEngine;

public class DialogueSystemComponent : MonoBehaviour, IMicrophoneEventListener, IEvent<IDialogueReady>, IInit
{
    [SerializeField] private DialogueSystem dialogueSystem;

    private CancellationTokenSource cancellationToken;

    [TextArea(10,30)]
    public string prompt;

    
    [Init]
    protected void Init(SpawnManager nPCRegistry, ScenarioSettingsWrapper scenarioSettingsWrapper, ExtendedWhisperManager extendedWhisperManager)
    {
        dialogueSystem = new DialogueSystem(scenarioSettingsWrapper.getScenarioSettings(), nPCRegistry.GetNPCRegistry(), prompt);
        extendedWhisperManager.AddListener(this);
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
        dialogueSystem.AddListener(listener);
    }


    public void RemoveListener(IDialogueReady listener)
    {
        dialogueSystem.RemoveListener(listener);
    }
}