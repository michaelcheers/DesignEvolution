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
            public BlockType blockType;
            public int blockNum;
            public Direction direction;
        }
    }
}