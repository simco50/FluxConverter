﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using FluxConverterTool.Graphics.ImageControl;
using FluxConverterTool.Helpers;
using FluxConverterTool.Models;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using SharpDX.DXGI;

namespace FluxConverterTool.Graphics
{
    public struct VertexPosNormTanTex
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector3 Tangent;
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

        private EffectMatrixVariable _worldMatrixVar;
        private EffectMatrixVariable _wvpMatrixVar;
        private EffectShaderResourceVariable _diffuseTextureVar;
        private EffectScalarVariable _useDiffuseTextureVar;
        private EffectShaderResourceVariable _normalTextureVar;
        private EffectScalarVariable _useNormalTextureVar;

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
                if(_mesh.DiffuseTexture != null)
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

            CompilationResult result = ShaderBytecode.CompileFromFile("./Resources/Shaders/Default_Forward.fx", "fx_4_0");
            if (result.HasErrors)
                return;
            _effect = new Effect(_context.Device, result.Bytecode);
            _technique = _effect.GetTechniqueByIndex(0);

            _useDiffuseTextureVar = _effect.GetVariableByName("gUseDiffuseTexture").AsScalar();
            _diffuseTextureVar = _effect.GetVariableByName("gDiffuseTexture").AsShaderResource();
            _useNormalTextureVar = _effect.GetVariableByName("gUseNormalTexture").AsScalar();
            _normalTextureVar = _effect.GetVariableByName("gNormalTexture").AsShaderResource();
            _wvpMatrixVar = _effect.GetVariableBySemantic("WORLDVIEWPROJECTION").AsMatrix();
            _worldMatrixVar = _effect.GetVariableBySemantic("WORLD").AsMatrix();

            InputElement[] vertexLayout =
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 20, 0, InputClassification.PerVertexData, 0),
                new InputElement("TANGENT", 0, Format.R32G32B32_Float, 32, 0, InputClassification.PerVertexData, 0),
            };
            _inputLayout = new InputLayout(_context.Device, _technique.GetPassByIndex(0).Description.Signature, vertexLayout);

            DebugLog.Log($"Initialized", "Mesh Renderer");
        }

        public void Shutdown()
        {
            if (_vertexBuffer != null)
                Disposer.RemoveAndDispose(ref _vertexBuffer);
            if (_indexBuffer != null)
                Disposer.RemoveAndDispose(ref _indexBuffer);
            if(_effect != null)
                Disposer.RemoveAndDispose(ref _effect);
            DebugLog.Log($"Shutdown", "Mesh Renderer");
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
            desc.SizeInBytes = Marshal.SizeOf(typeof(VertexPosNormTanTex)) * _mesh.Positions.Count;
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;

            List<VertexPosNormTanTex> vertices = new List<VertexPosNormTanTex>();
            for (int i = 0; i < _mesh.Positions.Count; i++)
            {
                VertexPosNormTanTex vertex = new VertexPosNormTanTex();
                if(_mesh.Positions.Count != 0)
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

            _context.Device.InputAssembler.InputLayout = _inputLayout;
            _context.Device.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            _context.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, Marshal.SizeOf(typeof(VertexPosNormTanTex)), 0));
            _context.Device.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            _wvpMatrixVar.SetMatrix(Matrix.Identity * _context.Camera.ViewProjectionMatrix);
            _worldMatrixVar.SetMatrix(Matrix.Identity);
            _useDiffuseTextureVar.Set(_mesh.DiffuseTexture != null);
            if(_mesh.DiffuseTexture != null)
                _diffuseTextureVar.SetResource(_mesh.DiffuseTexture);
            _useNormalTextureVar.Set(_mesh.NormalTexture != null);
            if(_mesh.NormalTexture != null)
                _normalTextureVar.SetResource(_mesh.NormalTexture);

            EffectTechniqueDescription desc = _technique.Description;
            for (int i = 0; i < desc.PassCount; i++)
            {
                _technique.GetPassByIndex(i).Apply();
                _context.Device.DrawIndexed(_mesh.Indices.Count, 0, 0);
            }
        }
    }
}
