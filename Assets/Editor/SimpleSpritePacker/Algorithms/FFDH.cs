namespace SimpleSpritePacker
{
    using System.Linq;
    using System.Collections.Generic;

    using UnityEngine;

    public static class FFDH
    {
        private static List<TextureRect> _packedTextures = new List<TextureRect>();
        private static List<TextureRect> _unpackedTextures = new List<TextureRect>();

        // Methods

        public static Texture2D Pack(int width, int height, int padding, List<Texture2D> textures, out List<TextureRect> packedTextures, out List<TextureRect> unpackedTextures)
        {
            _packedTextures = new List<TextureRect>();
            _unpackedTextures = new List<TextureRect>();

            textures = (from texture in textures
                         orderby texture.height descending, texture.name
                         select texture).ToList();

            List<SpaceRect> spaceList = new List<SpaceRect>();
            spaceList.Add(new SpaceRect(0, 0, width, height));

            foreach (Texture2D texture in textures)
            {
                for (int i = spaceList.Count - 1; i >= 0; i--)
                {
                    SpaceRect space = spaceList[i];

                    if (texture.width + padding > space.Width || texture.height + padding > space.Height)
                    {
                        if (!IsUnpacked(texture))
                            _unpackedTextures.Add(new TextureRect(texture, space));

                        continue;
                    }

                    RemoveFromUnpacked(texture);
                    _packedTextures.Add(new TextureRect(texture, space));

                    if (texture.width + padding == space.Width && texture.height + padding == space.Height)
                    {
                        var lastSpace = spaceList[spaceList.Count - 1];
                        spaceList.RemoveAt(spaceList.Count - 1);

                        if (i < spaceList.Count) spaceList[i] = lastSpace;
                    }
                    else if (texture.height + padding == space.Height)
                    {
                        space.X += texture.width + padding;
                        space.Width -= texture.width + padding;
                    }
                    else if (texture.width + padding == space.Width)
                    {
                        space.Y += texture.height + padding;
                        space.Height -= texture.height + padding;
                    }
                    else
                    {
                        spaceList.Add(new SpaceRect(space.X + (texture.width + padding), space.Y, space.Width - (texture.width + padding), texture.height + padding));

                        space.Y += texture.height + padding;
                        space.Height -= texture.height + padding;
                    }

                    break;
                }
            }

            Texture2D _outputTexture = new Texture2D(width, height);

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    _outputTexture.SetPixel(i, j, Color.clear);

            for (int i = 0; i < _packedTextures.Count; i++)
                _outputTexture.SetPixels(_packedTextures[i].X, _packedTextures[i].Y, _packedTextures[i].Width, _packedTextures[i].Height, _packedTextures[i].Texture.GetPixels());

            _outputTexture.Apply();

            packedTextures = _packedTextures;
            unpackedTextures = _unpackedTextures;

            _packedTextures = null;
            _unpackedTextures = null;

            return _outputTexture;
        }

        public static bool IsUnpacked(Texture2D texture)
        {
            for (int i = 0; i < _unpackedTextures.Count; i++)
                if (_unpackedTextures[i].Texture == texture)
                    return true;

            return false;
        }

        public static void RemoveFromUnpacked(Texture2D texture)
        {
            for (int i = 0; i < _unpackedTextures.Count; i++)
            {
                if (_unpackedTextures[i].Texture == texture)
                {
                    _unpackedTextures.RemoveAt(i);
                    return;
                }
            }
        }
    }
}