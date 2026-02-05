using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

public class SpawnManager : MonoBehaviour, IEvent<INPCEventListener>
{
    [ShowInInspector]
    private NPCSpawnPoint[] spawnLocations;

    private Action<NPC> OnCreateNPC;

    [ShowInInspector]
    [LabelText("NPCs ")]
    public HashSet<GameObject> NPCs {get; private set;} = new();

    public void AddListener(IEnumerable<INPCEventListener> listeners)
    {
        foreach (INPCEventListener item in listeners)
        {
            AddListener(item);
        }
    }

    public void AddListener(INPCEventListener listener)
    {
        OnCreateNPC += listener.OnCreateNPC;
    }

    public void RemoveListener(IEnumerable<INPCEventListener> listeners)
    {
        foreach (INPCEventListener item in listeners)
        {
            AddListener(item);
        }
    }

    public void RemoveListener(INPCEventListener listener)
    {
        OnCreateNPC -= listener.OnCreateNPC;
    }

    void Start()
    {
        spawnLocations = FindObjectsByType<NPCSpawnPoint>(sortMode: FindObjectsSortMode.None);
    }


}
