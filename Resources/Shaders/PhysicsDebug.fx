float4x4 gWorld : WORLD;
float4x4 gWorldViewProj : WORLDVIEWPROJECTION; 

struct VS_INPUT
{
	float3 pos : POSITION;
};

struct VS_OUTPUT
{
	float4 pos : SV_POSITION;
};

RasterizerState BackfaceCull
{
	CullMode = BACK;
	FillMode = WIREFRAME;
};

VS_OUTPUT VS(VS_INPUT input)
{
	VS_OUTPUT output;
	output.pos = mul ( float4(input.pos,1.0f), gWorldViewProj );
	return output;
}

float4 PS(VS_OUTPUT input) : SV_TARGET
{
	return float4(1, 1, 1, 1);
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