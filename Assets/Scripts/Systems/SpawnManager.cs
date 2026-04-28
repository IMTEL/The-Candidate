using System;
using System.Collections.Generic;
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

    public void UpdateScene()
    {
        spawnLocations = FindObjectsByType<NPCSpawnPoint>(sortMode: FindObjectsSortMode.None);
        nPCRegistry.Clear();
        List<GameObject> npcCopies = new(npcPrefabs); 
        System.Random random = new();
        foreach (NPCSpawnPoint spawnPoint in spawnLocations)
        {
            if(npcCopies.Count > 0 && spawnPoint.GetComponentInChildren<NPC>() == null)
            {
                int index = random.Next(1, npcCopies.Count)-1;
                GameObject gameObject = Instantiate(npcCopies[index], spawnPoint.location);
                nPCRegistry.Add(gameObject.GetComponent<NPC>());
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
