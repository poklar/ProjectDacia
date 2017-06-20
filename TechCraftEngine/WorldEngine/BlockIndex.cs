using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechCraftEngine.WorldEngine
{
    public class BlockIndex
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3 Position
        {
            get { return new Vector3(X, Y, Z); }
        }

        public static BlockIndex Zero { get { return new BlockIndex(0, 0, 0); } }
        public static BlockIndex UnitX { get { return new BlockIndex(1, 0, 0); } }
        public static BlockIndex UnitY { get { return new BlockIndex(0, 1, 0); } }
        public static BlockIndex UnitZ { get { return new BlockIndex(0, 0, 1); } }
        public static BlockIndex One { get { return new BlockIndex(1, 1, 1); } }

        public BlockIndex(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BlockIndex(Vector3 position)
        {
            X = (int)Math.Floor(position.X);
            Y = (int)Math.Floor(position.Y);
            Z = (int)Math.Floor(position.Z);
        }

        public AABB GetBoundingBox()
        {
            return new AABB(Position, (this + One).Position);
        }

        public static BlockIndex operator +(BlockIndex part1, BlockIndex part2)
        {
            return new BlockIndex(part1.X + part2.X, part1.Y + part2.Y, part1.Z + part2.Z);
        }

        public static BlockIndex operator -(BlockIndex part1, BlockIndex part2)
        {
            return new BlockIndex(part1.X - part2.X, part1.Y - part2.Y, part1.Z - part2.Z);
        }

        public static BlockIndex operator *(BlockIndex part1, int part2)
        {
            return new BlockIndex(part1.X * part2, part1.Y * part2, part1.Z * part2);
        }

        public static BlockIndex operator /(BlockIndex part1, int part2)
        {
            return new BlockIndex(part1.X / part2, part1.Y / part2, part1.Z / part2);
        }

        public static BlockIndex operator %(BlockIndex part1, int part2)
        {
            return new BlockIndex(part1.X % part2, part1.Y % part2, part1.Z % part2);
        }

        public static BlockIndex operator -(BlockIndex part)
        {
            return Zero - part;
        }

        public static bool operator ==(BlockIndex part1, BlockIndex part2)
        {
            if (Equals(part1, null) != Equals(part2, null))
            {
                return false;
            }
            else if (Equals(part1, null) || Equals(part2, null))
            {
                return true;
            }
            return (part1.X == part2.X && part1.Y == part2.Y && part1.Z == part2.Z);
        }

        public static bool operator !=(BlockIndex part1, BlockIndex part2)
        {
            if (Equals(part1, null) != Equals(part2, null))
            {
                return true;
            }
            else if (Equals(part1, null) || Equals(part2, null))
            {
                return false;
            }
            return !(part1.X == part2.X && part1.Y == part2.Y && part1.Z == part2.Z);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
