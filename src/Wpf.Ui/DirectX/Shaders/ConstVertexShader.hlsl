// Assets/ConstVertexShader.hlsl
cbuffer ViewProjection : register(b0)
{
    float4 Transform; // (scale.x, scale.y, offset.x, offset.y)
};

struct VSInput
{
    float3 position : POSITION;
};

struct VSOutput
{
    float4 position : SV_POSITION;
};

VSOutput VSMain(VSInput input)
{
    VSOutput output;
    float2 transformed = input.position.xy * Transform.xy + Transform.zw;
    output.position = float4(transformed, 0.0f, 1.0f);
    return output;
}