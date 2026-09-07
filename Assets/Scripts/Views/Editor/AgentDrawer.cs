using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Agent))]
public class AgentDrawer : PropertyDrawer
{
    [SerializeField]
    VisualTreeAsset tree;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = tree.Instantiate(property.propertyPath);
        AgentProperty agentProp = root.Q<AgentProperty>();
        Agent agent = (Agent)property.boxedValue;
        agent ??= new Agent();
        property.boxedValue = agent;
        property.serializedObject.ApplyModifiedProperties();
        agentProp.value = agent;
        // ← Add this: write the mutated copy back to the serialized data
        agentProp.RegisterCallback<ChangeEvent<Agent>>(evt =>
        {
            property.boxedValue = evt.newValue;
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        });
        return root;
    }
}