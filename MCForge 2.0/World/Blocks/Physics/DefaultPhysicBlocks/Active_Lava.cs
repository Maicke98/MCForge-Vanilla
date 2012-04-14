﻿/*
Copyright 2012 MCForge
Dual-licensed under the Educational Community License, Version 2.0 and
the GNU General Public License, Version 3 (the "Licenses"); you may
not use this file except in compliance with the Licenses. You may
obtain a copy of the Licenses at
http://www.opensource.org/licenses/ecl2.php
http://www.gnu.org/licenses/gpl-3.0.html
Unless required by applicable law or agreed to in writing,
software distributed under the Licenses are distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the Licenses for the specific language governing
permissions and limitations under the Licenses.
*/
using System;

namespace MCForge.World.Physics
{
    public class Active_Lava : PhysicsBlock
    {
        public override string Name
        {
            get { return "active_lava"; }
        }
        public override byte VisibleBlock
        {
            get { return 10; }
        }
        public override byte Permission
        {
            get { return 80; }
        }
        public Active_Lava(int x, int y, int z, Level l)
            : base(x, y, z, l)
        {

        }
        public override void Tick()
        {
            throw new NotImplementedException();
        }
    }
}
