﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCForge.Utils {

    public class Vector2D {
        public double x;
        public double z;
        public Vector2D() {
            this.x = 0;
            this.z = 0;
        }
        public Vector2D(double X, double Z) {
            x = X;
            z = Z;
        }

        public static Vector2D operator -(Vector2D a, Vector2D b) {
            return new Vector2D((double)(a.x - b.x), (double)(a.z - b.z));
        }
        public static Vector2D operator +(Vector2D a, Vector2D b) {
            return new Vector2D((double)(a.x + b.x), (double)(a.z + b.z));
        }
        public static Vector2D operator *(Vector2D a, Vector2D b) {
            return new Vector2D((double)(a.x * b.x), (double)(a.z * b.z));
        }
        public static Vector2D operator /(Vector2D a, Vector2D b) {
            return new Vector2D((double)(a.x / b.x), (double)(a.z / b.z));
        }
        public static bool operator ==(Vector2D a, Vector2D b) {
            return (a.x == b.x && a.z == b.z);
        }
        public static bool operator !=(Vector2D a, Vector2D b) {
            return !(a.x == b.x && a.z == b.z);
        }
        public Vector2D GetMove(double distance, Vector2D towards) {
            Vector2D ret = new Vector2D(x, z);
            ret.Move(distance, towards);
            return ret;
        }
        public void Move(double distance, Vector2D towards) {
            Vector2D way = towards - this;
            double length = way.Length;
            x += (double)((way.x / length) * distance);
            z += (double)((way.z / length) * distance);
        }
        public double Length {
            get {
                return Math.Sqrt(x * x + z * z);
            }
        }
        public override bool Equals(object obj) {
            return base.Equals(obj);
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public override string ToString() {
            return String.Format("x:{0} y:{1}", x, z);
        }
    }
}
