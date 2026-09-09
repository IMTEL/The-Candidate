using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class PickOption : OptionBase{
    protected PickOption(string name) : base(name)
    {}

    public abstract string  GetCurrentname();
    public abstract IEnumerable<object> GetOptionsObjects(); // all option values as objects
    public abstract IEnumerable<string> GetOptionDisplayNames(); // display labels for each option
}

[Serializable]
public class PickOption<T> : PickOption
{
    [SerializeField] private OptionEntry<T> value;
    [SerializeField] private List<OptionEntry<T>> options;

    public PickOption(string name,  IEnumerable<OptionEntry<T>> values, OptionEntry<T> defualtValue = null) : base(name) {
        if(values == null || values.Count() == 0)
        {
            throw new ArgumentException("The values has to be a list of options with 1 or more options");    
        }
        options = new(values);
        defualtValue ??= values.First();
        SetValue(defualtValue);
    }

    public PickOption(string name,  IEnumerable<(T, string)> values, object defualtValue = null) : base(name) {
        if(values == null || values.Count() == 0)
        {
            throw new ArgumentException("The values has to be a list of options with 1 or more options");    
        }
        options = new();
        foreach ((T, string) item in values)
        {
            OptionEntry<T> optionEntry = new(item.Item1, item.Item2);
            options.Add(optionEntry);
        }
        defualtValue ??= values.First();
        TrySetValue(defualtValue);
    }

    public OptionEntry<T> GetOption()
    {
        return value;
    }

    public T GetCurrentValue(){
        return value.GetValue();
    }

    public override string GetCurrentname(){
        return value.GetDisplayName();
    }

    public IEnumerable<OptionEntry<T>> GetOptions()
    {
        return options;
    }

    public bool SetValue(OptionEntry<T> value)
    {
        if(value == null || !options.Contains(value))
        {
            throw new ArgumentException("Value has to be within the list of options and not null");
        }
        this.value = value;
        return true;
    }

    public override object GetObjectValue()
    {
        return value;
    }

    public T GetValue()
    {
        return value.GetValue();
    }

    public override IEnumerable<object> GetOptionsObjects()
    {
        return options.Select(value => (object)value.GetValue());
    }

    public override IEnumerable<string> GetOptionDisplayNames()
    {
        return options.Select(value => value.GetDisplayName());
    }

    public override bool TrySetValue(object value)
    {
        if(value is OptionEntry<T> optionEntry){
            return SetValue(optionEntry);
        } else if (value is T typevalue){
            OptionEntry<T> foundOption = options.Find(option => option.GetValue().Equals(typevalue));
            if (foundOption != null) return SetValue(foundOption);
        }
        return false;
    }

    public bool Contains(OptionEntry<T> value)
    {
        return options.Contains(value);
    }

    public override bool Equals(object obj)
    {
        if (!base.Equals(obj))
        {
            return false;
        }
        // TODO: write your implementation of Equals() here
        PickOption<T> casted = (PickOption<T>)obj;
        return options.All(option => casted.GetOptions().Contains(option)) && GetHashCode().Equals(casted.GetHashCode())
            && base.Equals(casted);
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        int hash = 17;
        foreach (var option in options)
        {
            hash = hash * 23 + option.GetHashCode();
        }
        return hash+base.GetHashCode();
    }

    public bool SetValueObject(object value)
    {
        return TrySetValue(value);
    }

    public override OptionBase Clone()
    {
        return new PickOption<T>(GetName(), options, value);
    }
}

[Serializable]
public class OptionEntry<T> {
    [SerializeReference] private T value;
    [SerializeField] private string displayName;

    public OptionEntry(T value, string name)
    {        
        this.value = value;
        this.displayName = name; 
    }

    public T GetValue(){
        return value;
    }

    public string GetDisplayName(){
        return displayName;
    }

    public override bool Equals(object obj)
    {   
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        OptionEntry<T> casted = (OptionEntry<T>)obj;
        if(value.Equals(casted.GetValue()) && displayName.Equals(casted.GetDisplayName())) return true;
        return false;
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return HashCode.Combine(value, displayName);
    }
}

