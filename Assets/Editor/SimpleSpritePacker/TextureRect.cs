namespace SimpleSpritePacker
{
    using UnityEngine;

    public class TextureRect
    {
        public Texture2D Texture;

        public int X;
        public int Y;
        public int Width;
        public int Height;

        // Constructors

        public TextureRect(Texture2D texture, SpaceRect space)
        {
            Texture = texture;

            X = space.X;
            Y = space.Y;
            Width = texture.width;
            Height = texture.height;
        }
    }
}