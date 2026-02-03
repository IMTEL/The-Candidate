#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(AssetDB))]
public class BodySkinDBEditor : Editor
{
    private SerializedProperty assetDB;
    private ReorderableList list;

    private void OnEnable()
    {
        assetDB =  serializedObject.FindProperty("entries");
        list = new ReorderableList(
            serializedObject,
            assetDB,
            false, true, true, true
        );
        if(serializedObject == null) Debug.Log("It is not serilized");
        if(assetDB == null) Debug.Log("AssetDB property not found");
        list.drawHeaderCallback = rect =>
        {
            float w = rect.width / 3f;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, w, rect.height), "ID");
            EditorGUI.LabelField(new Rect(rect.x + w, rect.y, w, rect.height), "Texture");
            EditorGUI.LabelField(new Rect(rect.x + 2*w, rect.y, w, rect.height), "Index");
        };

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            float w = rect.width / 3f;

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, w - 5, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("referenceName"),
                GUIContent.none
            );

            EditorGUI.PropertyField(
                new Rect(rect.x + w, rect.y, w - 5, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("material"),
                GUIContent.none
            );

            EditorGUI.PropertyField(
                new Rect(rect.x + 2*w, rect.y, w - 5, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("index"),
                GUIContent.none
            );
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif