float4x4 gWorld : WORLD;
float4x4 gWorldViewProj : WORLDVIEWPROJECTION; 
float3 gLightDirection = float3(-0.577f, -0.577f, 0.577f);

bool gUseDiffuseTexture = false;
texture2D gDiffuseTexture;

bool gUseNormalTexture = false;
texture2D gNormalTexture;

struct VS_INPUT
{
	float3 pos : POSITION;
	float2 uv : TEXCOORD;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct VS_OUTPUT
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

RasterizerState BackfaceCull
{
	CullMode = BACK;
};

SamplerState samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

VS_OUTPUT VS(VS_INPUT input)
{
	VS_OUTPUT output;
	output.pos = mul ( float4(input.pos,1.0f), gWorldViewProj );
	output.uv = input.uv;
	output.normal = normalize(mul(input.normal, (float3x3)gWorld));
	output.tangent = normalize(mul(input.tangent, (float3x3)gWorld));
	return output;
}

float4 PS(VS_OUTPUT input) : SV_TARGET
{
	float4 color = float4(1, 1, 1, 1);
	if(gUseDiffuseTexture)
		color = gDiffuseTexture.Sample(samLinear, input.uv);
	if(gUseNormalTexture)
	{
		float3 binormal = cross(input.tangent, input.normal);
		float3x3 localAxis = float3x3(input.tangent, binormal, input.normal);
		float3 sampledNormal = gNormalTexture.Sample(samLinear, input.uv).xyz;
		sampledNormal = sampledNormal * 2.0f - 1.0f;
		//sampledNormal.y = -sampledNormal.y;
		sampledNormal = saturate(sampledNormal);
		input.normal = mul(sampledNormal, localAxis);
	}

	float diffuseStrength = dot(input.normal, -gLightDirection);
	color = float4(color.rgb * diffuseStrength, color.a);
	return color;
}

technique10 Default
{
    pass P0
    {
		SetRasterizerState(BackfaceCull);
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}