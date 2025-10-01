using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A enviroment contains jobs and actors that the candidate can try out
/// These work in a linear fashion where:
/// Enviroment -> Jobs -> Actors
/// This is to create dependencies so a shop or resturants can't have for example farmers. 
/// </summary>
public class Environment
{
    private string name = "";
    private HashSet<Job> jobs = new();

    private Environment(string name, List<Job> jobs)
    {
        jobs.ForEach(job => this.jobs.Add(job));
        this.name = name;
    }

    /// <summary>
    /// A potential job opening the candidate can try to interview about.  
    /// </summary>
    public class Job
    {
        private string name = "";

        private string description = "";
        private HashSet<Actor> actors = new();
        /// <summary>
        /// Creates a Job. Only meant to be called by the class itself to create static job
        /// The candidate and the director is automatically added to the list 
        /// </summary>
        /// <param name="name">Name of the job</param>
        /// <param name="actors">The different Actors that could be in the interview panel</param>
        /// <param name="description">A Description of the role, not mandatory</param>
        private Job(string name, List<Actor> actors, string description = "")
        {
            if (name == null || name == "")
            {
                throw new ArgumentNullException("EYOO, THis is not allowed. Fix you name");
            }
            if (actors == null || !actors.Any() || actors.Any((actor) => actor == null))
            {
                throw new ArgumentException("EYOO, fix your list of actors. It should not contain null, be null or be empty"); 
            }
            this.actors.Add(Actor.Candidate);
            this.actors.Add(Actor.Director);
            actors.ForEach(actor => this.actors.Add(actor));
            this.name = name;
            this.description = description;
        }
    }

    /// <summary>
    /// Actors that is within a scene or interview
    /// </summary>
    public class Actor
    {
        public static Actor Director = new("Director", "A actor that tells other actors what to do. Their dialogue is always a instructions to the next actor. CAN NEVER BE REFERENCED OR TALKED TO");
        public static Actor Candidate = new("Candidate", "The candidate you are interviewing. YOU CAN NEVER BECOME THIS ROLE");
        public static Actor Manager = new("Manager", "");
        public static Actor HiringManger = new("Hiring Manager",
        "You are the one that has called in the candidate for a interview. You assume this is one of your potential candidates looking trough their CV. You are here to confirm this trough what you think the company needs");
        public static Actor HR = new("HR", "Your main focus should be in assesing how you think the candidate would fit in with the rest of the company.");
        public static Actor Expert = new("Expert", "You are here to assess the persons skills.");
        public static Actor Employee = new("Employee", "");

        private string name;
        private string roleDescription;

        private Actor(string name, string roleDescription)
        {
            this.name = name;
            this.roleDescription = roleDescription;
        }

        public string getPrompt()
        {
            string start = "### %s \n %s \n";
            return string.Format(start, name, roleDescription);
        }

        public string getName()
        {
            return name;
        }
    }


}
