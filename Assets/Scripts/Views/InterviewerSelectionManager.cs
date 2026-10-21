using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI for picking one of the interviewers counsellors have published, or the standard interviewer.
/// The pick is saved in the ScenarioSettings and used when the NPCs spawn (see SpawnManager).
/// </summary>
public class InterviewerSelectionManager : MonoBehaviour
{
    public PublishedRagProvider publishedAgents;

    /// <summary>
    /// A prefab with an InterviewerButton
    /// </summary>
    public GameObject interviewerButtonPrefab;

    /// <summary>
    /// Where the buttons are placed, for example an object with a Vertical Layout Group
    /// </summary>
    public Transform buttonContainer;

    public string standardInterviewerName = "Standard intervjuer";

    private readonly List<InterviewerButton> buttons = new();

    private async void Start()
    {
        ScenarioSettings settings = BootStrap.Instance.GetComponentInChildren<ScenarioSettingsWrapper>().getScenarioSettings();

        // The standard interviewer is always there, also when RAGdoll can't be reached
        CreateButton(null, settings);
        List<PublishedAgent> agents = await publishedAgents.GetService().GetPublishedAgents();
        // The menu may have been closed while waiting
        if (this == null) return;
        foreach (PublishedAgent agent in agents)
        {
            CreateButton(agent, settings);
        }
        MarkSelected(settings.interviewer);
    }

    private void CreateButton(PublishedAgent agent, ScenarioSettings settings)
    {
        InterviewerButton button = Instantiate(interviewerButtonPrefab, buttonContainer).GetComponent<InterviewerButton>();
        button.SetAgent(agent, standardInterviewerName, clicked =>
        {
            settings.SetInterviewer(clicked.Agent);
            MarkSelected(clicked.Agent);
        });
        buttons.Add(button);
        MarkSelected(settings.interviewer);
    }

    private void MarkSelected(PublishedAgent agent)
    {
        foreach (InterviewerButton button in buttons)
        {
            button.SetSelected(agent == null ? button.Agent == null : button.Agent?.Id == agent.Id);
        }
    }
}
