using System;

namespace Aleab.LoopbackAudioVisualizer.Common
{
    [Serializable]
    public struct Rectangle
    {
        public static readonly Rectangle Empty = new Rectangle();

        private int x;
        private int y;
        private int width;
        private int height;

        public int X
        {
            get { return x; }
            set { this.x = value; }
        }

        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public Point Location
        {
            get
            {
                return new Point(this.X, this.Y);
            }
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }

        public Size Size
        {
            get
            {
                return new Size(this.Width, this.Height);
            }
            set
            {
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }

        public int Left
        {
            get { return this.X; }
        }

        public int Top
        {
            get { return this.Y; }
        }

        public int Right
        {
            get { return this.X + this.Width; }
        }

        public int Bottom
        {
            get { return this.Y + this.Height; }
        }

        public bool IsEmpty
        {
            get { return this.height == 0 && this.width == 0 && this.x == 0 && this.y == 0; }
        }

        public Rectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public bool Contains(int x, int y)
        {
            return this.X <= x &&
                   x < this.X + this.Width &&
                   this.Y <= y &&
                   y < this.Y + this.Height;
        }

        public bool Contains(Point pt)
        {
            return this.Contains(pt.X, pt.Y);
        }

        public bool Contains(Rectangle rect)
        {
            return this.X <= rect.X &&
                   rect.X + rect.Width <= this.X + this.Width &&
                   this.Y <= rect.Y &&
                   rect.Y + rect.Height <= this.Y + this.Height;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Rectangle))
                return false;

            Rectangle comp = (Rectangle)obj;

            return comp.X == this.X &&
                   comp.Y == this.Y &&
                   comp.Width == this.Width &&
                   comp.Height == this.Height;
        }

        public override int GetHashCode()
        {
            return (int)((uint)this.X ^
                         (((uint)this.Y << 13) | ((uint)this.Y >> 19)) ^
                         (((uint)this.Width << 26) | ((uint)this.Width >> 6)) ^
                         (((uint)this.Height << 7) | ((uint)this.Height >> 25)));
        }

        public static Rectangle FromLTRB(int left, int top, int right, int bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.X == right.X
                   && left.Y == right.Y
                   && left.Width == right.Width
                   && left.Height == right.Height;
        }

        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !(left == right);
        }
    }
}