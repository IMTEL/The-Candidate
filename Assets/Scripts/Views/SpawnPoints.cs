using System;
using UnityEngine;

public class NPCSpawnPoint : MonoBehaviour
{
    public Enum spawnID; 
    
    public enum spawnIDs
    {
        Interviewer,
        BackgroundNPC
    }

    Animation animationcycle;
}