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
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using MCForge.Core.HeartService;
using MCForge.Entity;
using MCForge.Interface;
using MCForge.Interface.Command;
using MCForge.Robot;
using MCForge.SQL;
using MCForge.Utils;
using MCForge.Utils.Settings;
using MCForge.World;

namespace MCForge.Core {
    public static class Server {
        /// <summary>
        /// Show the first run screen?
        /// </summary>
        public static bool ShowFirstRunScreen = true;
        /// <summary>
        /// The name of the server currency.
        /// </summary>
        public static string moneys;
        /// <summary>
        /// The miniumum rank that needs to verify.
        /// </summary>
        public static Groups.PlayerGroup VerifyGroup;
        /// <summary>
        /// Do people need to use /pass?
        /// </summary>
        public static bool Verifying = false;
        /// <summary>
        /// The server owner.
        /// </summary>
        public static string owner;
        /// <summary>
        /// The rank that can destroy griefer_stone without getting kicked
        /// </summary>
        public static byte grieferstoneperm = 80;
        /// <summary>
        /// The amount of griefer_stone warns player will recieve before getting kicked
        /// </summary>
        public static int grieferstonewarns = 3;
        /// <summary>
        /// Get whether the server is currently shutting down
        /// </summary>
        public static bool shuttingDown;
        /// <summary>
        /// Get whether the server is currently fully started or not
        /// </summary>
        public static bool Started = false;
        private static System.Timers.Timer UpdateTimer;
        private static int HeartbeatInterval = 300;
        private static int HeartbeatIntervalCurrent = 0;
        private static int GroupsaveInterval = 3000;
        private static int GroupsaveIntervalCurrent = 0;
        private static int PingInterval = 10;
        private static int PingIntervalCurrent = 0;
        private static int BotInterval = 10;
        private static int BotIntervalCurrent = 0;
        static bool debug = false;
        public static bool DebugMode {
            get {
                return debug;
            }
            set { debug = value; }
        }
        public struct TempBan { public string name; public DateTime allowed; }
        public static List<TempBan> tempbans = new List<TempBan>();
        public static DateTime StartTime = DateTime.Now;
        internal static List<Player> Connections = new List<Player>();
        /// <summary>
        /// Get the current list of online players, note that if you're doing a foreach on this always add .ToArray() to the end, it solves a LOT of issues
        /// </summary>
        public static List<Player> Players = new List<Player>();
        public static int PlayerCount { get { return Players.Count; } }
        /// <summary>
        /// Get the current list of bots, note that if you're doing a foreach on this always add .ToArray() to the end, it solves a LOT of issues
        /// </summary>
        public static List<Bot> Bots = new List<Bot>();
        public static int BotCount { get { return Bots.Count; } }
        /// <summary>
        /// The current list of banned IP addresses. Note that if you do a foreach on this (or any other public list) you should always add .ToArray() to the end to avoid errors!
        /// </summary>
        public static List<string> IPBans;
        /// <summary>
        /// The list of banned usernames. Note that if you do a foreach on this (or any other public list) you should always add .ToArray() to the end to avoid errors!
        /// </summary>
        public static List<string> UsernameBans;
        /// <summary>
        /// The list of MCForge developers.
        /// </summary>
        public static readonly List<string> devs = new List<string>(new string[] { "EricKilla", "Merlin33069", "Snowl", "gamezgalaxy", "headdetect", "Gamemakergm", "cazzar", "givo", "jasonbay13", "Alem_Zupa", "7imekeeper", "ninedrafted", "Nerketur", "Serado" });
        /// <summary>
        /// List of players that need to be reviewed
        /// </summary>
        public static List<Player> reviewlist = new List<Player>();
        /// <summary>
        /// List of players that agreed to the rules
        /// </summary>
        public static List<string> agreed = new List<string>();

        public static List<string> jokermessages = new List<string>();
        /// <summary>
        /// The main level of the server, where players spawn when they first join
        /// </summary>
        public static Level Mainlevel;
        /// <summary>
        /// Determines if the chat moderation is enabled
        /// </summary>
        public static bool moderation;
        //Voting
        /// <summary>
        /// Is the server in voting mode?
        /// </summary>
        public static bool voting;
        /// <summary>
        /// Is it a kickvote?
        /// </summary>
        public static bool kickvote;
        /// <summary>
        /// Amount of yes votes.
        /// </summary>
        public static int YesVotes;
        /// <summary>
        /// Amount of no votes.
        /// </summary>
        public static int NoVotes;
        /// <summary>
        /// The player who's getting, if it's /votekick
        /// </summary>
        public static Player kicker;
        /// <summary>
        /// The minecraft.net URL of the server
        /// </summary>
        /// 
        public static string URL = "";

        /// <summary>
        /// The IRC client for the server
        /// </summary>
        public static IRC IRC = null;
        /// <summary>
        /// The default color
        /// </summary>
        public static string DefaultColor { get { return ServerSettings.GetSetting("DefaultColor"); } }

