using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [ShowInInspector]
    private NPCSpawnPoint[] spawnLocations;

    [ShowInInspector]
    [LabelText("NPCs ")]
    public HashSet<GameObject> NPCs {get; private set;} = new();

    void Start()
    {
        spawnLocations = FindObjectsByType<NPCSpawnPoint>(sortMode: FindObjectsSortMode.None);
        
    }

}
