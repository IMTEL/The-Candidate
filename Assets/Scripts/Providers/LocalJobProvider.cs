using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/LocalJobService")]
public class LocalJobProvider : JobSelection
{

    [ShowInInspector]
    public List<JobContainer> jobs = new()
    {
        new(Environment.Job.Assistant),
        new(Environment.Job.Cleaner),
        new(Environment.Job.Post),
        new(Environment.Job.Store),
        new(Environment.Job.Sales)
    };

    public override IJobSelectionService CreateTypedService()
    {
        return new LocalJobs(jobs); 
    }
}