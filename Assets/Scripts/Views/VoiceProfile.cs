using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoiceProfile 
{        
    [SerializeField] private string displayName;
    [SerializeField] private string id;
    [SerializeField] private List<string> models = new();
    [SerializeField] private string language;

    public VoiceProfile(string id, string displayname, IEnumerable<string> models, string language = "no")
    {
        this.displayName = displayname;
        this.id = id;
        this.models = new(models);
        this.language = language;
    }
    public VoiceProfile(string id, string displayname, string language = "no")
    {
        this.displayName = displayname;
        this.id = id;
        this.language = language;
    }

    public string getId()
    {
        return id;
    }

    public string GetDisplayName()
    {
        return displayName;
    }
}
