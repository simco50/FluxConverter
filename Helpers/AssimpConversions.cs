using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using SharpDX;

namespace FluxConverterTool.Helpers
{
    public static class AssimpConversions
    {
        public static Vector3 ToVector3(this Vector3D other)
        {
            return new Vector3(other.X, other.Y, other.Z);
        }
        public static Vector2 ToVector2(this Vector2D other)
        {
            return new Vector2(other.X, other.Y);
        }
        public static Color ToColor(this Color4D other)
        {
            return new Color(other.R, other.G, other.G, other.A);
        }
    }
}
