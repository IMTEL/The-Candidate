using UnityEngine;

public class JobContainer
{
    
    private Texture2D icon;

    private Environment.Job job;
    public JobContainer(Environment.Job job, Texture2D icon = null)
    {
        this.job = job; 
        this.icon = icon;
    }

    public Texture2D GetTexture2D()
    {
        return icon;
    }

    public Environment.Job GetJob()
    {
        return job;
    }
}