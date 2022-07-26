namespace SimpleSpritePacker.Algorithms.BinaryTree
{
    using System.Linq;
    using System.Collections.Generic;

    using UnityEngine;

    using Extensions;

    /// <summary>
    /// https://github.com/jakesgordon/bin-packing/tree/6c8ea72c9cd7904b57c3d0945363a5159d7cf6c5
    /// </summary>
    public static class BinaryPacking
    {
        public static Texture2D Pack(int width, int height, int spacing, List<Texture2D> textures, out List<TextureNode> packedTextures, out List<TextureNode> unpackedTextures)
        {
            // Create nodes from input textures and order them.
            List<TreeNode> nodes = new List<TreeNode>();
            for (int i = 0; i < textures.Count; i++)
                nodes.Add(new TreeNode(textures[i], spacing));
            nodes = nodes.OrderByDescending(item => item.Height).ToList();

            // Initialize out parameters.
            packedTextures = new List<TextureNode>();
            unpackedTextures = new List<TextureNode>();

            List<TreeNode> tempPackedTextures = new List<TreeNode>();
            List<TreeNode> tempUnpackedTextures = new List<TreeNode>();

            // Create root node.
            TreeNode packer = new TreeNode(width, height);
            packer.Fit(nodes);

            // Create result texture.
            Texture2D result = new Texture2D(width, height);
            result.SetPixels32(Color.clear);

            for (int i = 0; i < nodes.Count; i++)
            {
                TreeNode node = nodes[i];

                if (node.Fitted != null)
                {
                    node.X = node.Fitted.X;
                    node.Y = node.Fitted.Y;

                    packedTextures.Add(node);
                    tempPackedTextures.Add(node);
                }
                else
                {
                    unpackedTextures.Add(node);
                    tempUnpackedTextures.Add(node);
                }
            }

            // Draw on the result texture.
            for (int i = 0; i < tempPackedTextures.Count; i++)
            {
                TreeNode node = tempPackedTextures[i];
                node.X += spacing / 2;
                node.Y += spacing / 2;

                node.Width -= spacing / 2;
                node.Height -= spacing / 2;

                result.SetPixels(node.X, node.Y, node.Width, node.Height, node.Texture.GetPixels());
            }

            // Apply pixels.
            result.Apply();

            return result;
        }
    }
}