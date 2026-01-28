using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [ShowInInspector]
    public HashSet<Transform> spawnLocations {get; private set;} = new();

    [ShowInInspector]
    public HashSet<GameObject> npcs {get; private set;} = new();
}
