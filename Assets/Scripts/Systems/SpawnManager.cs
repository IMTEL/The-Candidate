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
        if (settings.interviewer != null && publishedAgentProvider != null)
        {
            SpawnPublishedInterviewer(npcCopies, random);
            return;
        }
        List<(Environment.Actor actor, int amount)> actors = new(settings.GetActors().Where(actor => actor.amount > 0));
        foreach (NPCSpawnPoint spawnPoint in spawnLocations)
        {
            if(npcCopies.Count > 0 && spawnPoint.GetComponentInChildren<NPC>() == null && actors.Count > 0)
            {
                int actorIndex = random.Next(1, actors.Count)-1;
                int index = random.Next(1, npcCopies.Count)-1;
                GameObject gameObject = Instantiate(npcCopies[index], spawnPoint.location);
                NPC npc = gameObject.GetComponentInChildren<NPC>();
                nPCRegistry.Add(npc);
                npc.actor = actors[actorIndex].actor;
                if(actors[actorIndex].amount - 1 == 0) actors.RemoveAt(actorIndex);
                else actors[actorIndex] = (actors[actorIndex].actor, actors[actorIndex].amount - 1);
                npcCopies.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// A published interviewer was configured by a counsellor for one interview with one
    /// interviewer, so exactly one NPC spawns and talks as it. The actor amounts are not used.
    /// </summary>
    private void SpawnPublishedInterviewer(List<GameObject> npcCopies, System.Random random)
    {
        NPCSpawnPoint spawnPoint = spawnLocations.FirstOrDefault(point => point.GetComponentInChildren<NPC>() == null);
        if (spawnPoint == null || npcCopies.Count == 0) return;
        GameObject gameObject = Instantiate(npcCopies[random.Next(npcCopies.Count)], spawnPoint.location);
        NPC npc = gameObject.GetComponentInChildren<NPC>();
        nPCRegistry.Add(npc);
        npc.actor = Environment.Actor.HiringManger;
        npc.GetAgent().SetLLMServiceProvider(publishedAgentProvider);
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
