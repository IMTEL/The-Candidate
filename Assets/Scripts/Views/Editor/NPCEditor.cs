using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(NPC))]
public class NPCEditor : Editor
{

    private SerializedProperty TTService;


    private SerializedProperty LLMService;

    
    void OnEnable()
    {
        if(serializedObject == null) throw new ArgumentException("Object not serializable");
        TTService = serializedObject.FindProperty("TTService");
        LLMService = serializedObject.FindProperty("LLMService");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
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
        }
        if(LLMService.objectReferenceValue != null)
        {
            var options = ((NPC)target).GetLLMOptions();
            if (options != null)
                FleshOptiontypes(options);
            else
                EditorGUILayout.HelpBox("No LLM options available (null list)", MessageType.Warning);
        }

        EditorGUILayout.PropertyField(TTService, new GUIContent("TTService"));
        if(TTService.objectReferenceValue != null)
        {
            TTSserviceProvider TTSObject = (TTSserviceProvider)TTService.objectReferenceValue;
        }
        EditorGUI.BeginChangeCheck();

        serializedObject.ApplyModifiedProperties();
    }


    public void FleshOptiontypes(IEnumerable<OptionBase> Options)
    {
        if (Options == null) return;
        foreach(OptionBase option in Options)
            {
                if(option.GetType() == typeof(FloatBoundOption))
                {
                    EditorGUI.BeginChangeCheck();
                    FloatBoundOption floatBoundOption = (FloatBoundOption)option;
                    EditorGUILayout.LabelField(floatBoundOption.GetName());
                    float newValue = EditorGUILayout.Slider(floatBoundOption.GetValue(), floatBoundOption.GetBounds().Item1,floatBoundOption.GetBounds().Item2);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Changed " + floatBoundOption.GetName());
                        floatBoundOption.SetValue(newValue);
                        EditorUtility.SetDirty(target); 
                    }
                } else if(option.GetType() == typeof(PickOption<OpenAICompletionRequest.Model>))
                {
                    PickOption<OpenAICompletionRequest.Model> pickOption = (PickOption<OpenAICompletionRequest.Model>)option;
                    EditorGUILayout.LabelField(pickOption.GetName());
                }
            }
    }
}

