using Assimp;
using SharpDX;
using System.Collections.Generic;

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

		public static List<Vector2D> ToTexCoord(this List<Vector3D> other)
		{
			List<Vector2D> output = new List<Vector2D>();
			foreach (Vector3D v in other)
				output.Add(new Vector2D(v.X, v.Y));
			return output;
		}
    }
}
