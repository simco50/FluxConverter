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

    public class FluxMesh : ObservableObject
    {
        #region DATA

        public List<Vector3D> Positions { get; set; } = new List<Vector3D>();
        public List<int> Indices { get; set; } = new List<int>();
        public List<Vector3D> Normals { get; set; } = new List<Vector3D>();
        public List<Vector3D> Tangents { get; set; } = new List<Vector3D>();
        public List<Vector2D> TexCoords { get; set; } = new List<Vector2D>();
        public List<Color4D> VertexColors { get; set; } = new List<Color4D>();

        public List<Matrix4x4> BoneTransforms { get; set; } = new List<Matrix4x4>();
        public List<VertexBoneData> VertexWeights { get; set; } = new List<VertexBoneData>();

        public PhysicsMesh ConvexMesh { get; set; } = null;
        public PhysicsMesh TriangleMesh { get; set; } = null;

        #endregion

        #region MODEL PROPERTIES

        public string Name { get; set; }

        private bool _writePositions = true;
        public bool WritePositions { get { return _writePositions && Positions.Count > 0; } set { _writePositions = value; } }

        private bool _writeIndices = true;
        public bool WriteIndices { get { return _writeIndices && Indices.Count > 0; } set { _writeIndices = value; } }

        private bool _writeNormals = false;
        public bool WriteNormals { get { return _writeNormals && Normals.Count > 0; } set { _writeNormals = value; } }

        private bool _writeTangents = false;
        public bool WriteTangents { get { return _writeTangents && Tangents.Count > 0; } set { _writeTangents = value; } }

        private bool _writeTexcoords = false;
        public bool WriteTexcoords { get { return _writeTexcoords && TexCoords.Count > 0; } set { _writeTexcoords = value; } }

        private bool _writeColors = false;
        public bool WriteColors { get { return _writeColors && VertexColors.Count > 0; } set { _writeColors = value; } }

        public bool CookTriangleMesh { get; set; } = false;
        public bool CookConvexMesh { get; set; } = false;

        public bool HasAnimations => false;

        public ShaderResourceView DiffuseTexture = null;
        public ShaderResourceView NormalTexture = null;

        #endregion
    }
}
