using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechCraftEngine.WorldEngine
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
    }
}
