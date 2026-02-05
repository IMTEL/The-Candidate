using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIStartManager : MonoBehaviour
{
    public Dropdown jobs;
    public Dropdown amount;
    private List<Environment.Job> jobList;
    ScenarioSettings scenarioSettings;

    public Button startbutton;

    [ShowInInspector]
    public HashSet<Environment.Job> jobsAvailable = Environment.Job.All;

    // Start is callsed once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jobs.ClearOptions();
        jobList = jobsAvailable.ToList();
        List<Dropdown.OptionData> jobNames = new();
        jobList.ForEach(delegate(Environment.Job job)
        {
            jobNames.Add(new(job.name));
        });
        jobs.AddOptions(jobNames);
        scenarioSettings = Services.scenarioSettings;
        scenarioSettings.SetAmountOfPeople(jobList[0].actors.Count);
        scenarioSettings.SetJob(jobList[0]);
        updateAmountDropdown(jobList[0]);
        jobs.onValueChanged.AddListener(delegate
        {
            OnChangeJob(jobs);
        });
        amount.onValueChanged.AddListener(delegate
        {
            OnChangeAmount(amount);
        });
    }

     
    void OnChangeJob(Dropdown dropdown)
    {
        Environment.Job job = jobList[dropdown.value];
        if (scenarioSettings.amountOfPeople > job.actors.Count)
        {
            updateAmountDropdown(job, job.actors.Count);
        }
        scenarioSettings.SetJob(job);
    }

    private void updateAmountDropdown(Environment.Job job,  int setAmount = 1)
    {
        List<Dropdown.OptionData> amountlist = new();
        for(int i = 1; i <= jobList[0].actors.Count; i++)
        {
            amountlist.Add(new Dropdown.OptionData(i.ToString()));
        }
        amount.ClearOptions();
        amount.AddOptions(amountlist);
        amount.value = setAmount-1;
    }

    void OnChangeAmount(Dropdown dropdown)
    {
        scenarioSettings.SetAmountOfPeople(dropdown.value+1);
    }

    public void OnStartButtonPressed()
    {
        SceneManager.LoadScene("ConferanceRoom", LoadSceneMode.Single);
    }
}
