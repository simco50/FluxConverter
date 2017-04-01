﻿using FluxConverterTool.Graphics.ImageControl;
using FluxConverterTool.Helpers;
using FluxConverterTool.Models;
using SharpDX.Direct3D10;
using Device1 = SharpDX.Direct3D10.Device1;

namespace FluxConverterTool.Graphics
{
    public class D3DViewport : IDX10Viewport
    {
        public MeshRenderer MeshRenderer;

        private Grid _grid;

        public GraphicsContext Context = new GraphicsContext();

        public void Initialize(Device1 device, RenderTargetView renderTarget, DX10RenderCanvas canvasControl)
        {
            Context.Camera = new OrbitCamera(canvasControl);
            Context.Device = device;
            Context.RenderControl = canvasControl;
            Context.RenderTargetView = renderTarget;

            _grid = new Grid();
            _grid.Initialize(Context);
        
            MeshRenderer = new MeshRenderer();
            MeshRenderer.Initialize(Context);
            DebugLog.Log($"Initialized", "Viewport");

        }

        public void Deinitialize()
        {
            MeshRenderer.Shutdown();
            DebugLog.Log($"Shutdown", "Viewport");
        }

        public void Update(float deltaT)
        {
            Context.Camera.Update(deltaT);
        }

        public void Render(float deltaT)
        {
            MeshRenderer.Render();
            _grid.Render();
        }
    }
}
