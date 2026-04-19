using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Manager for the job display  
/// </summary>
public class JobSelectionManager : MonoBehaviour
{

    public List<JobSelection> jobServices; 

    public List<GameObject> JobContainers;

    private int page = 0;

    [Tooltip("This is divided upon the total amount of containers for the outlay")]
    public int PerPageAmount {set
        {
            if(value < 0)
            {
                value = 1;
            }
            PerPageAmount = value;
            CreateJobButtons();
            SetJobs();
        }
        get
        {
            return PerPageAmount;
        }
    }

    public GameObject jobButtonPrefab;

    public GameObject serviceButtonPrefab;

    public GameObject ServiceContainer;

    private IJobSelectionService jobservice;

    public Button NextPageButton;

    public Button PreviousPageButton;

    public void Awake()
    {
        foreach (JobSelection item in jobServices)
        {
            GameObject serviceButton = Instantiate(serviceButtonPrefab,ServiceContainer.transform);
            serviceButton.GetComponent<Button>().onClick.AddListener(() => {SwitchJobService(item);});
            serviceButton.GetComponent<RawImage>().texture = item.GetIcon();
        }
        if(jobServices.Count > 0)
        {
            SwitchJobService(jobServices[0]);
        }
        NextPageButton.onClick.AddListener(NextPage);
        PreviousPageButton.onClick.AddListener(PreviousPage);
        PreviousPageButton.interactable = false;
    }

    private void SwitchJobService(JobSelection jobSelectionService)
    {
        if (jobServices.Contains(jobSelectionService))
        {
            page = 0;
            jobservice = jobSelectionService.CreateTypedService();
            SetJobs();
        }
    }
    
    private void SetJobs()
    {
        int qoutinent = Math.DivRem(JobContainers.Count(),PerPageAmount,out _);
        IEnumerator<Environment.Job> jobs = jobservice.GetJobs(page, amount: PerPageAmount).GetEnumerator();
        for(int i = 0; jobs.MoveNext(); i++)
        {
            int y = Math.DivRem(i, qoutinent, out int x);

        }
    }

    private void NextPage()
    {
        page += 1;
        SetJobs();
    }

    private void PreviousPage()
    {
        page -= 1;
        SetJobs();
    }

    private void CreateJobButtons()
    {
        for (int i = 0; i < PerPageAmount; i++)
        {
            
        }
    }
}