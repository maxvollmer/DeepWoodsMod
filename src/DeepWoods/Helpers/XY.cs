using System;

namespace DeepWoodsMod
{
    public class XY
    {
        public int X { get; set; }
        public int Y { get; set; }
        public XY() { }
        public XY(int x, int y)
        {
            X = x;
            Y = y;
        }
        public override bool Equals(Object o)
        {
            return o is XY xy && xy.X == X && xy.Y == Y;
        }
        public override int GetHashCode()
        {
            return X ^ Y;
        }
    }
}
