#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Uniforms
float2 ScreenSize; // The size of the screen in pixels (for calculating positions)
texture AtlasTexture; // The texture atlas of the font
float2 AtlasSize; // The size of the texture atlas
float PxRange; // The distance range in pixels used to generate the MSDF atlas

// Texture sampler (the important thing is LINEAR Min/Mag filters)
sampler GlyphSampler = sampler_state {
	Texture = (AtlasTexture);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

// Vertex shader input (also the layout of the vertices)
struct VSIn {
    float3 LocalPos : POSITION;
    float4 Color : COLOR;
    float2 TexCoord : TEXCOORD0;
    float2 LocalCoord : TEXCOORD1;
    float2 GlyphSize : TEXCOORD2;
};

// Vertex shader output (pixel shader input)
struct VSOut {
    float4 OutPos : SV_POSITION;
    float4 Color: COLOR;
    float2 TexCoord : TEXCOORD0;
    float2 LocalCoord : TEXCOORD1;
    float2 GlyphSize : TEXCOORD2;
};

// Main vertex shader.
// Converts pixel coordinates to screen-space (-1 - 1) coordinates
VSOut MainVS(in VSIn input) {
	VSOut output = (VSOut)0;
    output.OutPos = float4(
        input.LocalPos.x / ScreenSize.x * 2 - 1.0f,
        input.LocalPos.y / ScreenSize.y * 2 - 1.0f,
        0,
        1.0
    );
    output.TexCoord = float2(
        input.TexCoord.x / AtlasSize.x,
        input.TexCoord.y / AtlasSize.y
    );
    output.Color = input.Color;
    output.LocalCoord = input.LocalCoord;
    output.GlyphSize = input.GlyphSize;
    return output;
}

float Median(float r, float g, float b) {
    return max(min(r, g), min(max(r, g), b));
}

// Main pixel (fragment) shader
// Sacrifices my firstborn in order to draw the glyph with anti-aliasing
float4 MainPS(VSOut input) : COLOR {
	float2 msdfUnit = PxRange / input.GlyphSize;
	float3 samp = tex2D(GlyphSampler, input.TexCoord).rgb;

	float sigDist = Median(samp.r, samp.g, samp.b) - 0.5f;
	sigDist = sigDist * dot(msdfUnit, 0.5f / fwidth(input.LocalCoord));

	float opacity = clamp(sigDist + 0.5f, 0.0f, 1.0f);
	return input.Color * opacity;
}

// Why is hlsl so fucking verbose
technique Main {
	pass P0 {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};