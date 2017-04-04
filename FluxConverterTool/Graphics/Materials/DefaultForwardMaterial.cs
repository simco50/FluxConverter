using FluxConverterTool.Models;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;

namespace FluxConverterTool.Graphics.Materials
{
    public class DefaultForwardMaterial : Material
    {
        public DefaultForwardMaterial(GraphicsContext context) : base(context)
        {
        }

        private EffectMatrixVariable _worldMatrixVar;
        private EffectMatrixVariable _wvpMatrixVar;
        private EffectShaderResourceVariable _diffuseTextureVar;
        private EffectScalarVariable _useDiffuseTextureVar;
        private EffectShaderResourceVariable _normalTextureVar;
        private EffectScalarVariable _useNormalTextureVar;

        protected override void LoadShaderVariables()
        {
            _useDiffuseTextureVar = Effect.GetVariableByName("gUseDiffuseTexture").AsScalar();
            _diffuseTextureVar = Effect.GetVariableByName("gDiffuseTexture").AsShaderResource();
            _useNormalTextureVar = Effect.GetVariableByName("gUseNormalTexture").AsScalar();
            _normalTextureVar = Effect.GetVariableByName("gNormalTexture").AsShaderResource();
            _wvpMatrixVar = Effect.GetVariableBySemantic("WORLDVIEWPROJECTION").AsMatrix();
            _worldMatrixVar = Effect.GetVariableBySemantic("WORLD").AsMatrix();
        }

        public override void UpdateShaderVariables(FluxMesh mesh)
        {
            _wvpMatrixVar.SetMatrix(Matrix.Identity * Context.Camera.ViewProjectionMatrix);
            _worldMatrixVar.SetMatrix(Matrix.Identity);
            _useDiffuseTextureVar.Set(mesh.DiffuseTexture != null);
            if (mesh.DiffuseTexture != null)
                _diffuseTextureVar.SetResource(mesh.DiffuseTexture);
            _useNormalTextureVar.Set(mesh.NormalTexture != null);
            if (mesh.NormalTexture != null)
                _normalTextureVar.SetResource(mesh.NormalTexture);
        }

        public override void Initialize()
        {
            CompilationResult result = ShaderBytecode.CompileFromFile("./Resources/Shaders/Default_Forward.fx", "fx_4_0");
            if (result.HasErrors)
                return;
            Effect = new Effect(Context.Device, result.Bytecode);
            Technique = Effect.GetTechniqueByIndex(0);

            InputElement[] vertexLayout =
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 20, 0, InputClassification.PerVertexData, 0),
                new InputElement("TANGENT", 0, Format.R32G32B32_Float, 32, 0, InputClassification.PerVertexData, 0),
            };
            InputLayout = new InputLayout(Context.Device, Technique.GetPassByIndex(0).Description.Signature,
                vertexLayout);

            LoadShaderVariables();
        }
    }
}
