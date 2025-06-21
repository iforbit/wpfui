cbuffer GraphColor : register(b1)
{
    float4 graphColor; // RGBA
};

struct PSInput
{
    float4 position : SV_POSITION;
};

float4 PSMain(PSInput input) : SV_TARGET
{
    return graphColor;
}