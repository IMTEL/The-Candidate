using System;
using Codice.Utils;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

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
        EditorGUILayout.PropertyField(LLMService, new GUIContent("LLMService"));
        if(LLMService.objectReferenceValue != null)
        {
            EditorGUILayout.TextField("A test of what can be said");
        }

        EditorGUILayout.PropertyField(TTService, new GUIContent("TTService"));
        if(TTService.objectReferenceValue != null)
        {
            TTSserviceProvider TTSObject = (TTSserviceProvider)TTService.objectReferenceValue;
        }
        serializedObject.ApplyModifiedProperties();
    }
}

