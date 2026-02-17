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

    void Start()
    {
        spawnLocations = FindObjectsByType<NPCSpawnPoint>(sortMode: FindObjectsSortMode.None);
    }

    [Init]
    public void Init()
    {
        nPCRegistry.Add(GameObject.FindAnyObjectByType<NPC>());
    }

    void INPCEventListener.OnCreateNPC(NPC nPC)
    {
        
    }

    public void OnDestroyNPC(NPC nPC)
    {
        throw new NotImplementedException();
    }

    public NPCRegistry GetNPCRegistry()
    {
        return nPCRegistry;
    }
}
