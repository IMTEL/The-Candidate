using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PickOption<T> : OptionBase
{
    [SerializeField] private T value;
    [SerializeField] private List<T> options;
    
    public PickOption(string name, T defualtValue, IEnumerable<T> values) : base(name) {
        if(values == null || values.Count() == 0)
        {
            throw new ArgumentException("The values has to be a list of options with 1 or more options");    
        }
        options = new List<T>(values);
        SetValue(defualtValue);
    }

    public T GetValue()
    {
        return value;
    }

    public IEnumerable<T> GetOptions()
    {
        return options;
    }

    public bool SetValue(T value)
    {
        if(value == null || !options.Contains(value))
        {
            throw new ArgumentException("Value has to be within the list of options and not null");
        }
        this.value = value;
        return true;
    }
}