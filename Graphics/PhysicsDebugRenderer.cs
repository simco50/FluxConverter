using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluxConverterTool.Graphics.ImageControl;
using FluxConverterTool.Graphics.Materials;
using FluxConverterTool.Helpers;
using PhysxNet;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D10.Buffer;

namespace FluxConverterTool.Graphics
{
    public class PhysicsDebugRenderer
    {
        private Material _material;
        private Buffer _indexBuffer;
        private Buffer _vertexBuffer;

        private PhysicsMesh _physicsMesh;

        private GraphicsContext _context;

        public PhysicsDebugRenderer(GraphicsContext context)
        {
            _context = context;
        }

        public void Initialize()
        {
            _material = new PhysicsDebugMaterial(_context);
            _material.Initialize();
        }

        public void Shutdown()
        {
            if (_vertexBuffer != null)
                Disposer.RemoveAndDispose(ref _vertexBuffer);
            if (_indexBuffer != null)
                Disposer.RemoveAndDispose(ref _indexBuffer);

            _material.Shutdown();

            DebugLog.Log($"Shutdown", "Physics Debug Renderer");
        }

        public void SetPhysicsMesh(PhysicsMesh mesh)
        {
            if (mesh == _physicsMesh)
                return;
            _physicsMesh = mesh;
            CreateBuffers();
        }

        void CreateBuffers()
        {
            if (_vertexBuffer != null)
                Disposer.RemoveAndDispose(ref _vertexBuffer);
            if (_indexBuffer != null)
                Disposer.RemoveAndDispose(ref _indexBuffer);

            BufferDescription desc = new BufferDescription();
            desc.SizeInBytes = sizeof(uint) * _physicsMesh.Indices.Count;
            desc.BindFlags = BindFlags.IndexBuffer;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            DataStream stream = DataStream.Create(_physicsMesh.Indices.ToArray(), false, false);
            _indexBuffer = new Buffer(_context.Device, stream, desc);

            desc = new BufferDescription();
            desc.SizeInBytes = Marshal.SizeOf(typeof(Vector3)) * _physicsMesh.Vertices.Count;
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;

            List<Vector3> vertices = new List<Vector3>();
            foreach (PxVec3 v in _physicsMesh.Vertices)
                vertices.Add(new Vector3(v.X, v.Y, v.Z));
           
            stream = DataStream.Create(vertices.ToArray(), false, false);
            _vertexBuffer = new Buffer(_context.Device, stream, desc);

            DebugLog.Log($"Buffers initialized", "Physics Debug Renderer");
        }


        public void Render()
        {
            if (_physicsMesh == null)
                return;
            _context.Device.InputAssembler.InputLayout = _material.InputLayout;
            _context.Device.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            _context.Device.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(_vertexBuffer, Marshal.SizeOf(typeof(Vector3)), 0));
            _context.Device.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            _material.UpdateShaderVariables(null);

            EffectTechniqueDescription desc = _material.Technique.Description;
            for (int i = 0; i < desc.PassCount; i++)
            {
                _material.Technique.GetPassByIndex(i).Apply();
                _context.Device.DrawIndexed(_physicsMesh.Indices.Count, 0, 0);
            }
        }
    }
}
