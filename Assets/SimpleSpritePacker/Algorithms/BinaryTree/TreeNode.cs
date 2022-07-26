namespace SimpleSpritePacker.Algorithms.BinaryTree
{
    using System.Collections.Generic;

    using UnityEngine;

    public sealed class TreeNode : TextureNode
    {
        public bool Used;
        public TreeNode Fitted;

        private TreeNode _right;
        private TreeNode _down;
        
        // Constructors

        public TreeNode(Texture2D texture, int spacing)
        {
            Texture = texture;
            Width = texture.width + spacing / 2;
            Height = texture.height + spacing / 2;
        }

        public TreeNode(int width, int height)
        {
            X = 0;
            Y = 0;
            Width = width;
            Height = height;
        }

        public TreeNode(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        // Methods

        public void Fit(List<TreeNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                TreeNode currentNode = nodes[i];
                TreeNode node = FindNode(this, currentNode.Width, currentNode.Height);
                if (node != null)
                    currentNode.Fitted = SplitNode(node, currentNode.Width, currentNode.Height);
            }
        }

        public TreeNode FindNode(TreeNode node, int width, int height)
        {
            if (node.Used)
                return FindNode(node._right, width, height) ?? FindNode(node._down, width, height);

            if (width <= node.Width && height <= node.Height)
                return node;

            return null;
        }

        public TreeNode SplitNode(TreeNode node, int width, int height)
        {
            node.Used = true;
            node._down = new TreeNode(x: node.X, y: node.Y + height, width: node.Width, height: node.Height - height);
            node._right = new TreeNode(x: node.X + width, y: node.Y, width: node.Width - width, height: height);
            return node;
        }
    }
}