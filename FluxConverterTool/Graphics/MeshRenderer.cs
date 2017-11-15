using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assimp;
using FluxConverterTool.Graphics.ImageControl;
using FluxConverterTool.Graphics.Materials;
using FluxConverterTool.Helpers;
using FluxConverterTool.Models;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Material = FluxConverterTool.Graphics.Materials.Material;

namespace FluxConverterTool.Graphics
{
    public struct VertexPosNormTanTex
    {
        public Vector3D Position;
        public Vector2D TexCoord;
        public Vector3D Normal;
        public Vector3D Tangent;
    }

    public class Model
    {
        public Model(SubMesh mesh)
        {
            _subMesh = mesh;
        }
        private SubMesh _subMesh;

        public void CreateBuffers(GraphicsContext context)
        {
            Dispose();

             BufferDescription desc = new BufferDescription();
            desc.SizeInBytes = sizeof(uint) * _subMesh.Indices.Count;
            desc.BindFlags = BindFlags.IndexBuffer;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            DataStream stream = DataStream.Create(_subMesh.Indices.ToArray(), false, false);
            _indexBuffer = new Buffer(context.Device, stream, desc);

            desc = new BufferDescription();
            desc.SizeInBytes = Marshal.SizeOf(typeof(VertexPosNormTanTex)) * _subMesh.Positions.Count;
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;

            VertexPosNormTanTex[] vertices = new VertexPosNormTanTex[_subMesh.Positions.Count];
            for (int i = 0; i < _subMesh.Positions.Count; i++)
            {
                if (i < _subMesh.Positions.Count)
                    vertices[i].Position = _subMesh.Positions[i];
                if (i < _subMesh.Normals.Count)
                    vertices[i].Normal = _subMesh.Normals[i];
                if (i < _subMesh.TexCoords.Count)
                    vertices[i].TexCoord = _subMesh.TexCoords[i];
                if (i < _subMesh.Tangents.Count)
                    vertices[i].Tangent = _subMesh.Tangents[i];
            }

            stream = DataStream.Create(vertices, false, false);
            _vertexBuffer = new Buffer(context.Device, stream, desc);
        }

        public void Render(GraphicsContext context)
        {
            context.Device.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            context.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, Marshal.SizeOf(typeof(VertexPosNormTanTex)), 0));
            if (_subMesh.Indices.Count > 0)
                context.Device.DrawIndexed(_subMesh.Indices.Count, 0, 0);
            else
                context.Device.Draw(_subMesh.Positions.Count, 0);
        }

        public void Dispose()
        {
            if (_vertexBuffer != null)
                Disposer.RemoveAndDispose(ref _vertexBuffer);
            if (_indexBuffer != null)
                Disposer.RemoveAndDispose(ref _indexBuffer);
        }

        private Buffer _indexBuffer;
        private Buffer _vertexBuffer;
    }

    public class MeshRenderer
    {
        private FluxMesh _mesh;
        private List<Model> _models = new List<Model>();
        private GraphicsContext _context;
        private Material _defaultMaterial;

        public MeshRenderer(GraphicsContext context)
        {
            _context = context;
        }

        public void SetMesh(FluxMesh mesh)
        {
            if (mesh == _mesh)
                return;
            _mesh = mesh;
            if (_mesh == null)
                return;
            CreateBuffers();
        }

        public void SetDiffuseTexture(string filePath)
        {
            if (_mesh != null)
            {
                if (_mesh.DiffuseTexture != null)
                    _mesh.DiffuseTexture.Dispose();
                _mesh.DiffuseTexture = ShaderResourceView.FromFile(_context.Device, filePath);
            }
        }

        public void SetNormalTexture(string filePath)
        {
            if (_mesh != null)
            {
                if (_mesh.NormalTexture != null)
                    _mesh.NormalTexture.Dispose();
                _mesh.NormalTexture = ShaderResourceView.FromFile(_context.Device, filePath);
            }
        }

        public void Initialize()
        {
            _defaultMaterial = new DefaultForwardMaterial(_context);
            _defaultMaterial.Initialize();

            DebugLog.Log($"Initialized", "Mesh Renderer");
        }

        public void Shutdown()
        {
            foreach (Model model in _models)
                model.Dispose();
            _models.Clear();

            _defaultMaterial.Shutdown();

            DebugLog.Log($"Shutdown", "Mesh Renderer");
        }

        void CreateBuffers()
        {
            foreach (Model model in _models)
                model.Dispose();
            _models.Clear();

            foreach (SubMesh subMesh in _mesh.Meshes)
            {
                Model model = new Model(subMesh);
                model.CreateBuffers(_context);
                _models.Add(model);
            }

            DebugLog.Log($"Buffers initialized for mesh '{_mesh.Name}'", "Mesh Renderer");
        }

        public void Render()
        {
            if (_mesh == null)
                return;

            _context.Device.InputAssembler.InputLayout = _defaultMaterial.InputLayout;
            _context.Device.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            _defaultMaterial.UpdateShaderVariables(_mesh);

            EffectTechniqueDescription desc = _defaultMaterial.Technique.Description;
            for (int i = 0; i < desc.PassCount; i++)
            {
                _defaultMaterial.Technique.GetPassByIndex(i).Apply();

                foreach (Model model in _models)
                {
                    model.Render(_context);
                }
            }
        }
    }
}
