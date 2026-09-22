using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

[CustomPropertyDrawer(typeof(Agent))]
public class AgentDrawer : PropertyDrawer
{
    Agent agent;
    SerializedProperty serialized;
    ScrollView LLMOptionsView;
    ScrollView TTSOptionsView;
    

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        SerializedProperty nameProperty = property.FindPropertyRelative("name");
        serialized = property;
        agent = serialized.boxedValue as Agent;
        LLMOptionsView = new();
        TTSOptionsView = new();
        if (agent == null)
        {
            agent = new();
            serialized.boxedValue = agent;
            serialized.serializedObject.ApplyModifiedProperties();
            WriteBack();
        }

        TabView tabs = new();
        PopupWindow popupWindow = new()
        {
            text = $"{property.name} settings"
        };

        TextField name = new()
        {
            value = nameProperty.stringValue,
            label = "Name",
        };
        _ = name.RegisterValueChangedCallback((value) =>
        {
            RecordChange();
            nameProperty.stringValue = value.newValue;
            serialized.serializedObject.ApplyModifiedProperties();
            WriteBack();
        });
        popupWindow.Add(name);
        popupWindow.Add(tabs);
        tabs.Add(CreateLLMSettings());
        tabs.Add(CreateTTSettings());
        return popupWindow;
    }

    private void OptionSetLLM(OptionSet optionSet)
    {
        RecordChange();
        agent.SetOptionsLLM(optionSet);
        WriteBack();
    }

    private void OptionSetTTS(OptionSet optionSet)
    {
        RecordChange();
        agent.SetOptionsTTS(optionSet);
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
            element.AddListener(OptionSetLLM);
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
            element.AddListener(OptionSetTTS);
        }
        tab.Add(TTSOptionsView);
        return tab;
    }

    void OnLLMFieldChange(ChangeEvent<Object> evt)
    {
        RecordChange();
        LLMOptionsView.Clear();
        if (evt.newValue is LLMServiceProvider LLMProvider)
        {
            agent.SetLLMServiceProvider(LLMProvider);
            serialized.FindPropertyRelative("LLMServiceProvider").objectReferenceValue = LLMProvider;
            if (agent.GetLLMSettings(out _, out OptionSet settings, false))
            {
                OptionsetElement element = new(settings, LLMProvider.GetLLMOptions());
                LLMOptionsView.Add(element);
                element.AddListener(OptionSetLLM);
            }
        }
        else
        {
            agent.SetLLMServiceProvider(null);
            serialized.FindPropertyRelative("LLMServiceProvider").objectReferenceValue = null;
        }
        serialized.serializedObject.ApplyModifiedProperties();
        WriteBack();
    }


    void OnTTSFieldValueChanges(ChangeEvent<Object> evt)
    {
        RecordChange();
        TTSOptionsView.Clear();
        if (evt.newValue is TTSServiceProvider TTSProvider)
        {
            agent.SetTTSServiceProvider(TTSProvider);
            serialized.FindPropertyRelative("TTServiceProvider").objectReferenceValue = TTSProvider;
            if (agent.GetTTSSettings(out _, out OptionSet settings, false))
            {
                OptionsetElement element = new(settings, TTSProvider.GetTTSOptions());
                TTSOptionsView.Add(element);
                element.AddListener(OptionSetTTS);
            }
        }
        else
        {
            agent.SetTTSServiceProvider(null);
            serialized.FindPropertyRelative("TTServiceProvider").objectReferenceValue = null;
        }
        serialized.serializedObject.ApplyModifiedProperties();
        WriteBack();
    }

    void RecordChange()
    {
        Object target = serialized.serializedObject.targetObject;
        Undo.RecordObject(target, "Change Agent settings");
    }

    void WriteBack()
    {
        Object target = serialized.serializedObject.targetObject;
        EditorUtility.SetDirty(target);
        if (PrefabUtility.IsPartOfPrefabInstance(target))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
        else if (PrefabUtility.IsPartOfPrefabAsset(target) && target is Component component)
        {
            GameObject prefabRoot = component.transform.root.gameObject;
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }
        else
        {
            AssetDatabase.SaveAssetIfDirty(target);
        }
    }


}