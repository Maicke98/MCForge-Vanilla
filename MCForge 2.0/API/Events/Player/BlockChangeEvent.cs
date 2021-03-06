﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCForge.Entity;

namespace MCForge.API.Events {
    /// <summary>
    /// PlayerBlockChange event class
    /// </summary>
    public class BlockChangeEvent : Event<Player, BlockChangeEventArgs> {
    }
    /// <summary>
    /// PlayerBlockChangeEventArgs
    /// </summary>
    public class BlockChangeEventArgs : EventArgs, ICancelable {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="x">The positions X coordinate</param>
        /// <param name="y">The positions Y coordinate</param>
        /// <param name="z">The positions Z coordinate</param>
        /// <param name="action">The ActionType action</param>
        /// <param name="holding">The type of the block</param>
        public BlockChangeEventArgs(ushort x, ushort y, ushort z, ActionType action, byte holding) : this(action, holding, x, y, z) { }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="x">The positions X coordinate</param>
        /// <param name="y">The positions Y coordinate</param>
        /// <param name="z">The positions Z coordinate</param>
        /// <param name="action">The ActionType action</param>
        /// <param name="holding">The type of the block</param>
        public BlockChangeEventArgs(ActionType action, byte holding, ushort x, ushort y, ushort z) {
            this.Action = action;
            this.Holding = holding;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        /// <summary>
        /// What we arre doing with this block
        /// </summary>
        public ActionType Action { get; private set; }
        /// <summary>
        /// The block at the coordinates
        /// </summary>
        public byte Holding { get; private set; }
        /// <summary>
        /// The x coordinate of the block changed.
        /// </summary>
        public ushort X { get; private set; }
        /// <summary>
        /// The y coordinate of the block changed.
        /// </summary>
        public ushort Y { get; private set; }
        /// <summary>
        /// The z coordinate of the block changed.
        /// </summary>
        public ushort Z { get; private set; }
        private bool canceled = false;
        /// <summary>
        /// Whether or not the handling should be canceled
        /// </summary>
        public bool Canceled {
            get { return canceled; }
        }
        /// <summary>
        /// Cancels the handling
        /// </summary>
        public void Cancel() {
            canceled = true;
        }
        /// <summary>
        /// Allows the handling
        /// </summary>
        public void Allow() {
            canceled = false;
        }
    }
    public enum ActionType : byte {
        Delete,
        Place
    }

}