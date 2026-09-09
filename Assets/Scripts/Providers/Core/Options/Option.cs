using System;
using UnityEngine;

/// <summary>
/// OptionBase is a class used by all options. They contain the following characteristic
/// 1. They have a name/key specific to that option
/// 2. When comparing options together only attributes that does not involve the value are to be used.
/// </summary>
[Serializable]
public abstract class OptionBase
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

    /// <summary>
    /// Tries to set the value of the option
    /// </summary>
    /// <param name="value">The value to set the option to</param>
    /// <returns>True or false depending if it could</returns>
    public abstract bool TrySetValue(object value);
    
    /// <summary>
    /// Gets the object value
    /// </summary>
    /// <returns>A object that is the value of the option. Need to be casted to the correct type</returns>
    public abstract object GetObjectValue();

    // override object.Equals
    public override bool Equals(object obj)
    {
        
        if (obj == null || !GetType().Equals(obj.GetType()))
        {
            return false;
        }
        
        OptionBase casted = (OptionBase)obj;
        return casted.name.Equals(name);
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return 17 * 23 * name.GetHashCode();
    }

    public abstract OptionBase Clone();
}