using FluxConverterTool.Graphics.ImageControl;
using FluxConverterTool.Models;
using SharpDX.Direct3D10;

namespace FluxConverterTool.Graphics.Materials
{
    public abstract class Material
    {
        public Material(GraphicsContext context)
        {
            Context = context;
        }

        public void Shutdown()
        {
            if (Effect != null)
                Disposer.RemoveAndDispose(ref Effect);
            if (InputLayout != null)
                Disposer.RemoveAndDispose(ref InputLayout);
        }

        public abstract void Initialize();
        protected abstract void LoadShaderVariables();
        public abstract void UpdateShaderVariables(FluxMesh mesh);

        public Effect Effect;
        public EffectTechnique Technique;
        public InputLayout InputLayout;

        protected GraphicsContext Context;
    }
}
