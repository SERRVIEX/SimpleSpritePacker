namespace SimpleSpritePacker.Algorithms.FirstFitDecreasingHeight
{
    using UnityEngine;

    public sealed class FFDHTextureNode : TextureNode
    {
        public FFDHTextureNode(Texture2D texture, FFDHSpaceNode space)
        {
            Texture = texture;

            X = space.X;
            Y = space.Y;
            Width = texture.width;
            Height = texture.height;
        }
    }
}