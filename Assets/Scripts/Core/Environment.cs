using System;
using System.Collections.Generic;

/// <summary>
/// A enviroment contains jobs and actors that the candidate can try out
/// These work in a linear fashion where:
/// Enviroment -> Jobs -> Actors
/// This is to create dependencies so a shop or resturants can't have for example farmers. 
/// </summary>
public class Environment {
    private string name = "";
    private string description = "";

    private HashSet<Job> jobs = new();

    public Environment(string name, List<Job> jobs)
    {
        jobs.ForEach(job => this.jobs.Add(job));
        this.name = name;
    }

    /// <summary>
    /// A potential job opening the candidate can try to interview about.  
    /// </summary>
    public class Job
    {
        public static Job Store = new("Store worker", "A store worker for a store. Standard work tasks being filling shelfs and helping customers.");
        public static Job Sales = new("Phone Seller", "Phone seller within a phone center. Standard work includes convincing customer in shifting providers, make them buy goods or handle complaints/questions.");
        public static Job Post = new("Postal Serice", "Postman delivering packages. Needs to have a access to a driving license. ");
        public static Job Cleaner = new("Cleaner",  "");
        public static Job Assistant = new("Assistant","");
        public static HashSet<Job> All = new() { Cleaner, Store, Post, Sales, Assistant };
        public string name {get; private set;} = ""; 
        public string description {get; private set;} = "";
        /// <summary>
        /// Creates a Job. Only meant to be called by the class itself to create static job
        /// The candidate and the director is automatically added to the list 
        /// </summary>
        /// <param name="name">Name of the job</param>
        /// <param name="actors">The different Actors that could be in the interview panel</param>
        /// <param name="description">A Description of the role, not mandatory</param>
        public Job(string name, string description = "")
        {
            if (name == null || name == "")
            {
                throw new ArgumentException("EYOO, This is not allowed. Fix you name");
            }
            this.name = name;
            this.description = description;
        }

        public override string ToString()
        {
            return string.Format("### {0}:\n{1} ",name, description);
        }
    }

    /// <summary>
    /// Actors that is within a scene or interview
    /// </summary>
    public class Actor
    {
        public static readonly Actor Director = new("Director", "A actor that tells other actors what to do. Their dialogue is always a instructions to the next actor. CAN NEVER BE REFERENCED OR TALKED TO");
        public static readonly Actor Candidate = new("Candidate", "The candidate you are interviewing. YOU CAN NEVER BECOME THIS ROLE");
        public static readonly Actor Manager = new("Manager", "");
        public static readonly Actor HiringManger = new("Hiring Manager",
        "You are the one that has called in the candidate for a interview. You assume this is one of your potential candidates looking trough their CV. You are here to confirm this trough what you think the company needs");
        public static readonly Actor HR = new("HR", "Your main focus should be in assesing how you think the candidate would fit in with the rest of the company.");
        public static readonly Actor Expert = new("Expert", "You are here to assess the persons skills. You are familiar with most of the tasks they will undertake and are interested in knowing what they would do in rare scenarios.");
        public static readonly Actor Employee = new("Employee", "");

        public static readonly List<Actor> NPCactors = new() {Manager, HiringManger, HR, Expert, Employee}; 
        private string name;
        private string roleDescription;

        private Actor(string name, string roleDescription)
        {
            this.name = name;
            this.roleDescription = roleDescription;
        }

        public string GetString()
        {
            string start = "### {0} \n {1} \n";
            return string.Format(start, name, roleDescription);
        }

        public string getName()
        {
            return name;
        }

    }


}
