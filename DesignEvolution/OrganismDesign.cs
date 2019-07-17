using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DesignEvolution
{
    public class OrganismDesign
    {
        public List<GrowPattern> GrowPatterns;

        public struct GrowPattern
        {
            byte _data;
            public BlockType blockType { get { return (BlockType)(_data & 7); } set { _data = (byte)((_data & ~7) | (int)value); } }
            public Direction direction { get { return (Direction)((_data >> 3) & 3); } set { _data = (byte)((_data & ~(3<<3)) | ((int)value << 3)); } }
            public byte blockNum { get { return (byte)(_data >> 5); } set { _data = (byte)((_data & ~(7 << 5)) | value << 5); } }
        }
    }
}