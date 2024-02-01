using UnityEngine;

using UnityEditor;

public static class AdvancedGUI
{
    public static void Headline(string label)
    {
        EditorGUILayout.Space(5);

        GUI.color = Color.white;

        {
            Rect rect = EditorGUILayout.GetControlRect(false, 24);
            rect.position = new Vector2(5, rect.position.y);

            GUIStyle labelStyle = new GUIStyle();
            if (EditorGUIUtility.isProSkin)
                labelStyle.normal.textColor = new Color32(255, 255, 255, 240);
            else
                labelStyle.normal.textColor = new Color32(25, 25, 25, 240);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 16;
            labelStyle.fixedWidth = EditorGUIUtility.currentViewWidth - 5;
            labelStyle.stretchWidth = true;
            EditorGUI.LabelField(rect, label, labelStyle);
        }

        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.position = new Vector2(0, rect.position.y - 2);
            rect.width = EditorGUIUtility.currentViewWidth;
            rect.height = 1;

            if (EditorGUIUtility.isProSkin)
                EditorGUI.DrawRect(rect, new Color32(50, 50, 50, 255));
            else
                EditorGUI.DrawRect(rect, new Color32(150, 150, 150, 255));
        }

        GUI.color = Color.white;
    }

    public static void HorizontalLine()
    {
        GUILayout.Space(5);

        if (EditorGUIUtility.isProSkin)
            GUI.color = new Color32(50, 50, 50, 255);
        else
            GUI.color = new Color32(150, 150, 150, 255);

        GUIStyle style = new GUIStyle();
        style.normal.background = EditorGUIUtility.whiteTexture;
        style.margin = new RectOffset(0, 0, 4, 4);
        style.fixedHeight = 1;

        GUILayout.Box(GUIContent.none, style);
        GUI.color = Color.white;

        GUILayout.Space(5);
    }

    public static Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}