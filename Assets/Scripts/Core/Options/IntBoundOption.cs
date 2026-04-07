using System;
using UnityEngine;

/// <summary>
/// Class to allow for options within the different provider services with a float value and bounds around it
/// </summary>
[Serializable]
public class FloatBoundOption : OptionBase
{
    [SerializeField] private float value;

    [SerializeField] private float minBound = float.MinValue;
    [SerializeField] private float maxBound = float.MaxValue;


    /// <summary>
    /// Creates a new option for FloatBoundOption
    /// </summary>
    /// <param name="name">The name of the option</param>
    /// <param name="value">The defualt value</param>
    /// <param name="Minvalue">The minimum value the object can contain</param>
    /// <param name="MaxValue">The maximum value the object can contain</param>
    /// <exception cref="ArgumentException">If min and max is the wrong way or if name is null or empty </exception>
    public FloatBoundOption(string name, float value = 0, float Minvalue = float.MinValue, float MaxValue = float.MaxValue) : base(name)
    {
        if(Minvalue > MaxValue)
        {
            throw new ArgumentException("Can't have min be higher then max");
        }
        SetValue(value);
        minBound = Minvalue;
        maxBound = MaxValue;
    }

    public override object GetObjectValue()
    {
        return value;
    }

    public float GetValue()
    {
        return value;
    }

    /// <summary>
    /// Returns the bounds the value has to be in
    /// </summary>
    /// <returns>The float bounds</returns>
    public (float, float) GetBounds()
    {
        return (minBound, maxBound);
    }

    /// <summary>
    /// Sets the value to the value given
    /// </summary>
    /// <param name="value">the value to be sset</param>
    /// <returns>If it could set the value to the value. If not, it will</returns>
    public bool SetValue(float value)
    {
        if (value < minBound)
        {
            this.value = minBound;
        }
        else if(value > maxBound)
        {
            this.value = maxBound;
        }
        else
        {
            this.value = value;
        }
        return value == this.value;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        FloatBoundOption casted = (FloatBoundOption)obj;
        return  casted.maxBound == this.maxBound && 
                casted.minBound == this.minBound && base.Equals(casted);
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (int)maxBound;
        hash = hash * 27  + (int)minBound;
        return base.GetHashCode()+hash;
    }

    public override bool TrySetValue(object value)
    {
        if(value is float v)
        {
           return SetValue(v); 
        }
        return false;
    }
}