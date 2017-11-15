using System.Collections.Generic;
using Assimp;
using GalaSoft.MvvmLight;
using PhysxNet;
using SharpDX.Direct3D10;

namespace FluxConverterTool.Models
{
    public class VertexBoneData
    {
        public uint[] IDs = new uint[4];
        public float[] Weights = new float[4];
    }

    public struct BoundingBox
    {
        public Vector3D Center;
        public Vector3D Extents;
    }

    public class FluxMesh : ObservableObject
    {
        #region DATA

        public List<SubMesh> Meshes { get; set; } = new List<SubMesh>();

        public BoundingBox BoundingBox { get; set; } = new BoundingBox();

        public PhysicsMesh ConvexMesh { get; set; } = null;
        public PhysicsMesh TriangleMesh { get; set; } = null;

        #endregion

        #region MODEL PROPERTIES

        public string Name { get; set; }

        private bool _writePositions = true;
        public bool WritePositions { get { return Meshes.Count > 0 && _writePositions && Meshes[0].Positions.Count > 0; } set { _writePositions = value; } }

        private bool _writeIndices = true;
        public bool WriteIndices { get { return Meshes.Count > 0 && _writeIndices && Meshes[0].Indices.Count > 0; } set { _writeIndices = value; } }

        private bool _writeNormals = false;
        public bool WriteNormals { get { return Meshes.Count > 0 && _writeNormals && Meshes[0].Normals.Count > 0; } set { _writeNormals = value; } }

        private bool _writeTangents = false;
        public bool WriteTangents { get { return Meshes.Count > 0 && _writeTangents && Meshes[0].Tangents.Count > 0; } set { _writeTangents = value; } }

        private bool _writeTexcoords = false;
        public bool WriteTexcoords { get { return Meshes.Count > 0 && _writeTexcoords && Meshes[0].TexCoords.Count > 0; } set { _writeTexcoords = value; } }

        private bool _writeColors = false;
        public bool WriteColors { get { return Meshes.Count > 0 && _writeColors && Meshes[0].VertexColors.Count > 0; } set { _writeColors = value; } }

        public bool CookTriangleMesh { get; set; } = false;
        public bool CookConvexMesh { get; set; } = false;

        public bool HasAnimations => false;

        public ShaderResourceView DiffuseTexture = null;
        public ShaderResourceView NormalTexture = null;

        #endregion
    }

    public class SubMesh
    {
        public List<Vector3D> Positions { get; set; } = new List<Vector3D>();
        public List<int> Indices { get; set; } = new List<int>();
        public List<Vector3D> Normals { get; set; } = new List<Vector3D>();
        public List<Vector3D> Tangents { get; set; } = new List<Vector3D>();
        public List<Vector2D> TexCoords { get; set; } = new List<Vector2D>();
        public List<Color4D> VertexColors { get; set; } = new List<Color4D>();

        public int MaterialID = -1;

        public bool TryMerge(SubMesh other)
        {
          //  return false;

            if (MaterialID != other.MaterialID)
                return false;

            int offset = Positions.Count;

            Positions.AddRange(other.Positions);
            Normals.AddRange(other.Normals);
            Tangents.AddRange(other.Tangents);
            TexCoords.AddRange(other.TexCoords);
            VertexColors.AddRange(other.VertexColors);

            foreach (int index in other.Indices)
                Indices.Add(index + offset);

            return true;
        }
    }
}
