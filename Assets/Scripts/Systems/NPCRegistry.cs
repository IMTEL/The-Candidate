using System.Collections.Generic;

public class NPCRegistry : INPCEventListener
{

    private readonly HashSet<NPC> NPCs= new();

    public IEnumerable<NPC> GetNPCEnumerable()
    {
        return new List<NPC>(NPCs);
    }

    public void OnCreateNPC(NPC nPC)
    {
        NPCs.Add(nPC);
        nPC.AddListener(this);
    }

    public void OnDestroyNPC(NPC nPC)
    {
        NPCs.Remove(nPC);
    }

    public void Add(NPC npc)
    {
        NPCs.Add(npc);
    }

    public void Clear()
    {
        NPCs.Clear();
    }
}
