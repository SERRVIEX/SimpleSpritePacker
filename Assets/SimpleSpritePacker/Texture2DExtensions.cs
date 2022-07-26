namespace SimpleSpritePacker.Extensions
{
    using UnityEngine;

    public static class Texture2DExtensions
    {
        public static void SetPixels(this Texture2D texture, Color color)
        {
            Color[] array = new Color[texture.width * texture.height];
            for (int i = 0; i < array.Length; i++)
                array[i] = color;

            texture.SetPixels(array);
        }

        public static void SetPixels(this Texture2D texture, int x, int y, int width, int height, Color color)
        {
            Color[] array = new Color[texture.width * texture.height];
            for (int i = 0; i < array.Length; i++)
                array[i] = color;

            texture.SetPixels(x, y, width, height, array);
        }

        public static void SetPixels32(this Texture2D texture, Color32 color)
        {
            Color32[] array = new Color32[texture.width * texture.height];
            for (int i = 0; i < array.Length; i++)
                array[i] = color;

            texture.SetPixels32(array);
        }
    }
}