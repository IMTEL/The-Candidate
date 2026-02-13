/// <summary>
/// Settings for the scenario
/// </summary>
public class ScenarioSettings {

    public Environment.Job job {get; private set;} = Environment.Job.Store;

    public int amountOfPeople {get; private set;} = 1;

    public void SetAmountOfPeople(int amount)
    {
        this.amountOfPeople = amount > job.actors.Count && amount > 0 ? amount  : amountOfPeople;
    }

    /// <summary>
    /// Will set the Job for the Scenario
    /// If the amount of actors is less then previous amount, it will reset the amount to the highest amount
    /// </summary>
    /// <param name="enviroment"></param>
    public void SetJob(Environment.Job job)
    {
        this.job = job ?? this.job;
        if(job.actors.Count > amountOfPeople)
        {
            SetAmountOfPeople(job.actors.Count);
        }
    }
}
