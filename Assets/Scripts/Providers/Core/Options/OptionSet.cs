using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A set of options. Used to contain multiple different options and save their state
/// </summary>
[Serializable]
public class OptionSet
{

    [SerializeField]
    private HashSet<OptionBase> optionBases = new();

    public OptionSet(){}

    public enum MergeMode
    {

        /// <summary>
        /// Merges right on left only if left contains them
        /// </summary>
        LeftMerge,
        /// <summary>
        /// Merges right and left to only identical elements
        /// </summary>
        Intersect,
        /// <summary>
        /// Adds all elements from right to left, while also adding the rest of the elements from right.
        /// </summary>
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

    /// <summary>
    /// Gets the options within the set
    /// </summary>
    /// <returns>Options within the set</returns>
    public IEnumerable<OptionBase> GetOptions()
    {
        return optionBases;
    }

    /// <summary>
    /// Adds a option to the set
    /// </summary>
    /// <param name="option">The option to be added</param>
    /// <returns>If it got added</returns>
    public bool AddOption(OptionBase option)
    {
        return optionBases.Add(option);
    }

    /// <summary>
    /// Merges the current OptionSet with another trough a merge strategy
    /// </summary>
    /// <param name="optionSet">A OptionSet to merge this OptionSet with</param>
    /// <param name="merge">A merge strategy. Default is Left</param>
    /// <returns></returns>
    public OptionSet Merge(OptionSet optionSet, MergeMode merge = MergeMode.LeftMerge)
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
    /// <summary>
    /// Removes a option from the set
    /// </summary>
    /// <param name="option">The option to remove</param>
    /// <returns>If it got removed</returns>
    public bool RemoveOption(OptionBase option)
    {
        return optionBases.Remove(option);
    }

    /// <summary>
    /// Checks if set contains the option.
    /// </summary>
    /// <param name="option">The option to check for</param>
    /// <returns>True or false depending if it is in the set</returns>
    public bool ContainsOption<T>(T option) where T : OptionBase
    {
        return ContainsOption(option, out _); 
    }
    
    /// <summary>
    /// Checks if set contains the option.
    /// </summary>
    /// <param name="option">The option to check for</param>
    /// <param name="replicate">The option it matched with</param>
    /// <returns>True or false depending if it is in the set</returns>
    public bool ContainsOption<T>(T option, out T replicate) where T : OptionBase
    {
        replicate = null;
        if(optionBases.TryGetValue(option, out OptionBase test))
        {
            replicate = test as T;
        }
        return replicate != null;
    } 
    
}
