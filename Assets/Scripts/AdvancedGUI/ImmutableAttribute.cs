using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImmutableAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ImmutableAttribute))]
public class ImmutableAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(rect, property, label, true);
        EditorGUI.EndDisabledGroup();
    }
}
#endif