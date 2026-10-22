/// <summary>
/// An interviewer a counsellor has published to The Candidate from the agent configurator.
/// Only holds what RAGdoll shares publicly: the name and short description the counsellor
/// wrote when publishing. Never the prompt or anything about the job seeker.
/// </summary>
public class PublishedAgent
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public PublishedAgent(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description ?? "";
    }
}
