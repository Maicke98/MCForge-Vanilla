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
using System;
using System.Collections.Generic;
using System.Text;
using MCForge;
using MCForge.Entity;
using MCForge.Core;
using MCForge.World;
using MCForge.Interface.Command;
using MCForge.Interface.Plugin;
using MCForge.API.PlayerEvent;

namespace CommandDll
{
    public class CmdCuboid : ICommand
    {
        public string Name { get { return "Cuboid"; } }
        public CommandTypes Type { get { return CommandTypes.Building; } }
        public string Author { get { return "Gamemakergm"; } }
        public Version Version { get { return new Version(1,0); } }
        public string CUD { get { return ""; } }
        public byte Permission { get { return 0; } }

        public void Use(Player p, string[] args)
        {
            //To-do
            //Broken types: Walls,
            //Hollows seems to make an error on GetBlock because of negative pos value >_>
            //Need testing for: Wire
            //Solid and holes are working ^_^
            CatchPos cpos = new CatchPos();
            cpos.block = 255;
            ushort unused; //For the TryParse
            switch (args.Length)
            {
                case 0:
                    Default(cpos);
                    break;
                case 1: //Block or type only
                    if (ValidSolidType(args[0]) || Block.ValidBlockName(args[0]))
                    {
                        cpos = Parser(p, true, false, args, cpos);
                        break;
                    }
                    else
                    {
                        p.SendMessage("Invalid block or type!");
                        return;
                    }
                case 2: //Block and type
                    if (ValidSolidType(args[0 | 1]) || Block.ValidBlockName(args[0 | 1]))
                    {
                        cpos = Parser(p, false, false, args, cpos);
                        break;
                    }
                    else
                    {
                        p.SendMessage("Invalid block or type!");
                        return;
                    }
                case 6: //Coordinates
                    cpos = CoordinatesParse(p, args, cpos);
                    cpos.block = 1;
                    cpos.cuboidType = SolidType.solid;
                    OnPlayerBlockChange.Register(CatchBlock, MCForge.API.Priority.Normal, cpos, p);
                    p.Click((ushort)cpos.pos.x, (ushort)cpos.pos.z, (ushort)cpos.pos.y, cpos.block);
                    OnPlayerBlockChange.Register(CatchBlock2, MCForge.API.Priority.Normal, cpos, p);
                    p.Click((ushort)cpos.secondPos.x, (ushort)cpos.secondPos.z, (ushort)cpos.secondPos.y, cpos.block);
                    return;
                case 7: //Coordinates and block or type
                    if (ValidSolidType(args[6]) || Block.ValidBlockName(args[6]))
                    {
                        cpos = Parser(p, true, true, args, cpos);
                        OnPlayerBlockChange.Register(CatchBlock, MCForge.API.Priority.Normal, cpos, p);
                        p.Click((ushort)cpos.pos.x, (ushort)cpos.pos.z, (ushort)cpos.pos.y, cpos.block);
                        OnPlayerBlockChange.Register(CatchBlock2, MCForge.API.Priority.Normal, cpos, p);
                        p.Click((ushort)cpos.secondPos.x, (ushort)cpos.secondPos.z, (ushort)cpos.secondPos.y, cpos.block);
                        return;
                    }
                    else
                    {
                        p.SendMessage("Invalid block or type!");
                        return;
                    }
                case 8: //Coordinates block and type
                    if (ValidSolidType(args[6 | 7]) || Block.ValidBlockName(args[6 | 7]))
                    {
                        cpos = Parser(p, false, true, args, cpos);
                        OnPlayerBlockChange.Register(CatchBlock, MCForge.API.Priority.Normal, cpos, p);
                        p.Click((ushort)cpos.pos.x, (ushort)cpos.pos.z, (ushort)cpos.pos.y, cpos.block);
                        OnPlayerBlockChange.Register(CatchBlock2, MCForge.API.Priority.Normal, cpos, p);
                        p.Click((ushort)cpos.secondPos.x, (ushort)cpos.secondPos.z, (ushort)cpos.secondPos.y, cpos.block);
                        return;
                    }
                    else
                    {
                        p.SendMessage("Invalid block or type!");
                        return;
                    }
                default:
                    if (ushort.TryParse(args[0 | 1 | 2 | 3 | 4 | 5], out unused))
                    {
                        p.SendMessage("You need 6 coordinates for cuboid to work like that!");
                        return;
                    }
                    else
                        p.SendMessage("Invalid arguments!");
                    Help(p);
                    return;
            }
            p.SendMessage("Place two blocks to determine the corners.");
            OnPlayerBlockChange.Register(CatchBlock, MCForge.API.Priority.Normal, cpos, p);
        }
        public void Help(Player p)
        {
            p.SendMessage("/cuboid [block] [type] - Creates a cuboid of blocks.");
            p.SendMessage("/cuboid x1 z1 y1 x2 z2 y2 [block] [type[ - Creates a cuboid at the specified coordinates.");
            p.SendMessage("Available types: <solid/hollow/walls/holes/wire/random>");
            p.SendMessage("Note that [block] and [type] are optional and can be in any order.");
        }

