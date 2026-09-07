using System.Collections.Generic;
using UnityEngine;

public class OptionSet
{

    [SerializeReference]
    private readonly HashSet<OptionBase> optionBases = new();

    public OptionSet(){}

    public OptionSet(IEnumerable<OptionBase> options)
    {
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

    public OptionSet MergeOnTop(OptionSet optionSet)
    {
        OptionSet merged = new(this);
        foreach (OptionBase item in optionSet.optionBases)
        {
            if (merged.optionBases.Contains(item))
            {
                merged.optionBases.TryGetValue(item, out OptionBase actualValue);
                actualValue.TrySetValue(item.GetObjectValue());
            } else
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
}