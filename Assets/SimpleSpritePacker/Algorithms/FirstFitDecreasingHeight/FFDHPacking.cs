namespace SimpleSpritePacker.Algorithms.FirstFitDecreasingHeight
{
    using System.Linq;
    using System.Collections.Generic;

    using UnityEngine;

    using Extensions;

    /// <summary>
    /// https://github.com/mapbox/potpack
    /// </summary>
    public static class FFDHPacking
    {
        private static List<FFDHTextureNode> _tempPackedTextures = new List<FFDHTextureNode>();
        private static List<FFDHTextureNode> _tempUnpackedTextures = new List<FFDHTextureNode>();

        // Methods

        public static Texture2D Pack(int width, int height, int spacing, List<Texture2D> textures, out List<TextureNode> packedTextures, out List<TextureNode> unpackedTextures)
        {
            _tempPackedTextures = new List<FFDHTextureNode>();
            _tempUnpackedTextures = new List<FFDHTextureNode>();

            // Order textures.
            textures = textures.OrderByDescending(item => item.height).ToList();

            // Initialize spaces.
            List<FFDHSpaceNode> spaces = new List<FFDHSpaceNode>();
            // Set root space.
            spaces.Add(new FFDHSpaceNode(0, 0, width, height));

            foreach (Texture2D texture in textures)
            {
                for (int i = spaces.Count - 1; i >= 0; i--)
                {
                    FFDHSpaceNode space = spaces[i];

                    if (texture.width + spacing > space.Width || texture.height + spacing > space.Height)
                    {
                        if (!IsUnpacked(texture))
                            _tempUnpackedTextures.Add(new FFDHTextureNode(texture, space));
                        continue;
                    }

                    RemoveFromUnpacked(texture);
                    _tempPackedTextures.Add(new FFDHTextureNode(texture, space));

                    if (texture.width + spacing == space.Width && texture.height + spacing == space.Height)
                    {
                        var lastSpace = spaces[spaces.Count - 1];
                        spaces.RemoveAt(spaces.Count - 1);

                        if (i < spaces.Count) spaces[i] = lastSpace;
                    }
                    else if (texture.height + spacing == space.Height)
                    {
                        space.X += texture.width + spacing;
                        space.Width -= texture.width + spacing;
                    }
                    else if (texture.width + spacing == space.Width)
                    {
                        space.Y += texture.height + spacing;
                        space.Height -= texture.height + spacing;
                    }
                    else
                    {
                        spaces.Add(new FFDHSpaceNode(space.X + (texture.width + spacing), space.Y, space.Width - (texture.width + spacing), texture.height + spacing));

                        space.Y += texture.height + spacing;
                        space.Height -= texture.height + spacing;
                    }

                    break;
                }
            }

            // Create result texture.
            Texture2D result = new Texture2D(width, height);
            result.SetPixels32(Color.clear);

            // Draw on the result texture.
            for (int i = 0; i < _tempPackedTextures.Count; i++)
            {
                FFDHTextureNode node = _tempPackedTextures[i];
                result.SetPixels(node.X, node.Y, node.Width, node.Height, node.Texture.GetPixels());
            }

            result.Apply();

            // Initialize out parameters.
            packedTextures = new List<TextureNode>();
            unpackedTextures = new List<TextureNode>();

            for (int i = 0; i < _tempPackedTextures.Count; i++)
                packedTextures.Add(_tempPackedTextures[i]);

            for (int i = 0; i < _tempUnpackedTextures.Count; i++)
                unpackedTextures.Add(_tempUnpackedTextures[i]);

            _tempPackedTextures = null;
            _tempUnpackedTextures = null;

            return result;
        }

        private static bool IsUnpacked(Texture2D texture)
        {
            for (int i = 0; i < _tempUnpackedTextures.Count; i++)
                if (_tempUnpackedTextures[i].Texture == texture)
                    return true;

            return false;
        }

        private static void RemoveFromUnpacked(Texture2D texture)
        {
            for (int i = 0; i < _tempUnpackedTextures.Count; i++)
            {
                if (_tempUnpackedTextures[i].Texture == texture)
                {
                    _tempUnpackedTextures.RemoveAt(i);
                    return;
                }
            }
        }
    }
}