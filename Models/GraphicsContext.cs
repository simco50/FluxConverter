using FluxConverterTool.ImageControl;
using SharpDX.Direct3D10;

namespace FluxConverterTool.Model
{
    public class GraphicsContext
    {
        public DX10RenderCanvas RenderControl { get; set; }
        public Device1 Device { get; set; }
        public OrbitCamera Camera { get; set; }
        public RenderTargetView RenderTargetView { get; set; }
    }
}