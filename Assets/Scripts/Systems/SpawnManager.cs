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

    private ScenarioSettings settings;


    [Init]
    private void Init(ScenarioSettingsWrapper scenarioSettingsWrapper)
    {
        this.settings = scenarioSettingsWrapper.getScenarioSettings();
    }

    public void UpdateScene()
    {
        spawnLocations = FindObjectsByType<NPCSpawnPoint>(sortMode: FindObjectsSortMode.None);
        if(spawnLocations.Count() == 0) return;
        nPCRegistry.Clear();

        List<GameObject> npcCopies = new(npcPrefabs); 
        System.Random random = new();
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
