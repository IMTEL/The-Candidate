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

    private readonly SemaphoreSlim DialogueCreation = new(1);
    public Action<(NPC, Dialog)> onDialogueReady;

    readonly Agent coordinator;

    readonly Agent AdSummary;


    readonly string CoordinatorPrompt = @"Agents:";

    public DialogueSystem(ScenarioSettings scenario, NPCRegistry nPCRegistry, Agent coordinator, Agent adSummary)
    {
        this.scenario = scenario;
        this.NPCRegistry = nPCRegistry;
        this.AdSummary = adSummary;
        this.coordinator = coordinator;
        this.coordinator.Initialize(this);
    }

    /// <summary>
    /// Handles user dialog by assigning it to a private function. Returns a cancellation token to stop it
    /// As well the task in progress
    /// </summary>
    /// <param name="userDialog">The user dialog that is created</param>
    /// <param name="tokenSource">Token source for cancellation token. Will create a new one if null</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public (Task, CancellationTokenSource) HandleUserDialog(Dialog userDialog, CancellationTokenSource tokenSource = null)
    {
        if (userDialog == null) throw new ArgumentException("Dialog can't be null");
        tokenSource ??= new();
        return (HandleUserDialog(userDialog, tokenSource.Token), tokenSource);
    }

    /// <summary>
    /// Task to process the audio and start creating dialogues for the NPC
    /// </summary>
    /// <param name="userDialog">Dialog from the user</param>
    /// <param name="token">A cancellation token that will be checked if it is suppose to stop</param>
    /// <returns></returns>
    private async Task HandleUserDialog(Dialog userDialog, CancellationToken token)
    {
        string tempCoordinator = CoordinatorPrompt;
        foreach (NPC item in NPCRegistry.GetNPCEnumerable())
        {
            tempCoordinator += $"\n- {item.name}";
        }
        dialogs.Add(userDialog);
        List<LLMDialogue> tempDialogs = new(dialogs)
            {
                new LLMDialogue(tempCoordinator, LLMDialogue.RoleCategory.SYSTEM)
            };
        token.ThrowIfCancellationRequested();
        LLMResponse response = await coordinator.CallLLM(tempDialogs);
        foreach (Action item in response.GetFunctions())
        {
            try { item.Invoke(); }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

    [LLMTool("Creates a new dialogue for a specific NPC", agentRole = new string[] { "Coordinator" })]
    public async Task CreateDialogue(
        [LLMParameter("The NPC that is gonna talk")] string NPC,
        [LLMParameter("The context behind the dialogue")] string Context,
        [LLMParameter("What they are supposed to say in the dialogue")] string Direction)
    {
        await DialogueCreation.WaitAsync();
        try
        {
            NPC npc = NPCRegistry.GetNPCEnumerable().First(npc => npc.name.Contains(NPC));
            List<LLMDialogue> tempDialogs = new(dialogs)
            {
                new LLMDialogue($"Context: {Context}\nDirection: {Direction}", LLMDialogue.RoleCategory.SYSTEM)
            };
            LLMResponse response = await npc.GetAgent().CallLLM(tempDialogs);
            if (response == null)
            {
                Debug.LogWarning("LLM is missing in the NPC");
                return;
            }
            Dialog dialog = new(npc.actor, response.Dialogue, npc.GetAgent().CallTTS(response.Dialogue));
            dialogs.Add(dialog);
            onDialogueReady.Invoke((npc, dialog));
        }
        catch (InvalidOperationException)
        {
            Debug.LogError($"NPC {NPC} not found. Are you sure it is in the registry?");
        }
        finally
        {
            DialogueCreation.Release();
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