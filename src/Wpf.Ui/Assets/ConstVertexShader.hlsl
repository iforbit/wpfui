// Assets/ConstVertexShader.hlsl
cbuffer ViewProjection : register(b0)
{
    float2 scale;
    float2 offset;
};

struct VSInput
{
    float3 position : POSITION; // ✅ COLOR 제거
};

struct VSOutput
{
    float4 position : SV_POSITION;
};

VSOutput VSMain(VSInput input)
{
    VSOutput output;
    float2 transformed = input.position.xy * scale + offset;
    output.position = float4(transformed, 0.0f, 1.0f);
    return output;
}