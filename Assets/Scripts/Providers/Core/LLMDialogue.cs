/// <summary>
/// LLMDialogue is a representation to be sent
/// </summary>
public class LLMDialogue
{
    /// <summary>
    /// Creates a LLMDialogue from a Dialogue and Role 
    /// </summary>
    /// <param name="Dialogue">The Dialogue</param>
    /// <param name="Role">A role</param>
    public LLMDialogue(string Dialogue, RoleCategory Role)
    {
        this.Role = Role;
        this.Dialogue = Dialogue;
    }

    /// <summary>
    /// The role the dialogue was on
    /// </summary>
    public readonly RoleCategory Role;

    /// <summary>
    /// The dialogue in string
    /// </summary>
    public readonly string Dialogue;

    /// <summary>
    /// The different role types
    /// </summary>
    public enum RoleCategory
    {
        SYSTEM,
        USER,
        ASSISTANT
    }
}