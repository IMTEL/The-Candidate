using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


/// <summary>
/// Creates dialogues with the AI model and
/// </summary>
public class DialogueSystem : IEvent<IDialogueReady>
{
    private readonly ScenarioSettings scenario;
    private readonly NPCRegistry NPCRegistry;

    private readonly List<Dialog> dialogs = new();
    public Action<(NPC, Dialog)> onDialogueReady;

    private readonly Random random = new();

    Agent coordinator;

    Agent AdSummary;

    public DialogueSystem(ScenarioSettings scenario, NPCRegistry nPCRegistry, Agent coordinator, Agent adSummary)
    {
        this.scenario = scenario;
        this.NPCRegistry = nPCRegistry;
        this.AdSummary = adSummary;
        this.coordinator = coordinator;
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
        dialogs.Add(userDialog);

        List<NPC> listnpcs = NPCRegistry.GetNPCEnumerable().ToList().OrderBy((_) => random.Next()).ToList();
        IEnumerator<NPC> npcs = listnpcs.GetEnumerator();
        npcs.MoveNext();
        while (npcs.Current)
        {
            NPC npc = npcs.Current;
            Agent agent = npc.GetAgent();
            if (!agent.GetLLMSettings(out LLMServiceProvider provider, out OptionSet options)){ return; }

            LLMResponse dialogue = await ((ILLMService)provider.CreateService()).GetAIResponse(dialogs, options);
            if(!agent.GetTTSSettings(out TTSServiceProvider TTSProvider, out OptionSet TTSOptions))
            {
                Dialog dialog = new(npc.actor, dialogue.Dialogue, ((ITTService)TTSProvider.CreateService()).getAudioClip(dialogue.Dialogue, TTSOptions));
                token.ThrowIfCancellationRequested();
                dialogs.Add(dialog);
                onDialogueReady?.Invoke((npc, dialog));
            }


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