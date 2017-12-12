using System;

namespace Aleab.LoopbackAudioVisualizer.Common
{
    [Serializable]
    public struct Point
    {
        public static readonly Point Empty = new Point();

        private int x;
        private int y;

        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        public bool IsEmpty { get { return this.x == 0 && this.y == 0; } }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;
            Point comp = (Point)obj;
            return comp.X == this.X && comp.Y == this.Y;
        }

        public override int GetHashCode()
        {
            return this.x ^ this.y;
        }
    }
}