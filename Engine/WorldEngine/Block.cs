using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.WorldEngine
{

    public class Block
    {
        private bool _isActive = false;

        private BlockType _type;
        public byte FaceInfo;

        public Block()
        {

        }

        public Block(BlockType type)
        {
            _type = type;
            FaceInfo = 0;
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public BlockType BlockType
        {
            get { return _type; }
            set { _type = value; }
        }
    }
}
