using System;
using UnityEngine;

[Serializable]
public class OptionBase
{

    [SerializeField] private string name;

    public OptionBase(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name can't be empty or null");
        }
        this.name = name;
    }
    
    /// <summary>
    /// Returns the name of the option
    /// </summary>
    /// <returns>The name</returns>
    public string GetName()
    {
        return name;
    }
}