using System;
using UnityEngine;

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

    public abstract bool TrySetValue(object value);

    public abstract object GetObjectValue();

    // override object.Equals
    public override bool Equals(object obj)
    {
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        OptionBase casted = (OptionBase)obj;
        // TODO: write your implementation of Equals() here
        return casted.name.Equals(this.name) &&
                casted.GetHashCode().Equals(this.GetHashCode());
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return 17 * 23 * name.GetHashCode();
    }
}