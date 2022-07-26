namespace SimpleSpritePacker.Algorithms
{
    public class Node
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        // Methods

        public override string ToString()
        {
            return $"({X}:{Y}) ({Width}x{Height})";
        }
    }
}