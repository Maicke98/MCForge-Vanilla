﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCForge.World.Blocks
{
    public class Stone : Block
    {
        public override string Name
        {
            get { return "stone"; }
        }
        public override byte VisableBlock
        {
            get { return 1; }
        }
    }
}
