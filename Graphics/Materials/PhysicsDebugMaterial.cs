using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluxConverterTool.Models;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;

namespace FluxConverterTool.Graphics.Materials
{
    public class PhysicsDebugMaterial : Material
    {
        private EffectMatrixVariable _worldMatrixVar;
        private EffectMatrixVariable _wvpMatrixVar;

        public PhysicsDebugMaterial(GraphicsContext context) : base(context)
        {
        }

        public override void Initialize()
        {
            CompilationResult result = ShaderBytecode.CompileFromFile("./Resources/Shaders/PhysicsDebug.fx", "fx_4_0");
            if (result.HasErrors)
                return;
            Effect = new Effect(Context.Device, result.Bytecode);
            Technique = Effect.GetTechniqueByIndex(0);

            InputElement[] vertexLayout =
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0)
            };
            InputLayout = new InputLayout(Context.Device, Technique.GetPassByIndex(0).Description.Signature,
                vertexLayout);

            LoadShaderVariables();
        }

        protected override void LoadShaderVariables()
        {
            _wvpMatrixVar = Effect.GetVariableBySemantic("WORLDVIEWPROJECTION").AsMatrix();
            _worldMatrixVar = Effect.GetVariableBySemantic("WORLD").AsMatrix();
        }

        public override void UpdateShaderVariables(FluxMesh mesh)
        {
            _wvpMatrixVar.SetMatrix(Matrix.Identity * Context.Camera.ViewProjectionMatrix);
            _worldMatrixVar.SetMatrix(Matrix.Identity);
        }
    }
}