        public void Initialize()
        {
            string[] CommandStrings = new string[2] { "cuboid", "z" };
            Command.AddReference(this, CommandStrings);
        }
        public void CatchBlock(OnPlayerBlockChange args)
        {
            CatchPos cpos = (CatchPos)args.GetData();
            cpos.pos = new Vector3(args.GetX(), args.GetZ(), args.GetY());
            args.Player.SendBlockChange(args.GetX(), args.GetZ(), args.GetY(), args.Player.Level.GetBlock(cpos.pos));
            args.Unregister(true);
            args.Cancel(true);
            OnPlayerBlockChange.Register(CatchBlock2, MCForge.API.Priority.Normal, cpos, args.Player);
        }
        public void CatchBlock2(OnPlayerBlockChange args)
        {
            CatchPos cpos = (CatchPos)args.GetData();
            byte NewType = args.GetPlayerHolding();
            Player p = args.Player;
            ushort x = args.GetX();
            ushort z = args.GetZ();
            ushort y = args.GetY();
            args.Player.SendBlockChange(args.GetX(), args.GetZ(), args.GetY(), args.Player.Level.GetBlock(cpos.pos));
            args.Unregister(true);
            args.Cancel(true);
            List<Pos> buffer = new List<Pos>();
            ushort xx, zz, yy;
            if (cpos.block != 255)
            {
                NewType = cpos.block;
            }
            switch (cpos.cuboidType)
            {
                case SolidType.solid:
                    buffer.Capacity = Math.Abs(cpos.pos.x - x) * Math.Abs(cpos.pos.z - z) * Math.Abs(cpos.pos.y - y);
                    for (xx = Math.Min((ushort)(cpos.pos.x), x); xx <= Math.Max((ushort)(cpos.pos.x), x); ++xx)
                    {
                        for (zz = Math.Min((ushort)(cpos.pos.z), z); zz <= Math.Max((ushort)(cpos.pos.z), z); ++zz)
                        {
                            for (yy = Math.Min((ushort)(cpos.pos.y), y); yy <= Math.Max((ushort)(cpos.pos.y), y); ++yy)
                            {
                                Vector3 loop = new Vector3(xx, zz, yy);
                                if (p.Level.GetBlock(loop) != NewType)
                                {
                                    BufferAdd(buffer, xx, zz, yy);
                                }
                            }
                        }
                    }
                    break;
                case SolidType.hollow:
                    for (zz = Math.Min((ushort)cpos.pos.z, z); zz <= Math.Max(cpos.pos.z, y); ++zz)
                        for (yy = Math.Min((ushort)cpos.pos.z, z); yy <= Math.Max(cpos.pos.z, z); ++yy)
                        {
                            if (p.Level.GetBlock(cpos.pos.x, yy, zz) != NewType)
                            {
                                BufferAdd(buffer, (ushort)cpos.pos.x, yy, zz);
                            }
                            if (cpos.pos.x != x)
                            {
                                if (p.Level.GetBlock(x, yy, zz) != NewType)
                                {
                                    BufferAdd(buffer, x, yy, zz);
                                }
                            }
                        }
                    if (Math.Abs(cpos.pos.x - x) >= 2)
                    {
                        for (xx = (ushort)(Math.Min(cpos.pos.x, x) + 1); xx <= Math.Max(cpos.pos.x, x) - 1; ++xx)
                            for (yy = Math.Min((ushort)cpos.pos.y, y); yy <= Math.Max((ushort)cpos.pos.y, y); ++yy)
                            {
                                if (p.Level.GetBlock(xx, cpos.pos.z, yy) != NewType)
                                {
                                    BufferAdd(buffer, xx, (ushort)cpos.pos.z, yy);
                                }
                                if (cpos.pos.z != z)
                                {
                                    if (p.Level.GetBlock(xx, z, yy) != NewType)
                                    {
                                        BufferAdd(buffer, xx, z, yy);
                                    }
                                }
                            }
                        if (Math.Abs(cpos.pos.z - z) >= 2)
                        {
                            for (xx = (ushort)(Math.Min(cpos.pos.x, x) + 1); xx <= Math.Max(cpos.pos.x, x) - 1; ++xx)
                                for (zz = (ushort)(Math.Min(cpos.pos.z, y) + 1); zz <= Math.Max(cpos.pos.z, y) - 1; ++zz)
                                {
                                    if (p.Level.GetBlock(xx, zz, cpos.pos.y) != NewType)
                                    {
                                        BufferAdd(buffer, xx, zz, (ushort)cpos.pos.y);
                                    }
                                    if (cpos.pos.y != y)
                                    {
                                        if (p.Level.GetBlock(xx, zz, y) != NewType)
                                        {
                                            BufferAdd(buffer, xx, zz, y);
                                        }
                                    }
                                }
                        }
                    }
                    break;
                case SolidType.walls:
                    for (zz = Math.Min((ushort)cpos.pos.z, z); zz <= Math.Max(cpos.pos.z, z); ++zz)
                        for (yy = Math.Min((ushort)cpos.pos.y, y); yy <= Math.Max(cpos.pos.y, y); ++yy)
                        {
                            if (p.Level.GetBlock(cpos.pos.x, zz, yy) != NewType)
                            {
                                BufferAdd(buffer, (ushort)cpos.pos.x, zz, yy);
                            }
                            if (cpos.pos.x != x)
                            {
                                if (p.Level.GetBlock(x, zz, yy) != NewType)
                                {
                                    BufferAdd(buffer, x, zz, yy);
                                }
                            }
                        }
                    if (Math.Abs(cpos.pos.x - x) >= 2)
                    {
                        if (Math.Abs(cpos.pos.y - y) >= 2)
                        {
                            for (xx = (ushort)(Math.Min(cpos.pos.x, x) + 1); xx <= Math.Max(cpos.pos.x, x) - 1; ++xx)
                                for (zz = (ushort)(Math.Min(cpos.pos.z, z)); zz <= Math.Max(cpos.pos.z, z); ++zz)
                                {
                                    if (p.Level.GetBlock(xx, zz, (ushort)cpos.pos.y) != NewType)
                                    {
                                        BufferAdd(buffer, xx, zz, (ushort)cpos.pos.y);
                                    }
                                    if (cpos.pos.y != y)
                                    {
                                        if (p.Level.GetBlock(xx, zz, y) != NewType)
                                        {
                                            BufferAdd(buffer, xx, zz, y);
                                        }
                                    }
                                }
                        }
                    }
                    break;
                case SolidType.holes:
                    bool Checked = true, startZ, startY;

                    for (xx = Math.Min((ushort)cpos.pos.x, x); xx <= Math.Max((ushort)cpos.pos.x, x); ++xx)
                    {
                        startZ = Checked;
                        for (zz = Math.Min((ushort)cpos.pos.z, z); zz <= Math.Max((ushort)cpos.pos.z, z); ++zz)
                        {
                            startY = Checked;
                            for (yy = Math.Min((ushort)cpos.pos.y, y); yy <= Math.Max((ushort)cpos.pos.y, y); ++yy)
                            {
                                Checked = !Checked;
                                if (Checked && p.Level.GetBlock(xx, zz, yy) != NewType)
                                {
                                    BufferAdd(buffer, xx, zz, yy);
                                }
                            } Checked = !startY;
                        } Checked = !startZ;
                    }
                    break;
                case SolidType.wire:
                    for (xx = Math.Min((ushort)cpos.pos.x, x); xx <= Math.Max(cpos.pos.x, x); ++xx)
                    {
                        BufferAdd(buffer, xx, z, y);
                        BufferAdd(buffer, xx, (ushort)cpos.pos.z, y);
                        BufferAdd(buffer, xx, y, (ushort)cpos.pos.z);
                        BufferAdd(buffer, xx, (ushort)cpos.pos.z, (ushort)cpos.pos.y);
                    }
                    for (zz = Math.Min((ushort)cpos.pos.z, z); zz <= Math.Max(cpos.pos.z, z); ++zz)
                    {
                        BufferAdd(buffer, x, zz, z);
                        BufferAdd(buffer, x, zz, (ushort)cpos.pos.y);
                        BufferAdd(buffer, (ushort)cpos.pos.x, zz, y);
                        BufferAdd(buffer, (ushort)cpos.pos.x, zz, (ushort)cpos.pos.y);
                    }
                    for (yy = Math.Min((ushort)cpos.pos.y, y); yy <= Math.Max(cpos.pos.y, y); ++yy)
                    {
                        BufferAdd(buffer, x, y, zz);
                        BufferAdd(buffer, x, (ushort)cpos.pos.z, yy);
                        BufferAdd(buffer, (ushort)cpos.pos.x, z, yy);
                        BufferAdd(buffer, (ushort)cpos.pos.x, (ushort)cpos.pos.z, yy);
                    }
                    break;
                case SolidType.random:
                    Random rand = new Random();
                    for (xx = Math.Min((ushort)cpos.pos.x, x); xx <= Math.Max(cpos.pos.x, x); ++xx)
                        for (zz = Math.Min((ushort)cpos.pos.z, z); zz <= Math.Max(cpos.pos.z, z); ++zz)
                            for (yy = Math.Min((ushort)cpos.pos.y, y); yy <= Math.Max(cpos.pos.y, y); ++yy)
                            {
                                if (rand.Next(1, 11) <= 5 && p.Level.GetBlock(xx, zz, yy) != NewType)
                                {
                                    BufferAdd(buffer, xx, zz, yy);
                                }
                            }
                    break;
            }
            //Anti-tunneling permissions

            //Server force cuboid

            //Group Max Blocks permissions here
            //if(buffer.Count > p.group.maxBlocks)
            //{
            //p.SendMessage("You tried to cuboid + " buffer.Count + "blocks.");
            //p.SendMessage("You cannot cuboid more than " + p.group.maxBlocks + ".");
            //}

            //Silent pyramids == false

            //Level bufferblocks and level not instant

            //Level Blockqueue
            p.SendMessage(buffer.Count.ToString() + " blocks.");
            buffer.ForEach(delegate(Pos pos)
            {
                p.Level.BlockChange((ushort)(pos.pos.x), (ushort)(pos.pos.z), (ushort)(pos.pos.y), NewType);
            });
        }
        protected CatchPos Default(CatchPos cpos)
        {
            cpos.block = 255;
            cpos.cuboidType = SolidType.solid;
            return cpos;
        }
        protected CatchPos CoordinatesParse(Player p, string[] args, CatchPos cpos)
        {
            CCheck(p, args);
            cpos.pos.x = short.Parse(args[0]);
            cpos.pos.z = short.Parse(args[1]);
            cpos.pos.y = short.Parse(args[2]);
            cpos.secondPos.x = short.Parse(args[3]);
            cpos.secondPos.z = short.Parse(args[4]);
            cpos.secondPos.y = short.Parse(args[5]);
            return cpos;
        }

