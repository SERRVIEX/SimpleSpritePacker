using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ColorAttribute : PropertyAttribute
{
    public Color Value { get; private set; }

    // Constructors

    /// <summary>
    /// Constructor for DescriptionAttribute that takes a RGB color values.
    /// </summary>
    public ColorAttribute(byte r, byte g, byte b) => Value = new Color32(r, g, b, 255);
    

    /// <summary>
    /// Initializes a new instance of the DescriptionAttribute 
    /// class with a ColorType enum value.
    /// </summary>
    public ColorAttribute(ColorType preset)
    {
        switch (preset)
        {
            case ColorType.White:
                Value = Color.white;
                break;
            case ColorType.Red:
                Value = new Color32(200, 50, 50, 255);
                break;
            case ColorType.Orange:
                Value = new Color32(200, 100, 50, 255);
                break;
            case ColorType.Yellow:
                Value = new Color32(200, 200, 50, 255);
                break;
            case ColorType.Green:
                Value = new Color32(50, 200, 50, 255);
                break;
            case ColorType.Cyan:
                Value = new Color32(50, 200, 200, 255);
                break;
            case ColorType.Blue:
                Value = new Color32(50, 50, 200, 255);
                break;
            case ColorType.Purple:
                Value = new Color32(200, 50, 200, 255);
                break;
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ColorAttribute))]
public class ColorAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        // Get the color attribute for the property.
        ColorAttribute attribute = this.attribute as ColorAttribute;

        // Draw the property and its children using the new rect, and provide a custom label with the property name.
        GUI.color = attribute.Value;
        EditorGUI.PropertyField(rect, property, label, true);
        GUI.color = Color.white;
    }
}
#endif