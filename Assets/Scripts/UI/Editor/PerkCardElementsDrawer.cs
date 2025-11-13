using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PerkCardElements))]
public class PerkCardElementsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Try to read the displayName field from the serialized property
        var displayNameProp = property.FindPropertyRelative("displayName");
        var displayName = displayNameProp != null && !string.IsNullOrEmpty(displayNameProp.stringValue)
            ? displayNameProp.stringValue
            : label.text;

        EditorGUI.BeginProperty(position, new GUIContent(displayName), property);
        // Draw the property using the custom label; true = include children
        EditorGUI.PropertyField(position, property, new GUIContent(displayName), true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Let Unity compute the full height including children
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
