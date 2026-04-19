using System.Collections.Generic;

public interface IJobSelectionService
{
    public IEnumerable<Environment.Job> GetJobs(int page, int amount = 9);
    public bool HasNextPage();
    public bool HasPreviousPage();

}