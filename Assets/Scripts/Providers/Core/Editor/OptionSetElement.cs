using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class OptionsetElement : PopupWindow
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
        if (current != null && !original.GetType().Equals(current.GetType()))
        {
            throw new ArgumentException("These are not the same type of Options");
        }
        if (original is FloatBoundOption floatBoundOption)
        {
            Slider field = new(original.GetName())
            {
                value = current != null ? ((FloatBoundOption)current).GetValue() : floatBoundOption.GetValue(),
                lowValue = floatBoundOption.GetBounds().Item1,
                showInputField = true,
                highValue = floatBoundOption.GetBounds().Item2,
                direction = SliderDirection.Horizontal,

            };
            field.RegisterValueChangedCallback(evt =>
            {
                FloatBoundOption floatBound = (FloatBoundOption)GetOptionChange(floatBoundOption);
                if (floatBound.TrySetValue(evt.newValue)) { notify?.Invoke(options); }
                else { field.value = floatBound.GetValue(); }
                ;
            });
            Add(field);
        }
        else if (original is PickOption pickOption)
        {
            DropdownField dropfield = new(pickOption.GetName())
            {
                choices = new(pickOption.GetOptionDisplayNames()),
                //Fetch the index of the string that matched the display name of the current (if it is not null) or the default option
                index = new List<string>(pickOption.GetOptionDisplayNames()).IndexOf(GetCurrentDisplayName(pickOption, (PickOption)current)),
            };
            dropfield.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                int index = new List<string>(pickOption.GetOptionDisplayNames()).IndexOf(evt.newValue);
                PickOption option = (PickOption)GetOptionChange(pickOption);
                option.TrySetValue(option.GetOptionsObjects().ElementAt(index));
                notify?.Invoke(options);
            });
            Add(dropfield);
        }
        else if (original is TextOption textOption)
        {
            TextOption currentTextOption = current as TextOption;
            TextField textField = new(textOption.GetName())
            {
                value = (currentTextOption ?? textOption).GetObjectValue() as string,
                multiline = true
            };
            textField.style.whiteSpace = WhiteSpace.PreWrap;
            textField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                GetOptionChange(textOption).TrySetValue(evt.newValue);
                notify?.Invoke(options);
            });
            Add(textField);
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

    public void AddListener(Action<OptionSet> listener)
    {
        notify += listener;
    }

    public void RemoveListener(Action<OptionSet> listener)
    {
        notify -= listener;
    }


    private static string GetCurrentDisplayName(
        PickOption original,
        PickOption current)
    {
        if (current != null &&
            current.GetOptionsObjects().Any() &&
            current.GetOptionDisplayNames().Any())
        {
            return current.GetCurrentname();
        }

        return original.GetCurrentname();
    }
}