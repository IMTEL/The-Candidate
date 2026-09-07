using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class OptionProperty : BindableElement, INotifyValueChanged<OptionBase>
{
    public OptionBase value
    {
        get => value;
        set
        {
            value.TrySetValue(value);
        }
    }

    public OptionProperty()
    {
        if (value is FloatBoundOption floatBoundOption)
        {
            Slider field = new()
            {
                value = floatBoundOption.GetValue(),
                lowValue = floatBoundOption.GetBounds().Item1,
                highValue = floatBoundOption.GetBounds().Item2,
                direction = SliderDirection.Horizontal
            };
            field.RegisterValueChangedCallback(evt => value.TrySetValue(evt.newValue));
            Add(field);
        }
        else if (value is PickOption pickOption)
        {
            DropdownField dropfield = new(value.GetName())
            {
                choices = new(pickOption.GetOptionDisplayNames()),
                index = new List<string>(pickOption.GetOptionDisplayNames()).IndexOf(pickOption.GetOptionDisplayNames().First(value => value.Equals(pickOption.GetCurrentname()))),
            };
            dropfield.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                int index = new List<string>(pickOption.GetOptionDisplayNames()).IndexOf(evt.newValue);
                value.TrySetValue(new List<object>(pickOption.GetOptionsObjects()).ElementAt(index));
            });
            Add(dropfield);
        }
    }

    public void SetValueWithoutNotify(OptionBase newValue)
    {
        value.TrySetValue(newValue);
    }
}
