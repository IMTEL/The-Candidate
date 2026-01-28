using System.Collections.Generic;
using UnityEngine;

public class NPCRegistry : MonoBehaviour
{

    private List<NPC> NPCs= new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<NPC> GetNPCs()
    {
        return NPCs;
    }
}
