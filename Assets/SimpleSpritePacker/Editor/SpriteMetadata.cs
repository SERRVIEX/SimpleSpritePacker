namespace SimpleSpritePacker
{
    using System;

    using UnityEngine;
    using UnityEditor;

    [Serializable]
    public struct SpriteMetadata
    {
        public GUID Id;
        public string Name;
        public Texture2D ReplaceTexture2D;

        // Constructors

        public SpriteMetadata(GUID id, string name)
        {
            Id = id;
            Name = name;
            ReplaceTexture2D = null;
        }

        public SpriteMetadata(GUID id, string name, Texture2D texture)
        {
            Id = id;
            Name = name;
            ReplaceTexture2D = texture;
        }
    }
}