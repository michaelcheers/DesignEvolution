using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignEvolution
{
    public struct Block
    {
        public BlockType Type { get { return (BlockType)TypeByte; } set { TypeByte = (byte)value; } }
        public int ControllerIdx;
        public byte SunlightAmount, EnergyAmount, TypeByte;
    }
}