        /// <summary>
        /// This delegate is used when a command or plugin needs to call a method after a certain amount of time
        /// </summary>
        /// <param name="dataPass">This delegate passes the object that was passed to it back to the method that is to be invoked</param>
        /// <returns>this delegate returns an updated object for the datapass</returns>
        public delegate object TimedMethodDelegate(object dataPass);
        static List<TimedMethod> TimedMethodList = new List<TimedMethod>();

        public delegate void ForeachPlayerDelegate(Player p);

        public delegate void ForeachBotDelegate(Bot p);

        public static void Init() {
            Logger.Log("Debug mode started", LogType.Debug);
            //TODO Add debug messages
            //TODO load the level if it exists
            Block.InIt();
            UpdateTimer = new System.Timers.Timer(100);
            UpdateTimer.Elapsed += delegate { Update(); };
            Logger.Log("Starting update timer", LogType.Debug);
            UpdateTimer.Start();
            Logger.Log("Log timer started", LogType.Debug);
            Logger.Log("Loading DLL's", LogType.Debug);
            LoadAllDlls.Init();
            Logger.Log("Finished loading DLL's", LogType.Debug);
            Logger.Log("Sending Heartbeat..", LogType.Debug);
            Heartbeat.sendHeartbeat();

            CmdReloadCmds reload = new CmdReloadCmds();
            reload.Initialize();

            Groups.PlayerGroup.Load();

            Database.init();

            CreateCoreFiles();

            Mainlevel = Level.LoadLevel(ServerSettings.GetSetting("Main-Level"));
            if (Mainlevel == null)
                Mainlevel = Level.CreateLevel(new Vector3S(256, 256, 64), Level.LevelTypes.Hell);

            Logger.Log("Loading Bans", LogType.Debug);
            Logger.Log("IPBANS", LogType.Debug);
            IPBans = new List<string>(File.ReadAllLines("bans/IPBans.txt"));
            Logger.Log("IPBANS", LogType.Debug);
            UsernameBans = new List<string>(File.ReadAllLines("bans/NameBans.txt"));
            StartListening();
            Started = true;
            Logger.Log("[Important]: Server Started.", Color.Black, Color.White);
            IRC = new IRC();
            try {
                IRC.Start();
            }
            catch { }
        }

        static void Update() {
            HeartbeatIntervalCurrent++;
            GroupsaveIntervalCurrent++;
            PingIntervalCurrent++;
            BotIntervalCurrent++;

            Player.GlobalUpdate();

            if (HeartbeatIntervalCurrent >= HeartbeatInterval) { Heartbeat.sendHeartbeat(); HeartbeatIntervalCurrent = 0; }
            if (GroupsaveIntervalCurrent >= GroupsaveInterval) { foreach (Groups.PlayerGroup g in Groups.PlayerGroup.Groups) { g.SaveGroup(); } GroupsaveIntervalCurrent = 0; }
            if (PingIntervalCurrent >= PingInterval) { Player.GlobalPing(); }
            if (BotIntervalCurrent >= BotInterval) { Bot.HandleBots(); }

            foreach (TimedMethod TM in TimedMethodList.ToArray()) {
                TM.time--;
                if (TM.time <= 0) {
                    TM.PassBack = TM.MethodToInvoke.Invoke(TM.PassBack);
                    if (TM.repeat == 0) TimedMethodList.Remove(TM);
                    TM.repeat--;
                    TM.time = TM.consistentTime;
                }
            }
        }

        static void CreateCoreFiles() {

            //Directories first
            FileUtils.CreateDirIfNotExist("bans");
            FileUtils.CreateDirIfNotExist("text");

            FileUtils.CreateFileIfNotExist("text/badwords.txt");
            FileUtils.CreateFileIfNotExist("text/replacementwords.txt");
            FileUtils.CreateFileIfNotExist("text/agreed.txt");
            FileUtils.CreateFileIfNotExist("text/hacksmessages.txt");
            FileUtils.CreateFileIfNotExist("text/news.txt");
            FileUtils.CreateFileIfNotExist("text/jokermessages.txt", "I am a pony" + Environment.NewLine + "Rainbow Dash <3" + Environment.NewLine + "I like trains!");

            FileUtils.CreateFileIfNotExist("bans/IPBans.txt");
            FileUtils.CreateFileIfNotExist("bans/NameBans.txt");
            FileUtils.CreateFileIfNotExist("bans/BanInfo.txt");

            try {
                string[] lines = File.ReadAllLines("text/agreed.txt");
                foreach (string pl in lines) { agreed.Add(pl); }
            }
            catch { Logger.Log("Error reading agreed players!", LogType.Error); }
        }
        /// <summary>
        /// Loops through online players
        /// </summary>
        /// <param name="a"></param>
        public static void ForeachPlayer(ForeachPlayerDelegate a) {
            for (int i = 0; i < Players.Count; i++) {
                if (Players.Count > i)
                    a.Invoke(Players[i]);
            }
        }
        public static void ForeachBot(ForeachBotDelegate a) {
            for (int i = 0; i < Bots.Count; i++) {
                if (Bots.Count > i)
                    a.Invoke(Bots[i]);
            }
        }
        internal static void AddConnection(Player p) {
            Connections.Add(p);
        }
        internal static void UpgradeConnectionToPlayer(Player p) {
            Connections.Remove(p);
            Players.Add(p);
        }
        internal static void RemovePlayer(Player p) {
            Connections.Remove(p);
            Players.Remove(p);
        }
        /// <summary>
        /// Add a method to be called in a specified time for a specified number of repetitions
        /// </summary>
        /// <param name="d">The delegate representing the method to be called</param>
        /// <param name="time">The amount of milliseconds you want to wait to call this method, and to wait inbetween each calling if it repeats (note that this is rounded to 10ths of a second)</param>
        /// <param name="repeat">the number of times to repeat this call</param>
        /// <param name="PassBack">the object to pass back to the method that is called.</param>
        public static void AddTimedMethod(TimedMethodDelegate d, int time, int repeat, object PassBack) {
            TimedMethod TMS = new TimedMethod(d, time, repeat, PassBack);
            TimedMethodList.Add(TMS);
        }

