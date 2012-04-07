﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCForge.World.Blocks
{
    public class Sponge : Block
    {
        public override string Name
        {
            get { return "sponge"; }
        }
        public override byte VisableBlock
        {
            get { return 19; }
        }
    }
}
