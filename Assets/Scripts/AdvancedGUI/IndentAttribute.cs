using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IndentAttribute : PropertyAttribute
{
    public int Value { get; private set; }

    // Constructors

    public IndentAttribute() => Value = 1;

    public IndentAttribute(int indent) => Value = indent;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(IndentAttribute))]
public class IndentAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        // Get the indent attribute for the property.
        IndentAttribute attribute = this.attribute as IndentAttribute;

        // Draw the property and its children using the new rect, and provide a custom label with the property name.
        int temp = EditorGUI.indentLevel;
        EditorGUI.indentLevel = attribute.Value;
        EditorGUI.PropertyField(rect, property, label, true);
        EditorGUI.indentLevel = temp;
    }
}
#endif