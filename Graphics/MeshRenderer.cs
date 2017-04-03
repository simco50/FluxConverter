using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assimp;
using FluxConverterTool.Graphics.ImageControl;
using FluxConverterTool.Graphics.Materials;
using FluxConverterTool.Helpers;
using FluxConverterTool.Models;
using SharpDX;
using SharpDX.D3DCompiler;
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

    public class MeshRenderer
    {
        private Buffer _indexBuffer;
        private Buffer _vertexBuffer;

        private FluxMesh _mesh;

        private GraphicsContext _context;

        private Material _defaultMaterial;
        private Material _physicsDebugMaterial;

        public void SetMesh(object mesh)
        {
            _mesh = mesh as FluxMesh;
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

        public void Initialize(GraphicsContext context)
        {
            _context = context;
 
            _defaultMaterial = new DefaultForwardMaterial(context);
            _defaultMaterial.Initialize();

            _physicsDebugMaterial = new PhysicsDebugMaterial(context);
            _physicsDebugMaterial.Initialize();

            DebugLog.Log($"Initialized", "Mesh Renderer");
        }

        public void Shutdown()
        {
            if (_vertexBuffer != null)
                Disposer.RemoveAndDispose(ref _vertexBuffer);
            if (_indexBuffer != null)
                Disposer.RemoveAndDispose(ref _indexBuffer);

            _defaultMaterial.Shutdown();
            _physicsDebugMaterial.Shutdown();

            DebugLog.Log($"Shutdown", "Mesh Renderer");
        }

        void CreateBuffers()
        {
            if (_vertexBuffer != null)
                Disposer.RemoveAndDispose(ref _vertexBuffer);
            if (_indexBuffer != null)
                Disposer.RemoveAndDispose(ref _indexBuffer);

            BufferDescription desc = new BufferDescription();
            desc.SizeInBytes = sizeof(uint) * _mesh.Indices.Count;
            desc.BindFlags = BindFlags.IndexBuffer;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            DataStream stream = DataStream.Create(_mesh.Indices.ToArray(), false, false);
            _indexBuffer = new Buffer(_context.Device, stream, desc);

            desc = new BufferDescription();
            desc.SizeInBytes = Marshal.SizeOf(typeof(VertexPosNormTanTex)) * _mesh.Positions.Count;
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;

            List<VertexPosNormTanTex> vertices = new List<VertexPosNormTanTex>();
            for (int i = 0; i < _mesh.Positions.Count; i++)
            {
                VertexPosNormTanTex vertex = new VertexPosNormTanTex();
                if (_mesh.Positions.Count != 0)
                    vertex.Position = _mesh.Positions[i];
                if (_mesh.Normals.Count != 0)
                    vertex.Normal = _mesh.Normals[i];
                if (_mesh.UVs.Count != 0)
                    vertex.TexCoord = _mesh.UVs[i];
                if (_mesh.Tangents.Count != 0)
                    vertex.Tangent = _mesh.Tangents[i];
                vertices.Add(vertex);
            }

            stream = DataStream.Create(vertices.ToArray(), false, false);
            _vertexBuffer = new Buffer(_context.Device, stream, desc);

            DebugLog.Log($"Buffers initialized", "Mesh Renderer");
        }

        public void Render()
        {
            if (_mesh == null)
                return;

            _context.Device.InputAssembler.InputLayout = _defaultMaterial.InputLayout;
            _context.Device.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            _context.Device.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(_vertexBuffer, Marshal.SizeOf(typeof(VertexPosNormTanTex)), 0));
            _context.Device.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            _defaultMaterial.UpdateShaderVariables(_mesh);

            EffectTechniqueDescription desc = _defaultMaterial.Technique.Description;
            for (int i = 0; i < desc.PassCount; i++)
            {
                _defaultMaterial.Technique.GetPassByIndex(i).Apply();
                _context.Device.DrawIndexed(_mesh.Indices.Count, 0, 0);
            }
        }
    }
}
