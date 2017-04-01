using System.Collections.Generic;
using SharpDX;
using GalaSoft.MvvmLight;
using PhysxNet;
using SharpDX.Direct3D10;

namespace FluxConverterTool.Models
{
    public class FluxMesh : ObservableObject
    {
        public List<Vector3> Positions { get; set; } = new List<Vector3>();
        public List<int> Indices { get; set; } = new List<int>();
        public List<Vector3> Normals { get; set; } = new List<Vector3>();
        public List<Vector3> Tangents { get; set; } = new List<Vector3>();
        public List<Vector2> UVs { get; set; } = new List<Vector2>();
        public List<Color> VertexColors { get; set; } = new List<Color>();

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
        public bool WriteTexcoords { get { return _writeTexcoords && UVs.Count > 0; } set { _writeTexcoords = value; } }

        private bool _writeColors = false;
        public bool WriteColors { get { return _writeColors && VertexColors.Count > 0; } set { _writeColors = value; } }

        public bool CookTriangleMesh { get; set; } = false;
        public bool CookConvexMesh { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool HasAnimations => false;

        public PhysicsMesh ConvexMesh { get; set; } = null;
        public PhysicsMesh TriangleMesh { get; set; } = null;

        public ShaderResourceView Texture = null;
    }
}
