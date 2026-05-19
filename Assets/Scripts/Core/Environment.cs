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
        public static Job Store = new("Butikkmedarbeider", "En butikkmedarbeider kan arbeide innenfor mange forskjellige bransjer, for eksempel dagligvarer, klær, sport eller kosmetikk. Som butikkmedarbeider må du ha kunnskap om produktene du skal selge for å kunne gi kundene den informasjonen og veiledningen de trenger.");
        public static Job Sales = new("Telefonselger", "En telefonselger er en salgsmedarbeider som jobber med salg av ulike varer og tjenester over telefon. Som telefonselger består jobben din av salg, markedsføring og kundebehandling over telefon til privatkunder eller bedrifter. For å lykkes i jobben bør du ha god kunnskap om produktene og kundene. ");
        public static Job Post = new("Postbud", "Som postbud består arbeidet i å levere post til private husstander og bedrifter. Postbud distribuerer all type post - alt av brev, pakker, blader, aviser, reklame og annet. Utdelingen foregår utendørs, i hovedsak i tidsrommet fra klokka 07.00 til 17.00. Stort sett foregår utdelingen til fots, med sykkel, moped eller bil, avhengig av arbeidssted.");
        public static Job Cleaner = new("Renholdsoperatør",  "Renholdsoperatøren utfører daglig rengjøring i offentlige og private bygg som kontorer, sykehus, skoler, industribygg, tog, buss og andre steder der mennesker oppholder seg på jobb og i fritiden. Arbeidet kan innebære oppgaver som gulvrengjøring, vinduspuss, smitteforebyggende renhold og bruk av avanserte rengjøringsmaskiner. Moderne renhold handler ikke bare om rengjøring. Det handler også om å ta gode valg for klima og miljø og bruk av utstyr som reduserer helsebelastningen og øker effektiviteten.");
        public static Job Assistant = new("Sekretær","Sekretærer spiller en viktig rolle i den daglige driften av kontorer og arbeidsplasser. De har ansvar for en rekke administrative oppgaver, som er sentrale for å sikre en effektiv og velfungerende arbeidshverdag. Sekretærer er ofte en nøkkelperson i organisasjonen, og deres arbeid er avgjørende for å sikre at arbeidsflyten er jevn og produktiv.");
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
        public static readonly Actor Manager = new("Daglig Leder", "");
        public static readonly Actor HiringManger = new("Hiring Manager",
        "You are the one that has called in the candidate for a interview. You assume this is one of your potential candidates looking trough their CV. You are here to confirm this trough what you think the company needs");
        public static readonly Actor HR = new("HR", "Your main focus should be in assesing how you think the candidate would fit in with the rest of the company.");
        public static readonly Actor Expert = new("Expert", "You are here to assess the persons skills. You are familiar with most of the tasks they will undertake and are interested in knowing what they would do in rare scenarios.");
        public static readonly Actor Employee = new("Ansatt", "");

        public static readonly List<Actor> NPCactors = new() {Manager, HiringManger, HR, Expert, Employee}; 
        private readonly string name;
        private readonly string roleDescription;

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
