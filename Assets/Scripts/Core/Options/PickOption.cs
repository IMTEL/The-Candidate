using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public interface IPickOption{
    string GetCurrentname();
    object GetValueObject();                    // current value as object
    IEnumerable<object> GetOptionsObjects(); // all option values as objects
    IEnumerable<string> GetOptionDisplayNames(); // display labels for each option
    bool SetValueObject(object value);         // set by value/entry/index
}

[Serializable]
public class PickOption<T> : OptionBase, IPickOption, IEvent<T>
{
    [SerializeField] private OptionEntry<T> value;
    [SerializeField] private List<OptionEntry<T>> options;

    public PickOption(string name,  IEnumerable<OptionEntry<T>> values, OptionEntry<T> defualtValue = null) : base(name) {
        if(values == null || values.Count() == 0)
        {
            throw new ArgumentException("The values has to be a list of options with 1 or more options");    
        }
        options = new List<OptionEntry<T>>(values);
        defualtValue ??= values.First();
        SetValue(defualtValue);
    }

    public OptionEntry<T> GetOption()
    {
        return value;
    }

    public T GetCurrentValue(){
        return value.GetValue();
    }

    public string GetCurrentname(){
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

    public object GetValueObject()
    {
        return value;
    }

    public IEnumerable<object> GetOptionsObjects()
    {
        return options.Select(value => (object)value.GetValue());
    }

    public IEnumerable<string> GetOptionDisplayNames()
    {
        return options.Select(value => value.GetDisplayName());
    }

    public bool SetValueObject(object value)
    {
        if(value is OptionEntry<T> optionEntry){
            return SetValue(optionEntry);
        } else if (value is T typevalue){
            OptionEntry<T> foundOption = options.Find(option => option.GetValue().Equals(typevalue));
            if (foundOption != null) return SetValue(foundOption);
        }
        return false;
    }

    public void AddListener(IEnumerable<T> listeners)
    {
        throw new NotImplementedException();
    }

    public void AddListener(T listener)
    {
        throw new NotImplementedException();
    }

    public void RemoveListener(IEnumerable<T> listeners)
    {
        throw new NotImplementedException();
    }

    public void RemoveListener(T listener)
    {
        throw new NotImplementedException();
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
}

