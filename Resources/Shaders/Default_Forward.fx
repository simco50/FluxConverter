float4x4 gWorld : WORLD;
float4x4 gWorldViewProj : WORLDVIEWPROJECTION; 
float3 gLightDirection = float3(-0.577f, -0.577f, 0.577f);

struct VS_INPUT
{
	float3 pos : POSITION;
	float3 normal : NORMAL;
};

struct VS_OUTPUT
{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL;
};

RasterizerState BackfaceCull
{
	CullMode = BACK;
};

VS_OUTPUT VS(VS_INPUT input)
{
	VS_OUTPUT output;
	output.pos = mul ( float4(input.pos,1.0f), gWorldViewProj );
	output.normal = normalize(mul(input.normal, (float3x3)gWorld));
	return output;
}

float4 PS(VS_OUTPUT input) : SV_TARGET
{
	float4 color = float4(1, 1, 1, 1);
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