        #region Socket Stuff
        private static TcpListener listener;
        private static void StartListening() {
            if (ServerSettings.GetSettingBoolean("Use-UPnP")) {
                    if (!UPnP.Discover()) {
                        Logger.Log("Your router does not support UPnP. You must port forward.", LogType.Error);
                    }
                    else {

                        UPnP.ForwardPort(ServerSettings.GetSettingInt("port"), ProtocolType.Tcp, "MCForge Server");
                        Logger.Log("Port forwarded automatically using UPnP");

                    }
            }
            try {
                listener = new TcpListener(System.Net.IPAddress.Any, ServerSettings.GetSettingInt("port"));
                listener.Start();
            }
            catch (Exception e) {
                Logger.LogError(e);
                return;
            }
            while (true) {
                try {
                    IAsyncResult ar = listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
                    break;
                }
                catch (SocketException E) {
                    Logger.LogError(E);
                    break;
                }
                catch (Exception E) {
                    Logger.LogError(E);
                    continue;
                }
            }
        }
        private static void AcceptCallback(IAsyncResult ar) {
            TcpListener listener2 = (TcpListener)ar.AsyncState;
            try {
                TcpClient clientSocket = listener2.EndAcceptTcpClient(ar);
                new Player(clientSocket);
            }
            catch { }
            if (!shuttingDown) {
                listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
            }
        }
        #endregion
        #region Log Stuff

        /// <summary>
        /// Write A message to the Console and the GuiLog using default (white on black) colors.
        /// </summary>
        /// <param name="message">The message to show</param>
        [Obsolete("Please use Logger.Log", false)]
        public static void Log(string message) {
            Logger.Log(message, Color.White, Color.Black);
        }
        /// <summary>
        /// Write an error to the Console and the GuiLog using Red on black colors
        /// </summary>
        /// <param name="E">The error exception to write.</param>
        [Obsolete("Please use Logger.Log", false)]
        public static void Log(Exception E) {

            Logger.Log("[ERROR]:", Color.Red, Color.Black, LogType.Error);
            Logger.Log(E.Message, Color.Red, Color.Black);
            Logger.Log(E.StackTrace, Color.Red, Color.Black);
        }

        /// <summary>
        /// Write a message to the console and GuiLog using a specified TextColor and BackGround Color
        /// </summary>
        /// <param name="message">The Message to show</param>
        /// <param name="TextColor">The color of the text to show</param>
        /// <param name="BackgroundColor">The color behind the text.</param>
        [Obsolete("Please use Logger.Log", false)]
        public static void Log(string message, ConsoleColor TextColor, ConsoleColor BackgroundColor) {
            var tColor = ColorUtils.ToColor(TextColor);
            var bColor = ColorUtils.ToColor(BackgroundColor);
            Logger.Log(message, tColor, bColor);
        }

        public static void OnLog(object sender, LogEventArgs args) {
            if (args == null) return;
            if (!DebugMode && args.LogType == LogType.Debug)
                return;
            var tColor = ColorUtils.ToConsoleColor(args.TextColor);
            var bColor = ColorUtils.ToConsoleColor(args.BackgroundColor);
            Console.ForegroundColor = tColor;
            Console.BackgroundColor = bColor;
            Console.WriteLine(args.Message.PadRight(Console.WindowWidth - 1));
            Console.ResetColor();
        }
        #endregion

        class TimedMethod {
            public TimedMethodDelegate MethodToInvoke;
            public int consistentTime;
            public int time;
            public int repeat;
            public object PassBack;

            public TimedMethod(TimedMethodDelegate a, int b, int c, object d) {
                MethodToInvoke = a;
                consistentTime = b / 100;
                time = b / 100;
                repeat = c;
                PassBack = d;
            }
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public static void Stop() {
            foreach (var p in Players)
                p.Kick(ServerSettings.GetSetting("ShutdownMessage"));
            shuttingDown = true;
            UpdateTimer.Stop();
            listener.Stop();
            Logger.DeInit();
        }


        /// <summary>
        /// Saves all of the levels and groups
        /// </summary>
        public static void SaveAll() {
            foreach (var l in Level.Levels)
                l.SaveToBinary();
            foreach (var g in Groups.PlayerGroup.Groups)
                g.SaveGroup();

        }
    }
}
