using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LocalJobs : IJobSelectionService
{

    readonly List<JobContainer> jobs;

    public LocalJobs(IEnumerable<JobContainer> jobContainers)
    {
        jobs = new(jobContainers);
    }

    public async Task<(IEnumerable<JobContainer>, bool)> GetJobs(int page = 1, int amount = 9)
    {
        bool hasNextPage = false;
        if(jobs.Count() < (page - 1) * amount)
        {
            throw new ArgumentException("Out of scope with the amount of jobs");
        }
        if(jobs.Count() > page * amount)
        {
            hasNextPage = true;
        }
        int startIndex = (page-1)*amount;
        int endIndex = Math.Min(jobs.Count(),page*amount);
        return (jobs.GetRange(startIndex, endIndex-startIndex), hasNextPage);
        
    }
}