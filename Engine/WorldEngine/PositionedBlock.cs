using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.WorldEngine
{
    public struct PositionedBlock
    {
        public readonly Vector3i Position;
        public readonly BlockType Type;

        public PositionedBlock(Vector3i position, BlockType type)
        {
            Position = position;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj is PositionedBlock)
            {
                PositionedBlock other = (PositionedBlock)obj;
                return Position == other.Position && Type == other.Type;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(PositionedBlock a, PositionedBlock b)
        {
            return a.Position == b.Position && a.Type == b.Type;
        }

        public static bool operator !=(PositionedBlock a, PositionedBlock b)
        {
            return !(a.Position == b.Position && a.Type == b.Type);
        }
        public override int GetHashCode()
        {
            int hash = 17;
            unchecked
            {
                hash = hash * 29 + Position.GetHashCode();
                hash = hash * 29 + Type.GetHashCode();
            }
            return hash;
        }

    }
}
