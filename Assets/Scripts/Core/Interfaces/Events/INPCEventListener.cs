public interface INPCEventListener
{
    /// <summary>
    /// Event for the creation of NPC 
    /// </summary>
    /// <param name="nPC">the NPC created</param>
    public void OnCreateNPC(NPC nPC);

    /// <summary>
    /// Event for when a NPC gets destroyed
    /// </summary>
    /// <param name="nPC">The npc that gets destroyed</param>
    public void OnDestroyNPC(NPC nPC);
}