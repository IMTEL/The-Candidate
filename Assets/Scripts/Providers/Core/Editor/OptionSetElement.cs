using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class OptionsetElement : PopupWindow, IEvent<IOptionChange>
{
    Action<OptionSet> notify;
    OptionSet options;

    public OptionsetElement(OptionSet options, OptionSet defaults = null) : base()
    {
        this.options = options ?? throw new ArgumentException("This option set is invalid");
        OptionSet optionSet = defaults ?? options;
        foreach (OptionBase item in optionSet.GetOptions())
        {
            options.ContainsOption(item, out OptionBase configOption);
            GenerateOption(item, configOption);
        }
    }
    private void GenerateOption(OptionBase original, OptionBase current = null)
    {
        if(current != null && !original.GetType().Equals(current.GetType()))
        {
            throw new ArgumentException("These are not the same type of Options");
        }
        if (original is FloatBoundOption floatBoundOption)
        {
            Slider field = new(original.GetName())
            {
                value = current != null ? ((FloatBoundOption)current).GetValue() : floatBoundOption.GetValue(),
                lowValue = floatBoundOption.GetBounds().Item1,
                highValue = floatBoundOption.GetBounds().Item2,
                direction = SliderDirection.Horizontal,
                
            };
            field.RegisterValueChangedCallback(evt =>
            {
                FloatBoundOption floatBound = (FloatBoundOption)GetOptionChange(floatBoundOption);
                floatBound.TrySetValue(evt.newValue);
                notify?.Invoke(options);
            });
            Add(field);
        }
        else if (original is PickOption pickOption)
        {
            DropdownField dropfield = new(pickOption.GetName())
            {
                choices = new(pickOption.GetOptionDisplayNames()),
                //Fetch the index of the string that matched the display name of the current (if it is not null) or the default option
                index = new List<string>(pickOption.GetOptionDisplayNames()).IndexOf(pickOption.GetOptionDisplayNames().First(value => value.Equals(current != null ?((PickOption)current).GetCurrentname() : pickOption.GetCurrentname()))),
            };
            dropfield.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                int index = new List<string>(pickOption.GetOptionDisplayNames()).IndexOf(evt.newValue);
                PickOption option = (PickOption)GetOptionChange(pickOption);
                _ = option.TrySetValue(option.GetOptionsObjects().ElementAt(index));
                notify?.Invoke(options);
            });
            Add(dropfield);
        }
        // Reset button
        //ResetButton.style.display = !original.GetObjectValue().Equals(overrideOption.GetObjectValue()) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private OptionBase GetOptionChange(OptionBase opt)
    {
        if (!options.ContainsOption(opt, out OptionBase first))
        {
            first = opt.Clone();
            options.AddOption(first);
        }
        return first;
    }

    public void AddListener(IOptionChange listener)
    {
        notify += listener.OptionBaseChange;
    }

    public void RemoveListener(IOptionChange listener)
    {
        notify -= listener.OptionBaseChange;
    }

}