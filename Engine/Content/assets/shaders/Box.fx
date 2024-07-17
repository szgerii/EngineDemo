#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Uniforms
float2 ScreenSize;

// Vertex shader input = vertex layout
struct VSIn {
    float3 LocalPos : POSITION;
    float2 LocalCoord : TEXCOORD0;
    float BorderRadius : TEXCOORD1;
    float BorderThickness : TEXCOORD2;
    float2 PixelSize : TEXCOORD3;
    float4 BackgroundColor : COLOR0;
    float4 BorderColor : COLOR1;
};

// Vertex shader output = fragment shasder input
struct VSOut {
    float4 OutPos : SV_POSITION;
    float2 LocalCoord : TEXCOORD0;
    float BorderRadius : TEXCOORD1;
    float BorderThickness : TEXCOORD2;
    float2 PixelSize : TEXCOORD3;
    float4 BackgroundColor : COLOR0;
    float4 BorderColor : COLOR1;
};

// Vertex shader
VSOut MainVS(in VSIn input) {
    VSOut output = (VSOut)0;
    output.OutPos = float4(
        input.LocalPos.x / ScreenSize.x * 2 - 1.0f,
        input.LocalPos.y / ScreenSize.y * 2 - 1.0f,
        0,
        1.0
    );
    output.LocalCoord = input.LocalCoord;
    output.BorderRadius = input.BorderRadius;
    output.BorderThickness = input.BorderThickness;
    output.PixelSize = input.PixelSize;
    output.BackgroundColor = input.BackgroundColor;
    output.BorderColor = input.BorderColor;
    return output;
}

// SDF for a rounded rectangle
float RectSDF(float2 p, float2 b, float r) {
    float2 d = abs(p) - b + float2(r, r);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;
}

// Fragment shader
float4 MainPS(VSOut input) : COLOR {
    float2 pos = input.PixelSize * input.LocalCoord;

    float fDist = RectSDF(pos - input.PixelSize/2.0, input.PixelSize/2.0 - input.BorderThickness / 2.0 - 1.0, input.BorderRadius);
    float blendAmmount = smoothstep(-1.0, 1.0, abs(fDist) - input.BorderThickness / 2.0);

    float4 from = input.BorderColor;
    float4 to = (fDist < 0.0) ? input.BackgroundColor : float4(0.0, 0.0, 0.0, 0.0);

    return lerp(from, to, blendAmmount);
}

// The day I understand why this shit is neccessary I will ascend to godhood
technique Main {
	pass P0 {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};