/// <summary>
/// An interviewer a counsellor has published to The Candidate from the agent configurator.
/// Only holds what RAGdoll shares publicly: never the prompt or anything about the job seeker.
/// </summary>
public class PublishedAgent
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    /// <summary>
    /// The RAGdoll role to chat as. Null if the agent has no roles.
    /// </summary>
    public string Role { get; }

    public PublishedAgent(string id, string name, string description, string role)
    {
        Id = id;
        Name = name;
        Description = description ?? "";
        Role = role;
    }
}
