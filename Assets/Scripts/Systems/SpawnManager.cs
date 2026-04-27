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
    public HashSet<GameObject> npcPrefabs = new();
    

    public void UpdateScene()
    {
        spawnLocations = FindObjectsByType<NPCSpawnPoint>(sortMode: FindObjectsSortMode.None);
        nPCRegistry.Clear();
        foreach (NPC item in FindObjectsByType<NPC>(sortMode: FindObjectsSortMode.None))
        {
            nPCRegistry.Add(item);
        }
        
    }

    void Awake()
    {
        //SceneManager.activeSceneChanged += OnSceneChange;
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
