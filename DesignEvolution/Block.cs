﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignEvolution
{
    public struct Block
    {
        public BlockType Type;
        public Organism Controller;
        public byte SunlightAmount, EnergyAmount;
    }
}