        protected void CCheck(Player p, string[] args)
        {
            foreach (var i in args)
            {
                    short unused;
                    if (!short.TryParse(i, out unused) && !ValidSolidType(i) && !Block.ValidBlockName(i))
                    {
                        p.SendMessage("Invalid coordinate \"" + i + "\"!");
                    }
            }
        }
        protected CatchPos Parser(Player p, bool one, bool coordinates, string[] args, CatchPos cpos)
        {
            ushort unused;
            if (coordinates)
            {
                if (one)
                {
                    cpos = CoordinatesParse(p, args, cpos);
                    cpos.block = (Block.ValidBlockName(args[6]) ? Block.NameToBlock(args[6]) : (byte)1);
                    cpos.cuboidType = (ValidSolidType(args[6]) ? StringToSolidType(args[6]) : SolidType.solid);
                }
                else if (!one)
                {
                    cpos = CoordinatesParse(p, args, cpos);
                    cpos.block = (Block.ValidBlockName(args[6]) ? Block.NameToBlock(args[6]) : (Block.ValidBlockName(args[7]) ? Block.NameToBlock(args[7]) : (byte)1));
                    cpos.cuboidType = (ValidSolidType(args[6]) ? StringToSolidType(args[6]) : ValidSolidType(args[7]) ? StringToSolidType(args[7]) : SolidType.solid);
                }
            }
            else
            {
                if (one)
                {
                    cpos.block = (Block.ValidBlockName(args[0]) ? Block.NameToBlock(args[0]) : Block.BlockList.AIR);
                    cpos.cuboidType = (ValidSolidType(args[0]) ? StringToSolidType(args[0]) : SolidType.solid);
                }
                else if (!one)
                {
                    cpos.block = (Block.ValidBlockName(args[0]) ? Block.NameToBlock(args[0]) : (Block.ValidBlockName(args[1]) ? Block.NameToBlock(args[1]) : (byte)255));
                    cpos.cuboidType = (ValidSolidType(args[0]) ? StringToSolidType(args[0]) : ValidSolidType(args[1]) ? StringToSolidType(args[1]) : SolidType.solid);
                }
                else if ((ushort.TryParse(args[0 | 1 | 2 | 3 | 4 | 5], out unused)))
                {
                    p.SendMessage("You need 6 coordinates for cuboid to work like that!");
                }
            }
            return cpos;
        }
        protected bool ValidSolidType(string solidTypeName)
        {
            if (Enum.IsDefined(typeof(SolidType), solidTypeName))
            {
                return true;
            }
            return false;
        }
        protected SolidType StringToSolidType(string _string)
        {
            switch (_string)
            {
                case "solid":
                    {
                        return SolidType.solid;
                    }
                case "hollow":
                    {
                        return SolidType.hollow;
                    }
                case "walls":
                    {
                        return SolidType.walls;
                    }
                case "holes":
                    {
                        return SolidType.holes;
                    }
                case "wire":
                    {
                        return SolidType.wire;
                    }
                case "random":
                    {
                        return SolidType.random;
                    }
            }
            return SolidType.solid;
        }
        protected void BufferAdd(List<Pos> list, ushort x, ushort z, ushort y)
        {
            BufferAdd(list, new Vector3(x, z, y));
        }
        protected void BufferAdd(List<Pos> list, Vector3 type)
        {
            Pos pos;
            pos.pos = type;
            list.Add(pos);
        }
        protected struct CatchPos
        {
            public SolidType cuboidType;
            public byte block;
            public Vector3 pos;
            public Vector3 secondPos;
        }
        protected struct Pos
        {
            public Vector3 pos;
        }
        protected enum SolidType { solid, hollow, walls, holes, wire, random };
    }
}