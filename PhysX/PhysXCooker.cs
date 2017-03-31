using System.Collections.Generic;
using System.IO;
using FluxConverterTool.Models;
using PhysxNet;
using SharpDX;

namespace FluxConverterTool.PhysX
{
    public class PhysXCooker
    {
        public void Initialize()
        {
            _foundation = new Foundation();
            _cooking = new Cooking(_foundation);
        }

        public void Dispose()
        {
            _cooking.Release();
            _foundation.Release();
        }

        public byte[] CookConvexMesh(FluxMesh mesh)
        {
            MemoryStream stream = new MemoryStream();
            ConvexMeshDesc desc = new ConvexMeshDesc(mesh.Positions.ToCookerVertices(), mesh.Indices);
            _cooking.CookConvexMesh(desc, stream);
            return stream.GetBuffer();
        }

        public byte[] CookTriangleMesh(FluxMesh mesh)
        {
            MemoryStream stream = new MemoryStream();
            TriangleMeshDesc desc = new TriangleMeshDesc(mesh.Positions.ToCookerVertices(), mesh.Indices);
            _cooking.CookTriangleMesh(desc, stream);
            return stream.GetBuffer();
        }

        private Foundation _foundation;
        private Cooking _cooking;
    }

    public static class CookerExtensions
    {
        public static List<PxVec3> ToCookerVertices(this List<Vector3> sharpVerts)
        {
            List<PxVec3> output = new List<PxVec3>();
            for (int i = 0; i < sharpVerts.Count; i++)
                output.Add(new PxVec3(sharpVerts[i].X, sharpVerts[i].Y, sharpVerts[i].Z));
            return output;
        }
    }
}
