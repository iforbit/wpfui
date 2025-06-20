cbuffer ViewportBuffer : register(b0)
{
    float2 viewportSize;
    float2 padding;
};

cbuffer LineParamsBuffer : register(b1)
{
    float lineWidth;
    float feather;
    float2 padding2;
};


struct VSInput
{
    float3 Position : POSITION;
    float4 Color : COLOR;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
    float2 PixelPos : TEXCOORD0;
};

PSInput VSMain(VSInput input)
{
    PSInput output;
    output.Position = float4(input.Position.xy, 0.0f, 1.0f);
    output.Color = input.Color;
    output.PixelPos = ((input.Position.xy + 1.0f) * 0.5f) * viewportSize;
    return output;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 pixelCoord = input.Position.xy;
    float2 center = round(pixelCoord);

    float dist = length(pixelCoord - center);
    float alpha = saturate(1.0f - (dist - lineWidth * 0.5f) / feather);

    return float4(input.Color.rgb, 1.0f); // 알파 강제
}