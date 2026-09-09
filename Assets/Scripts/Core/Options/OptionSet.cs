using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OptionSet
{

    [SerializeField]
    private HashSet<OptionBase> optionBases = new();

    public OptionSet(){}

    public enum MergeMode
    {
        LeftMerge,
        Intersect,
        CompleteMerge

    }

    public OptionSet(IEnumerable<OptionBase> options)
    {
        if(options == null)
        {
            throw new ArgumentException("Options can't be null");
        }
        foreach (OptionBase option in options)
        {
            optionBases.Add(option);
        }
    }

    public OptionSet(OptionSet optionSet) : this(optionSet.GetOptions()) {}

    public IEnumerable<OptionBase> GetOptions()
    {
        return optionBases;
    }

    public bool AddOption(OptionBase option)
    {
        return optionBases.Add(option);
    }

    public OptionSet MergeOnTop(OptionSet optionSet, MergeMode merge = MergeMode.LeftMerge)
    {
        OptionSet merged = MergeMode.Intersect != merge ? new(this) : new();
        if(optionSet == null) return merged;
        foreach (OptionBase item in optionSet.optionBases)
        {
            if (merged.optionBases.TryGetValue(item, out OptionBase actualValue) || optionBases.TryGetValue(item, out actualValue))
            {
                actualValue.TrySetValue(item.GetObjectValue());
                if(merge == MergeMode.Intersect)
                {
                    merged.AddOption(actualValue);
                }
            }
            else if (merge == MergeMode.CompleteMerge)
            {
                merged.AddOption(item);
            }
        }
        return merged;
    }

    public bool RemoveOption(OptionBase option)
    {
        return optionBases.Remove(option);
    }

    public bool ContainsOption(OptionBase option)
    {
        return ContainsOption(option, out _); 
    }
    public bool ContainsOption(OptionBase option, out OptionBase replicate)
    {
        return optionBases.TryGetValue(option, out replicate);
    } 
    
}
