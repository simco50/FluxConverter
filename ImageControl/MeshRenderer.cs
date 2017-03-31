using System.Collections.Generic;
using System.Runtime.InteropServices;
using FluxConverterTool.Models;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Device1 = SharpDX.Direct3D10.Device1;
using FluxConverterTool.Model;

namespace FluxConverterTool.ImageControl
{
    public struct VertexPosNorm
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    public class MeshRenderer
    {
        private Effect _effect;
        private EffectTechnique _technique;
        private InputLayout _inputLayout;

        private Buffer _indexBuffer;
        private Buffer _vertexBuffer;

        private FluxMesh _mesh;

        private GraphicsContext _context;

        public void SetMesh(FluxMesh mesh)
        {
            if (mesh == _mesh)
                return;
            _mesh = mesh;
            CreateBuffers();
        }

        public void Initialize(GraphicsContext context)
        {
            _context = context;

            CompilationResult result = ShaderBytecode.CompileFromFile("./Resources/Shaders/Default_Forward.fx", "fx_4_0");
            if (result.HasErrors)
            {
                return;
            }
            _effect = new Effect(_context.Device, result.Bytecode);
            _technique = _effect.GetTechniqueByIndex(0);

            InputElement[] vertexLayout =
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0)
            };
            _inputLayout = new InputLayout(_context.Device, _technique.GetPassByIndex(0).Description.Signature, vertexLayout);
        }

        void CreateBuffers()
        {
            if(_vertexBuffer != null)
                Disposer.RemoveAndDispose(ref _vertexBuffer);
            if(_indexBuffer != null)
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
            desc.SizeInBytes = Marshal.SizeOf(typeof(VertexPosNorm)) * _mesh.Positions.Count;
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;

            List<VertexPosNorm> vertices = new List<VertexPosNorm>();
            for (int i = 0; i < _mesh.Positions.Count; i++)
            {
                VertexPosNorm vertex = new VertexPosNorm();
                vertex.Position = _mesh.Positions[i];
                vertex.Normal = _mesh.Normals[i];
                vertices.Add(vertex);
            }

            stream = DataStream.Create(vertices.ToArray(), false, false);
            _vertexBuffer = new Buffer(_context.Device, stream, desc);
        }

        public void Render()
        {
            if (_mesh == null)
                return;

            _context.Device.InputAssembler.InputLayout = _inputLayout;
            _context.Device.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            _context.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, Marshal.SizeOf(typeof(VertexPosNorm)), 0));
            _context.Device.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            _effect.GetVariableBySemantic("WORLDVIEWPROJECTION").AsMatrix().SetMatrix(Matrix.Identity * _context.Camera.ViewProjectionMatrix);
            _effect.GetVariableBySemantic("WORLD").AsMatrix().SetMatrix(Matrix.Identity);

            EffectTechniqueDescription desc = _technique.Description;
            for (int i = 0; i < desc.PassCount; i++)
            {
                _technique.GetPassByIndex(i).Apply();
                _context.Device.DrawIndexed(_mesh.Indices.Count, 0, 0);
            }
        }
    }
}
