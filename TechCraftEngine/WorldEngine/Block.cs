using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine
{

    public class Block
    {
        private bool _isActive = false;

        public BlockType Type;
        public byte FaceInfo;
        
        public Block(BlockType type)
        {
            Type = type;
            FaceInfo = 0;
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
    }
}
