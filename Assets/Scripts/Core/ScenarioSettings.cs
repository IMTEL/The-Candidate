using System.Collections.Generic;
using Newtonsoft.Json.Utilities;

/// <summary>
/// Settings for the scenario
/// </summary>
public class ScenarioSettings {

    public Environment.Job job {get; private set;} = Environment.Job.Store;

    private Dictionary<Environment.Actor, int> actors = new();


    public ScenarioSettings()
    {
        foreach (Environment.Actor item in Environment.Actor.NPCactors)
        {
            actors.Add(item, 0);
        }
    }

    /// <summary>
    /// Will set the Job for the Scenario
    /// If the amount of actors is less then previous amount, it will reset the amount to the highest amount
    /// </summary>
    /// <param name="enviroment"></param>
    public void SetJob(Environment.Job job)
    {
        this.job = job ?? this.job;
    }

    public void SetAmount(Environment.Actor actorType, int amount)
    {
        if(!actors.ContainsKey(actorType)) return;
        if (amount < 0) amount = 0;
        actors[actorType] = amount;
    }

    public IEnumerable<(Environment.Actor actor, int amount)> GetActors()
    {
        List<(Environment.Actor, int)> list = new();
        foreach (Environment.Actor item in actors.Keys)
        {
            list.Add((item, actors[item]));
        }
        return list;
    }

    public int GetActorAmount(Environment.Actor actor)
    {
        if(!actors.ContainsKey(actor)) return 0;
        return actors[actor];
    }
}
