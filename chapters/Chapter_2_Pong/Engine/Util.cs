using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SlimDX;

namespace Chapter2Pong.Engine {
    /// <summary>
    /// Catch-all utility class
    /// </summary>
    public static class Util {
        private static readonly Random Rand;

        static Util()  {
            Rand = new Random();
        }

        public static bool RandomBool() {
            return Rand.Next()%2 == 0;
        }

        public static float Random(float min, float max) {
            double mantissa = Rand.NextDouble();
            return min + ((float)(mantissa)) * (max-min) ;
        }

        /// <summary>
        /// Utility method to release COM DirectX objects cleanly
        /// Object reference will be null after this method returns
        /// </summary>
        /// <typeparam name="T">Any IDisposable object</typeparam>
        /// <param name="x">Reference to the object to be released</param>
        public static void ReleaseCom<T>(ref T x) where T : class, IDisposable {
            if (x != null) {
                x.Dispose();
                x = null;
            }
        }
        /// <summary>
        /// Win32 hackery - WindowProc messages sometime pack multiple values into different parts of the lParam argument
        /// Returns the low-order WORD (16-bits) of a 32-bit value
        /// </summary>
        /// <param name="i">32-bit integer</param>
        /// <returns>low 16-bits</returns>
        public static int LowWord(this int i) {
            return i & 0xFFFF;
        }
        /// <summary>
        /// Win32 hackery - WindowProc messages sometime pack multiple values into different parts of the lParam argument
        /// Returns the high-order WORD (16-bits) of a 32-bit value
        /// </summary>
        /// <param name="i">32-bit integer</param>
        /// <returns>high 16-bits</returns>
        public static int HighWord(this int i) {
            return (i >> 16) & 0xFFFF;
        }

        public static PointF ToPointF( this Vector2 v) {
            return new PointF(v.X, v.Y);
        }

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        public static bool IsKeyDown(Keys key) {
            return (GetAsyncKeyState(key) & 0x8000) != 0;
        }

        public static bool CircleIntersectsLine(Vector2 center, float r, Vector2 p0, Vector2 p1) {
            var ac = center - p0;
            var ab = p1 - p0;

            var ab2 = Vector2.Dot(ab, ab);
            var acab = Vector2.Dot(ac, ab);

            var t = acab / ab2;
            if (t < 0) {
                t = 0;
            } else if (t > 1) {
                t = 1;
            }
            var h = ((ab*t) + p0) - center;
            var h2 = Vector2.Dot(h, h);
            return h2 < (r*r);
        }
    }
}