using System.Collections.Generic;

public class NPCRegistry : INPCEventListener
{

    private List<NPC> NPCs= new();

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
}
