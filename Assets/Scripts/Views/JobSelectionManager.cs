using System;
using System.Collections.Concurrent;
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

    public SpriteImageAnimation animator;

    [ShowInInspector]
    private int page = 1;
    private int perPageAmount = 8;

    public int GetPerPageAmount()
    {
        return perPageAmount;
    }

    [Button]
    public void SetPerPageAmount(int value)
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

    private readonly ConcurrentDictionary<(IJobSelectionService, int), Task<(IEnumerable<JobContainer> jobs, bool nextpage)>> jobtasks = new();

    private void Start()
    {
        page = 1;
        CreateJobButtons();
        foreach (JobSelection item in jobServices)
        {
            GameObject serviceButton = Instantiate(serviceButtonPrefab, ServiceContainer.transform);
            serviceButton.GetComponent<Button>().onClick.AddListener(() => { SwitchJobService(item); });
            serviceButton.GetComponent<RawImage>().texture = item.GetIcon();
        }
        if (jobServices.Count > 0)
        {
            SwitchJobService(jobServices[0]);
        }
        NextPageButton.onClick.AddListener(NextPage);
        PreviousPageButton.onClick.AddListener(PreviousPage);
        PreviousPageButton.interactable = false;
    }

    private void Update()
    {
        Task<(IEnumerable<JobContainer> jobs, bool nextpage)> task;
        lock (jobtasks)
        {
            jobtasks.TryGetValue((jobservice, page), out task);
            if (task == null || !task.IsCompleted) return;
            jobtasks[(jobservice, page)] = null;
        }
        animator.StopAnimation();
        IEnumerator<JobContainer> jobEnumerator = task.Result.jobs.GetEnumerator();
        foreach (GameObject row in JobContainers)
        {
            JobButton[] buttons = row.GetComponentsInChildren<JobButton>();
            int i;
            for (i = 0; buttons.Count() > i && jobEnumerator.MoveNext(); i++)
            {
                buttons[i].SetJob(jobEnumerator.Current);
            }
            for (; i < buttons.Count(); i++)
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
            SetPage(1);
            jobservice = jobSelectionService.CreateTypedService();
            StartFetchingJobs();
        }
    }

    private void StartFetchingJobs()
    {
        jobservice ??= jobServices[0].CreateTypedService();
        jobtasks.TryGetValue((jobservice, page), out Task<(IEnumerable<JobContainer> jobs, bool nextpage)> task);
        //Reset the buttons if we need to fetch 
        foreach (GameObject row in JobContainers)
        {
            JobButton[] buttons = row.GetComponentsInChildren<JobButton>();
            for (int i = 0; i < buttons.Count(); i++)
            {
                buttons[i].SetJob(null);
            }
        }
        //Start Doing loading animations
        animator.PlayAnimation();
        //Disable both buttons
        NextPageButton.interactable = false;
        PreviousPageButton.interactable = page > 1;

        //If the user has not already invoked this service or the fetching of this already.
        if (task == null)
        {
            //Build thread and start the fetching
            Thread thread = new(ThreadMethod);
            thread.Start();
        }


    }

    private async void ThreadMethod()
    {
        jobtasks[(jobservice, page)] = jobservice.GetJobs(page, amount: GetPerPageAmount());
        jobtasks[(jobservice, page)].Wait();
    }

    private void NextPage()
    {
        SetPage(page + 1);
        StartFetchingJobs();

    }

    private void PreviousPage()
    {
        SetPage(page - 1);
        StartFetchingJobs();
    }

    private void SetPage(int page)
    {
        this.page = page <= 0 ? 1 : page;
        pageNumber.text = this.page.ToString();
    }

    private void CreateJobButtons()
    {
        //We can clear everything... or just the excess on every row
        int amount = Math.DivRem(perPageAmount, JobContainers.Count(), out int reminder);
        foreach (GameObject row in JobContainers)
        {
            JobButton[] jobButtons = row.GetComponentsInChildren<JobButton>();
            int count = jobButtons.Count();

            if (jobButtons.Count() <= amount + (reminder > 0 ? 1 : 0))
            {
                for (int i = 0; i < amount + reminder - count; i++)
                {
                    Instantiate(jobButtonPrefab, row.transform);
                }
            }
            else
            {
                for (int i = 0; i < count - (amount + (reminder > 0 ? 1 : 0)); i++)
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