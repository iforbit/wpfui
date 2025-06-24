// Assets/ConstVertexShader.hlsl
cbuffer ViewProjectionBuffer : register(b0)
{
    float4 Transform;
}

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

    // 위치 변환 (x, y 좌표에 대한 스케일 및 오프셋 적용)
    // float2 transformed = input.position.xy * Transform.xy + Transform.zw;
    //  스케일 후 오프셋 제거
    // float2 transformed = input.position.xy * Transform.xy - Transform.zw;
    
    // (input - offset) - offset을 뺀 후에 스케일 
    float2 transformed = (input.position.xy - Transform.zw) * Transform.xy;
    
    // 클립 공간으로 변환
    output.position = float4(transformed, 0.0f, 1.0f);

    return output;
}