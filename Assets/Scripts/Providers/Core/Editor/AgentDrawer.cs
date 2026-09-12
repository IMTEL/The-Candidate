using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

[CustomPropertyDrawer(typeof(Agent))]
public class AgentDrawer : PropertyDrawer, IOptionChange
{
    Agent agent;
    SerializedProperty serialized;
    ScrollView LLMOptionsView;
    ScrollView TTSOptionsView;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        agent = (Agent)fieldInfo.GetValue(property.serializedObject.targetObject);
        serialized = property;
        LLMOptionsView = new();
        TTSOptionsView = new();
        if (agent == null)
        {
            agent = new();
            WriteBack();
        }

        TabView tabs = new();
        PopupWindow popupWindow = new()
        {
            text = $"{property.name} settings"
        };
        popupWindow.Add(tabs);
        tabs.Add(CreateLLMSettings());
        tabs.Add(CreateTTSettings());
        return popupWindow;
    }

    public void OptionBaseChange(OptionSet optionSet)
    {
        agent.SetOptionsLLM(optionSet);
        WriteBack();
    }

    private Tab CreateLLMSettings()
    {
        Tab tab = new("LLMSettings");
        ObjectField LLMService = new();
        tab.Add(LLMService);
        LLMService.objectType = typeof(LLMServiceProvider);
        LLMService.RegisterValueChangedCallback(OnLLMFieldChange);
        if (agent.GetLLMSettings(out LLMServiceProvider LLMProvider, out OptionSet LLMoptions, false))
        {
            LLMService.value = LLMProvider;
            OptionsetElement element = new(LLMoptions, LLMProvider.GetLLMOptions());
            LLMOptionsView.Add(element);
            element.AddListener(this);
        }
        tab.Add(LLMOptionsView);
        return tab;
    }

    private Tab CreateTTSettings()
    {
        Tab tab = new("TTSettings");
        ObjectField TTService = new();
        tab.Add(TTService);
        TTService.objectType = typeof(TTSServiceProvider);
        TTService.RegisterValueChangedCallback(OnTTSFieldValueChanges);
        if (agent.GetTTSSettings(out TTSServiceProvider TTSProvider, out OptionSet TTSOptions, false))
        {
            TTService.value = TTSProvider;
            OptionsetElement element = new(TTSOptions, TTSProvider.GetTTSOptions());
            TTSOptionsView.Add(element);
            element.AddListener(this);
        }
        tab.Add(TTSOptionsView);
        return tab;
    }

    void OnLLMFieldChange(ChangeEvent<Object> evt)
    {
        LLMOptionsView.Clear();
        if (evt.newValue is LLMServiceProvider LLMProvider)
        {
            agent.SetLLMServiceProvider(LLMProvider);
            if (agent.GetLLMSettings(out _, out OptionSet settings, false))
            {
                OptionsetElement element = new(settings, LLMProvider.GetLLMOptions());
                LLMOptionsView.Add(element);
                element.AddListener(this);
            }
        }
        else
        { agent.SetLLMServiceProvider(null); }
        WriteBack();
    }


    void OnTTSFieldValueChanges(ChangeEvent<Object> evt)
    {
        TTSOptionsView.Clear();
        if (evt.newValue is TTSServiceProvider TTSProvider)
        {
            agent.SetTTSServiceProvider(TTSProvider);
            if (agent.GetTTSSettings(out _, out OptionSet settings, false))
            {
                OptionsetElement element = new(settings, TTSProvider.GetTTSOptions());
                TTSOptionsView.Add(element);
                element.AddListener(this);
            }


        }
        else { agent.SetTTSServiceProvider(null); }
        WriteBack();
    }

    void WriteBack()
    {
        EditorUtility.SetDirty(serialized.serializedObject.targetObject);
    }


}