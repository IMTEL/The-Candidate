using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// Creates dialogues with the AI models
/// </summary>
public class DialogueSystem : IEvent<IDialogueReady>
{
    private readonly ScenarioSettings scenario;
    private readonly NPCRegistry NPCRegistry;

    private readonly List<Dialog> dialogs = new();
    public Action<(NPC, Dialog)> onDialogueReady;

    Agent coordinator;

    Agent AdSummary;

    public DialogueSystem(ScenarioSettings scenario, NPCRegistry nPCRegistry, Agent coordinator, Agent adSummary)
    {
        this.scenario = scenario;
        this.NPCRegistry = nPCRegistry;
        this.AdSummary = adSummary;
        this.coordinator = coordinator;
        this.coordinator.Initialize(this);
    }


    public (Task, CancellationTokenSource) HandleUserDialog(Dialog userDialog, CancellationTokenSource tokenSource = null)
    {
        if (userDialog == null) throw new ArgumentException("Dialog can't be null");
        tokenSource ??= new();
        return (HandleUserDialog(userDialog, tokenSource.Token), tokenSource);
    }

    /// <summary>
    /// Task to process the audio and start creating dialogues for the NPC
    /// </summary>
    /// <param name="recordedAudio">The recorded audio from the microphone</param>
    /// <param name="token">A cancellation token that will be checked if it is suppose to stop</param>
    /// <returns></returns>
    private async Task HandleUserDialog(Dialog userDialog, CancellationToken token)
    {
        
    }

    [LLMTool("Creates a new dialogue for a specific NPC", agentRole = new string[]{"Coordinator"})]
    private async Task CreateDialogue(
        [LLMParameter("The NPC that is gonna talk")] string NPC, 
        [LLMParameter("The context behind the dialogue")] string Context, 
        [LLMParameter("What they are supposed to say in the dialogue")] string Direction)
    {
        try
        {
            NPC npc = NPCRegistry.GetNPCEnumerable().First(npc => npc.name.Contains(NPC));
            LLMResponse response = await npc.GetAgent().CallLLM(dialogs);

            if(response == null)
            {
                Debug.LogWarning("LLM is missing in the NPC");
                return;
            }
        }
        catch (Exception)
        {
            Debug.LogError($"Could not find NPC with name {NPC}");
        }
    }


    public void AddListener(IDialogueReady listener)
    {
        onDialogueReady += listener.onDialogueReady;
    }

    public void RemoveListener(IDialogueReady listener)
    {
        onDialogueReady -= listener.onDialogueReady;
    }
}