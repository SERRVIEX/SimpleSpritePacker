namespace SimpleSpritePacker.Algorithms.FirstFitDecreasingHeight
{
    public sealed class FFDHSpaceNode : Node
    {
        public FFDHSpaceNode() { }

        public FFDHSpaceNode(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}