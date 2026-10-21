using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One interviewer in the interviewer picker. A null agent is the standard interviewer.
/// </summary>
[RequireComponent(typeof(Button))]
public class InterviewerButton : MonoBehaviour
{
    public TMP_Text NameText;

    /// <summary>
    /// Optional. Shows the short description the counsellor wrote.
    /// </summary>
    public TMP_Text DescriptionText;

    /// <summary>
    /// Optional. Shown when this interviewer is the picked one.
    /// </summary>
    public GameObject SelectedMarker;

    private Button button;

    public PublishedAgent Agent { get; private set; }

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetAgent(PublishedAgent agent, string standardName, Action<InterviewerButton> onClick)
    {
        Agent = agent;
        NameText.text = agent != null ? agent.Name : standardName;
        if (DescriptionText != null) DescriptionText.text = agent != null ? agent.Description : "";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(this));
    }

    public void SetSelected(bool selected)
    {
        if (SelectedMarker != null) SelectedMarker.SetActive(selected);
    }
}
