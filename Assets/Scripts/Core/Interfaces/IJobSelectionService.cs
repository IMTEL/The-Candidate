using System.Collections.Generic;
using System.Threading.Tasks;

public interface IJobSelectionService
{
    /// <summary>
    /// Gives out a IEnumerable of jobs that is on a certain page     
    /// </summary>
    /// <param name="page">the page</param>
    /// <param name="amount">A amount of jobs that they want</param>
    /// <returns>Task with a tuple. Item 1 consist of the jobs that got found. The second is a <see langword="bool"/> to identify if there is a next page</returns>
    public Task<(IEnumerable<JobContainer>, bool)> GetJobs(int page = 1, int amount = 9);

}