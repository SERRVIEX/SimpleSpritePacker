using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DescriptionAttribute : PropertyAttribute
{
    public string Value { get; private set; }
    public Color Color { get; private set; }

    // Constructors

    /// <summary>
    /// Constructor for DescriptionAttribute that takes a string value.
    /// </summary>
    public DescriptionAttribute(string value)
    {
        Value = value;
        Color = Color.white;
    }

    /// <summary>
    /// Constructor for DescriptionAttribute that takes a string value 
    /// and RGB color values.
    /// </summary>
    public DescriptionAttribute(string value, byte r, byte g, byte b)
    {
        Value = value;
        Color = new Color32(r, g, b, 255);
    }

    /// <summary>
    /// Initializes a new instance of the DescriptionAttribute class with 
    /// a string value and a ColorType enum value.
    /// </summary>
    public DescriptionAttribute(string value, ColorType preset)
    {
        Value = value;

        switch (preset)
        {
            case ColorType.White:
                Color = Color.white;
                break;
            case ColorType.Red:
                Color = new Color32(200, 50, 50, 255);
                break;
            case ColorType.Orange:
                Color = new Color32(200, 100, 50, 255);
                break;
            case ColorType.Yellow:
                Color = new Color32(200, 200, 50, 255);
                break;
            case ColorType.Green:
                Color = new Color32(50, 200, 50, 255);
                break;
            case ColorType.Cyan:
                Color = new Color32(50, 200, 200, 255);
                break;
            case ColorType.Blue:
                Color = new Color32(50, 50, 200, 255);
                break;
            case ColorType.Purple:
                Color = new Color32(200, 50, 200, 255);
                break;
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DescriptionAttribute))]
public class DescriptionAttributeDrawer : PropertyDrawer
{
    private GUIStyle _helpBoxStyle;

    // Methods

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        // Get the description attribute for the property.
        DescriptionAttribute attribute = this.attribute as DescriptionAttribute;

        // Create a temporary rect to draw the description text.
        var temp = rect;
        temp.height -= EditorGUIUtility.singleLineHeight + 2;

        // Draw the description text using the help box style.
        GUI.color = attribute.Color;
        EditorGUI.LabelField(temp, attribute.Value, _helpBoxStyle);
        GUI.color = Color.white;

        // Move the rect down by a single line and a small offset to draw the property below the description text.
        rect.y += temp.height + 2;

        // Draw the property and its children using the new rect, and provide a custom label with the property name.
        EditorGUI.PropertyField(rect, property, new GUIContent(property.displayName), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // If the help box style hasn't been created yet, create it now.
        if (_helpBoxStyle == null)
        {
            _helpBoxStyle = new GUIStyle(EditorStyles.helpBox);
            _helpBoxStyle.richText = true;
        }

        // Get the description text for the property.
        string description = (attribute as DescriptionAttribute).Value;

        // Calculate the height of the description text using the help box style.
        float height = _helpBoxStyle.CalcHeight(new GUIContent(description), EditorGUIUtility.currentViewWidth);

        // Add the height of a single line and a small offset to account for the space below the description text.
        height += EditorGUIUtility.singleLineHeight + 4;

        // Return the calculated height.
        return height;
    }
}
#endif