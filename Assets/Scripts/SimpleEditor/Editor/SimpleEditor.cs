namespace UnityEditor
{
    using UnityEngine;

    public static class SimpleEditor
    {
        private static GUIStyle _headerTitleStyle;
        private static GUIStyle _horizontalLineStyle;

        // Methods

        /// <summary>
        /// Call on begin of the OnGUI() method.
        /// </summary>
        public static void Initialize()
        {
            {
                _headerTitleStyle = new GUIStyle(GUI.skin.box);
                _headerTitleStyle.normal.textColor = Color.white;
                _headerTitleStyle.normal.background = CreateTexture(2, 2, new Color32(62, 62, 62, 255));
            }

            {
                _horizontalLineStyle = new GUIStyle();
                _horizontalLineStyle.normal.background = EditorGUIUtility.whiteTexture;
                _horizontalLineStyle.margin = new RectOffset(0, 0, 4, 4);
                _horizontalLineStyle.fixedHeight = 1;
            }
        }

        public static Texture2D CreateTexture(int width, int height, Color color)
        {
            Color32[] pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = color;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels32(pixels);
            result.Apply();

            return result;
        }

        public static void Header(string title)
        {
            EditorGUILayout.Space(5);

            GUI.color = Color.white;

            {
                Rect rect = EditorGUILayout.GetControlRect(false, -1);
                rect.position = new Vector2(0, rect.position.y);
                rect.width = EditorGUIUtility.currentViewWidth;
                rect.height = 1;

                EditorGUI.DrawRect(rect, new Color32(26, 26, 26, 255));
            }

            {
                Rect rect = EditorGUILayout.GetControlRect(false, 20);
                rect.position = new Vector2(0, rect.position.y);
                GUIStyle titleStyle = new GUIStyle(_headerTitleStyle);
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fixedWidth = EditorGUIUtility.currentViewWidth;
                titleStyle.stretchWidth = true;
                EditorGUI.LabelField(rect, title, titleStyle);
            }

            {
                Rect rect = EditorGUILayout.GetControlRect(false, 1);
                rect.position = new Vector2(0, rect.position.y - 2);
                rect.width = EditorGUIUtility.currentViewWidth;
                rect.height = 1;

                EditorGUI.DrawRect(rect, new Color32(48, 48, 48, 255));
            }

            GUI.color = Color.white;
        }

        public static void HorizontalLine()
        {
            GUILayout.Space(5);

            GUI.color = new Color32(48, 48, 48, 255);
            GUILayout.Box(GUIContent.none, _horizontalLineStyle);
            GUI.color = Color.white;

            GUILayout.Space(5);
        }
    }
}