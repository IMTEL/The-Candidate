using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alchemy.Inspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Manager for the job display  
/// </summary>
public class JobSelectionManager : MonoBehaviour
{

    public List<JobSelection> jobServices; 

    public List<GameObject> JobContainers;

    [ShowInInspector]
    private int page = 1;
    private int perPageAmount = 8;

    public int GetPerPageAmount()
    {
        return perPageAmount;
    }

    [Button]
    public async void SetPerPageAmount(int value)
    {
        if (value < 0)
        {
            value = 1;
        }
        perPageAmount = value;
        CreateJobButtons();
        #if !UNITY_EDITOR 
            StartFetchingJobs();
        #endif
    }

    public GameObject jobButtonPrefab;

    public GameObject serviceButtonPrefab;

    public GameObject ServiceContainer;

    private IJobSelectionService jobservice;

    public Button NextPageButton;

    public TMP_Text pageNumber;

    public Button PreviousPageButton;

    private Dictionary<IJobSelectionService, Task<(IEnumerable<JobContainer> jobs, bool nextpage)>> jobtasks = new();

    public void Awake()
    {
        page = 1;
        foreach (JobSelection item in jobServices)
        {
            GameObject serviceButton = Instantiate(serviceButtonPrefab,ServiceContainer.transform);
            serviceButton.GetComponent<Button>().onClick.AddListener(async () => {SwitchJobService(item);});
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

    private void Update()
    {
        jobtasks.TryGetValue(jobservice, out Task<(IEnumerable<JobContainer> jobs, bool nextpage)> task);
        if(task == null || !task.IsCompleted) return;
        jobtasks[jobservice] = null;
        IEnumerator<JobContainer> jobEnumerator = task.Result.jobs.GetEnumerator();
        foreach (GameObject row in JobContainers)
        {
            JobButton[] buttons = row.GetComponentsInChildren<JobButton>();
            int i;
            for (i = 0; buttons.Count() > i && jobEnumerator.MoveNext(); i++)
            {
                buttons[i].SetJob(jobEnumerator.Current);
            }
            for (;i < buttons.Count(); i++)
            {
                buttons[i].SetJob(null);
            }
        }
        NextPageButton.interactable = task.Result.nextpage;
        PreviousPageButton.interactable = page > 1;
    }

    private void SwitchJobService(JobSelection jobSelectionService)
    {
        if (jobServices.Contains(jobSelectionService))
        {
            page = 0;
            jobservice = jobSelectionService.CreateTypedService();
            StartFetchingJobs();
        }
    }
    
    private void StartFetchingJobs()
    {
        jobservice ??= jobServices[0].CreateTypedService();
        page = page > 0 ? page : 1;
        jobtasks.TryGetValue(jobservice, out Task<(IEnumerable<JobContainer> jobs, bool nextpage)> task);
        if(task == null)
        {
            Thread thread = new(ThreadMethod);
            thread.Start();            
        }


    }

    private async void ThreadMethod()
    {
        jobtasks[jobservice] = jobservice.GetJobs(page, amount: GetPerPageAmount());
        jobtasks[jobservice].Wait();
    }

    private void NextPage()
    {
        page += 1;
        pageNumber.text = page.ToString();
        StartFetchingJobs();
        
    }

    private void PreviousPage()
    {
        page -= 1;
        pageNumber.text = page.ToString();
        StartFetchingJobs();
    }

    private void CreateJobButtons()
    {
        //We can clear everything... or just the excess on every row
        int amount = Math.DivRem(perPageAmount, JobContainers.Count(), out int reminder);
        foreach (GameObject row in JobContainers)
        {
            JobButton[] jobButtons = row.GetComponentsInChildren<JobButton>();
            int count = jobButtons.Count();
            
            if(jobButtons.Count() <= amount + (reminder > 0 ? 1 : 0))
            {
                for(int i = 0; i < amount + reminder - count; i++)
                {
                    Instantiate(jobButtonPrefab, row.transform);
                } 
            }
            else
            {
                for(int i = 0; i < count - (amount + (reminder > 0 ? 1 : 0)); i++)
                {
                    //Maybe just disable them and instantiate when we need to? I don't know
                    #if UNITY_EDITOR 
                        DestroyImmediate(jobButtons.Last().gameObject);
                    #else 
                        Destroy(jobButtons.Last().gameObject);
                    #endif   
                } 
            }
            reminder += reminder > 0 ? -1 : 0;
        }
    }
}