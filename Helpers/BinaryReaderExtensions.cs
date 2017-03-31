using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace FluxConverterTool.Helpers
{
    public static class BinaryReaderExtensions
    {
        public static void Write(this BinaryWriter writer, Vector3 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            writer.Write(v.Z);
        }
        public static void Write(this BinaryWriter writer, Vector2 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
        }
        public static void Write(this BinaryWriter writer, Color v)
        {
            writer.Write(v.R);
            writer.Write(v.G);
            writer.Write(v.B);
            writer.Write(v.A);
        }
    }
}
