namespace SimpleSpritePacker
{
    using System;

    using UnityEngine;
    using UnityEditor;

    [Serializable]
    public class ReplacedTexture
    {
        public GUID Id;
        public string Name;
        public Texture2D ReplaceTexture2D;

        // Constructors

        public ReplacedTexture(GUID id, string name)
        {
            Id = id;
            Name = name;
            ReplaceTexture2D = null;
        }

        public ReplacedTexture(GUID id, string name, Texture2D texture)
        {
            Id = id;
            Name = name;
            ReplaceTexture2D = texture;
        }
    }
}