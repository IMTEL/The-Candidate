using System;
using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using UnityEngine;

public class SpawnManager : MonoBehaviour, INPCEventListener, IInit
{
    [ShowInInspector]
    private NPCSpawnPoint[] spawnLocations;

    private Action<NPC> OnCreateNPC;

    [ShowInInspector]
    [LabelText("NPC Register")]
    readonly NPCRegistry nPCRegistry = new();
    
    [ShowInInspector]
    [LabelText("NPC Prefabs")]
    public List<GameObject> npcPrefabs = new();

    /// <summary>
    /// Used by the interviewer NPC when a published interviewer is picked
    /// </summary>
    public PublishedRagProvider publishedAgentProvider;

    private ScenarioSettings settings;


    public void UpdateScene()
    {
        spawnLocations = FindObjectsByType<NPCSpawnPoint>(sortMode: FindObjectsSortMode.None);
        if(spawnLocations.Count() == 0) return;
        nPCRegistry.Clear();

        List<GameObject> npcCopies = new(npcPrefabs); 
        System.Random random = new();
        List<(Environment.Actor actor, int amount)> actors = new(settings.GetActors().Where(actor => actor.amount > 0));
        List<NPC> spawned = new();
        foreach (NPCSpawnPoint spawnPoint in spawnLocations)
        {
            if(npcCopies.Count > 0 && spawnPoint.GetComponentInChildren<NPC>() == null && actors.Count > 0)
            {
                int actorIndex = random.Next(1, actors.Count)-1;
                int index = random.Next(1, npcCopies.Count)-1;
                GameObject gameObject = Instantiate(npcCopies[index], spawnPoint.location);
                NPC npc = gameObject.GetComponentInChildren<NPC>();
                nPCRegistry.Add(npc);
                spawned.Add(npc);
                npc.actor = actors[actorIndex].actor;
                if(actors[actorIndex].amount - 1 == 0) actors.RemoveAt(actorIndex);
                else actors[actorIndex] = (actors[actorIndex].actor, actors[actorIndex].amount - 1);
                npcCopies.RemoveAt(index);
            }
        }
        UsePublishedInterviewer(spawned);
    }

    /// <summary>
    /// The actors that lead an interview, most fitting first
    /// </summary>
    private static readonly Environment.Actor[] InterviewerActors =
        { Environment.Actor.HiringManger, Environment.Actor.Manager, Environment.Actor.HR };

    /// <summary>
    /// If a published interviewer is picked, one NPC talks as it: the one whose actor leads
    /// the interview (see InterviewerActors), or the first one spawned if none does.
    /// The other NPCs keep their own agent.
    /// </summary>
    private void UsePublishedInterviewer(List<NPC> spawned)
    {
        if (spawned.Count == 0 || settings.interviewer == null || publishedAgentProvider == null) return;
        NPC interviewer = InterviewerActors
            .Select(actor => spawned.FirstOrDefault(npc => npc.actor == actor))
            .FirstOrDefault(npc => npc != null) ?? spawned[0];
        interviewer.GetAgent().SetLLMServiceProvider(publishedAgentProvider);
    }

    void Awake()
    {
        List<GameObject> npcPrefabsCopy = new(npcPrefabs);
        foreach (GameObject item in npcPrefabsCopy)
        {
            if (!item.GetComponentInChildren<NPC>())
            {
                npcPrefabs.Remove(item);
            }
        }
    }


    void Start()
    {
        this.settings = BootStrap.Instance.GetComponentInChildren<ScenarioSettingsWrapper>().getScenarioSettings();
        UpdateScene();
    }
    void INPCEventListener.OnCreateNPC(NPC nPC)
    {
        
    }

    public NPCRegistry GetNPCRegistry()
    {
        return nPCRegistry;
    }

    public void OnDestroyNPC(NPC nPC)
    {
        throw new NotImplementedException();
    }

}
