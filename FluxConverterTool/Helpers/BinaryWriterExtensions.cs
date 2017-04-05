using System.IO;
using Assimp;
using SharpDX;

namespace FluxConverterTool.Helpers
{
    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, Vector3D v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            writer.Write(v.Z);
        }
        public static void Write(this BinaryWriter writer, Vector2D v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
        }
        public static void Write(this BinaryWriter writer, Color4D v)
        {
            writer.Write(v.R);
            writer.Write(v.G);
            writer.Write(v.B);
            writer.Write(v.A);
        }
    }
}
