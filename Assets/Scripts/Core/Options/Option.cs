using System;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

[Serializable]
public class OptionBase
{

    [SerializeField] private string name;

    public OptionBase(string name)
    {
        if (name.IsNotNullOrEmpty())
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