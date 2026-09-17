using System;
using UnityEngine;

[Serializable]
public class TextOption : OptionBase
{
    [SerializeField]
    private string value;

    public TextOption(string name, string value = "") : base(name)
    {
        this.value = value;
    }
 
    public override OptionBase Clone()
    {
        return new TextOption(GetName(), value);
    }

    public override object GetObjectValue()
    {
        return value;
    }

    public override bool TrySetValue(object value)
    {
        this.value = value as string;
        return true;
    }
}