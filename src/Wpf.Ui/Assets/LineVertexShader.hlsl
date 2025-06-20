cbuffer ViewProjection : register(b0)
{
    float2 scale;
    float2 offset;
};

struct VSInput
{
    float3 position : POSITION;
    float4 color : COLOR;
};

struct VSOutput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

VSOutput VSMain(VSInput input)
{
    VSOutput output;
    float2 transformed = (input.position.xy + offset) * scale;
    output.position = float4(transformed, 0.0f, 1.0f);
    output.color = input.color;
    return output;
}