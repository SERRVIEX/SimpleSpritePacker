namespace SimpleSpritePacker
{
    using System;

    using UnityEngine;

    [Serializable]
    public struct SpriteMetadata
    {
        public string Name;
        public Texture2D ReplaceTexture2D;

        // Constructors

        public SpriteMetadata(string name)
        {
            Name = name;
            ReplaceTexture2D = null;
        }

        public SpriteMetadata(string name, Texture2D texture)
        {
            Name = name;
            ReplaceTexture2D = texture;
        }
    }
}