﻿/*
Copyright 2011 MCForge
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
using System.IO;
using System;
using MCForge.Interface.Command;
using MCForge.Entity;
using MCForge.Core;
using MCForge.Utils;
using MCForge.Utils.Settings;

namespace CommandDll
{
    public class CmdAgree : ICommand
    {
        public string Name { get { return "Agree"; } }
        public CommandTypes Type { get { return CommandTypes.Misc; } }
        public string Author { get { return "Arrem"; } }
        public int Version { get { return 1; } }
        public string CUD { get { return ""; } }
        public byte Permission { get { return 0; } }

        public void Use(Player p, string[] args)
        {
            if (!ServerSettings.GetSettingBoolean("AgreeingToRules")) { p.SendMessage("Agreeing to rules is disabled on this server!"); return; }
            p.ExtraData.CreateIfNotExist("ReadRules", false);
            if (Server.agreed.Contains(p.Username)) { p.SendMessage("You have already agreed to the rules!"); return; }
            if (!(bool)p.ExtraData["ReadRules"]) { p.SendMessage("You need to read the /rules before you can agree!"); return; }
            Server.agreed.Add(p.Username);
            if (!File.Exists("text/agreed.txt")) { File.Create("text/agreed.txt").Close(); }
            File.AppendAllText("text/agreed.txt", p.Username + Environment.NewLine); 
            p.SendMessage("Congratulations! You have agreed to the rules. You're now able to use commands!");
        }

        public void Help(Player p)
        {
            p.SendMessage("/agree - agree to the rules");
            p.SendMessage("You will be able to use commands when you agree to the rules!");
        }

        public void Initialize()
        {
            Command.AddReference(this, "agree");
        }
    }
}

