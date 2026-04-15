using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPC))]
public class NPCEditor : Editor
{
    private SerializedProperty TTService;

    private SerializedProperty LLMService;
    
    new private SerializedProperty name;
    
    void OnEnable()
    {
        if(serializedObject == null) throw new ArgumentException("Object not serializable");
        TTService = serializedObject.FindProperty("TTService");
        LLMService = serializedObject.FindProperty("LLMService");
        name = serializedObject.FindProperty("name");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(name, new GUIContent("Name"));
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(LLMService, new GUIContent("LLMService"));
        if (EditorGUI.EndChangeCheck())
        {
            if(LLMService.objectReferenceValue != null)
            {
                ((NPC)target).SetLLMOptions(((LLMServiceProvider) LLMService.objectReferenceValue).GetLLMOptions());
            }
            else
            {
                ((NPC)target).SetLLMOptions(new List<OptionBase>());
            }
            EditorUtility.SetDirty(target);
            Repaint(); 
        }
        if(LLMService.objectReferenceValue != null)
        {
            var options = ((NPC)target).GetLLMOptions();
            if (options != null)
                FleshOptiontypes(options, ((NPC)target).SetLLMOverrideValue);
            else
                EditorGUILayout.HelpBox("No LLM options available (null list)", MessageType.Warning);
        }
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(TTService, new GUIContent("TTService"));
        if (EditorGUI.EndChangeCheck())
        {
            if(TTService.objectReferenceValue != null)
            {
                ((NPC)target).SetTTSOptions(((TTSserviceProvider) TTService.objectReferenceValue).GetTTSOptions());
            }
            else
            {
                ((NPC)target).SetTTSOptions(new List<OptionBase>());
            }
            EditorUtility.SetDirty(target); 
            Repaint();
        }
        if(TTService.objectReferenceValue != null)
        {
            var options = ((NPC)target).GetTTSOptions();
            if (options != null)
                FleshOptiontypes(options,((NPC)target).SetTTSOverrideValue);
            else
                EditorGUILayout.HelpBox("No TTS options available (null list)", MessageType.Warning);
        }
        EditorGUI.BeginChangeCheck();

        serializedObject.ApplyModifiedProperties();
    }


    public void FleshOptiontypes(IEnumerable<OptionBase> Options, Action<OptionBase> overridemethod)
    {
        if (Options == null) return;
        foreach(OptionBase option in Options)
            {
                EditorGUILayout.LabelField(option.GetName());
                if(option is FloatBoundOption floatBoundOption)
                {
                    EditorGUI.BeginChangeCheck();
                    float newValue = EditorGUILayout.Slider(floatBoundOption.GetValue(), floatBoundOption.GetBounds().Item1,floatBoundOption.GetBounds().Item2);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Changed " + floatBoundOption.GetName());
                        floatBoundOption.SetValue(newValue);
                        overridemethod.Invoke(floatBoundOption);
                        EditorUtility.SetDirty(target); 
                    }
                } else if(option is PickOption pickOption){

                    if (EditorGUILayout.DropdownButton(new GUIContent(pickOption.GetCurrentname()), FocusType.Passive))
                    {
                        //A bit of AI help on this bit. Mostly becouse I did not know how to create a own window/menu for this
                        GenericMenu menu = new();
                        List<string> displayNames = new(pickOption.GetOptionDisplayNames());
                        List<object> objects = new(pickOption.GetOptionsObjects());
                        for (int i = 0; i < displayNames.Count(); i++)
                        {
                            int index = i;
                            bool on = displayNames[i].Equals(pickOption.GetCurrentname());
                            menu.AddItem(new GUIContent(displayNames[i]), on, () =>
                            {
                                Undo.RecordObject(target, "Changed " + pickOption.GetCurrentname());
                                pickOption.TrySetValue(objects[index]);
                                overridemethod.Invoke(pickOption);
                                EditorUtility.SetDirty(target);
                            });
                        }
                        menu.ShowAsContext();
                    }
            }
            }
    }
}

