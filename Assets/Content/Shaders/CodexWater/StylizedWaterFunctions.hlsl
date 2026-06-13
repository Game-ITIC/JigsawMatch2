#ifndef CODEX_STYLIZED_WATER_INCLUDED
#define CODEX_STYLIZED_WATER_INCLUDED

float CodexWaterStripe(float value, float width)
{
    return smoothstep(1.0 - width, 1.0, value);
}

void StylizedWaterColor_float(float4 UV, float3 PositionWS, out float4 Color)
{
    float2 uv = UV.xy;
    float t = _Time.y;

    float2 worldFlow = PositionWS.xz * 0.055;
    float2 p = uv * 3.4 + worldFlow;

    float waveA = sin((p.x * 6.5 + p.y * 2.0) + t * 1.45);
    float waveB = sin((p.x * -3.2 + p.y * 7.4) - t * 1.95);
    float waveC = sin((p.x * 12.0 + p.y * 5.0) + t * 0.75);
    float softWave = waveA * 0.45 + waveB * 0.35 + waveC * 0.20;
    float wave01 = softWave * 0.5 + 0.5;

    float2 shimmerP = p + float2(t * 0.11, -t * 0.075);
    float ribbonA = sin(shimmerP.x * 14.0 + shimmerP.y * 2.4 + sin(shimmerP.y * 1.6 + t) * 0.55);
    float ribbonB = sin(shimmerP.x * 10.5 + shimmerP.y * 1.7 - t * 0.8);
    float highlights = CodexWaterStripe(ribbonA, 0.020) * 0.85 + CodexWaterStripe(ribbonB, 0.014) * 0.28;
    highlights *= smoothstep(0.55, 1.0, wave01);
    highlights *= smoothstep(0.18, 0.45, uv.y) * smoothstep(1.0, 0.72, uv.y);

    float depthMask = smoothstep(0.0, 0.85, uv.y) * 0.45 + smoothstep(0.0, 0.75, uv.x) * 0.15;
    float3 deep = float3(0.010, 0.135, 0.50);
    float3 mid = float3(0.015, 0.45, 0.78);
    float3 shallow = float3(0.30, 0.87, 1.00);
    float3 color = lerp(deep, mid, saturate(wave01 * 0.58 + depthMask));

    float fresnelSoft = pow(saturate(1.0 - abs(softWave)), 3.0) * 0.10;
    color += float3(0.16, 0.45, 0.65) * fresnelSoft;
    color += float3(0.78, 0.97, 1.0) * highlights * 0.48;

    Color = float4(saturate(color), 1.0);
}

void StylizedWaterColor_half(half4 UV, half3 PositionWS, out half4 Color)
{
    float4 result;
    StylizedWaterColor_float(float4(UV), float3(PositionWS), result);
    Color = half4(result);
}

void StylizedWaterVertex_float(float3 PositionOS, float4 UV, out float3 PositionOut)
{
    float2 uv = UV.xy;
    float t = _Time.y;

    float waveA = sin(PositionOS.x * 0.030 + PositionOS.z * 0.018 + t * 1.55);
    float waveB = sin(PositionOS.x * -0.018 + PositionOS.z * 0.037 - t * 1.25);
    float waveC = sin((uv.x + uv.y) * 18.0 + t * 2.2);
    float height = (waveA * 0.55 + waveB * 0.35 + waveC * 0.10) * 1.15;

    PositionOut = PositionOS + float3(0.0, height, 0.0);
}

void StylizedWaterVertex_half(half3 PositionOS, half4 UV, out half3 PositionOut)
{
    float3 result;
    StylizedWaterVertex_float(float3(PositionOS), float4(UV), result);
    PositionOut = half3(result);
}

#endif
