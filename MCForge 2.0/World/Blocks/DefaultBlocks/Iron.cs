﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCForge.World.Blocks
{
    public class Iron : Block
    {
        public override string Name
        {
            get { return "iron"; }
        }
        public override byte VisableBlock
        {
            get { return 42; }
        }
    }
}
