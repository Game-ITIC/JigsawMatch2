Shader "JigsawMatch2/Butterfly Trace"
{
    Properties
    {
        [HDR] _CoreColor ("Warm Gold Core", Color) = (1.2, 0.72, 0.24, 1)
        [HDR] _EdgeColor ("Violet Edge", Color) = (0.58, 0.2, 1.1, 0.72)
        [HDR] _AccentColor ("Aqua Shimmer", Color) = (0.18, 0.9, 1.35, 1)
        _EdgeSoftness ("Edge Softness", Range(0.5, 4)) = 1.65
        _CoreSize ("Core Size", Range(0.05, 0.9)) = 0.36
        _ShimmerScale ("Shimmer Scale", Range(0.1, 8)) = 2.2
        _ShimmerSpeed ("Shimmer Speed", Range(-5, 5)) = 1.25
        _SparkDensity ("Spark Density", Range(0.5, 12)) = 4
        _Intensity ("Glow Intensity", Range(0, 3)) = 1.1
        _Opacity ("Opacity", Range(0, 1)) = 0.92
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ButterflyTraceURP"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                half4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _EdgeColor;
                half4 _AccentColor;
                half _EdgeSoftness;
                half _CoreSize;
                half _ShimmerScale;
                half _ShimmerSpeed;
                half _SparkDensity;
                half _Intensity;
                half _Opacity;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half radial = saturate(1.0h - abs(input.uv.y * 2.0h - 1.0h));
                half body = pow(radial, _EdgeSoftness);
                half core = smoothstep(_CoreSize, 1.0h, radial);

                float travel = dot(input.positionWS.xy, float2(0.83, 1.17));
                half wave = sin((travel * _ShimmerScale - _Time.y * _ShimmerSpeed) * 6.2831853h);
                half shimmer = pow(saturate(wave * 0.5h + 0.5h), 10.0h) * body;

                float2 sparkleCell = floor(input.positionWS.xy * _SparkDensity);
                half randomValue = frac(sin(dot(sparkleCell, float2(12.9898, 78.233))) * 43758.5453);
                half twinkle = pow(saturate(sin(_Time.y * 4.0h + randomValue * 6.2831853h) * 0.5h + 0.5h), 12.0h);
                half sparkle = step(0.82h, randomValue) * twinkle * pow(radial, 7.0h);

                half3 color = lerp(_EdgeColor.rgb, _CoreColor.rgb, core);
                color += _AccentColor.rgb * (shimmer * 0.28h + sparkle * 0.8h);
                color *= input.color.rgb * _Intensity;

                half alpha = body * lerp(_EdgeColor.a, _CoreColor.a, core);
                alpha *= input.color.a * _Opacity;
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ButterflyTraceBuiltIn"

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            fixed4 _CoreColor;
            fixed4 _EdgeColor;
            fixed4 _AccentColor;
            half _EdgeSoftness;
            half _CoreSize;
            half _ShimmerScale;
            half _ShimmerSpeed;
            half _SparkDensity;
            half _Intensity;
            half _Opacity;

            struct Attributes
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                fixed4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.positionWS = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                half radial = saturate(1.0h - abs(input.uv.y * 2.0h - 1.0h));
                half body = pow(radial, _EdgeSoftness);
                half core = smoothstep(_CoreSize, 1.0h, radial);

                float travel = dot(input.positionWS.xy, float2(0.83, 1.17));
                half wave = sin((travel * _ShimmerScale - _Time.y * _ShimmerSpeed) * 6.2831853h);
                half shimmer = pow(saturate(wave * 0.5h + 0.5h), 10.0h) * body;

                float2 sparkleCell = floor(input.positionWS.xy * _SparkDensity);
                half randomValue = frac(sin(dot(sparkleCell, float2(12.9898, 78.233))) * 43758.5453);
                half twinkle = pow(saturate(sin(_Time.y * 4.0h + randomValue * 6.2831853h) * 0.5h + 0.5h), 12.0h);
                half sparkle = step(0.82h, randomValue) * twinkle * pow(radial, 7.0h);

                fixed3 color = lerp(_EdgeColor.rgb, _CoreColor.rgb, core);
                color += _AccentColor.rgb * (shimmer * 0.28h + sparkle * 0.8h);
                color *= input.color.rgb * _Intensity;

                half alpha = body * lerp(_EdgeColor.a, _CoreColor.a, core);
                alpha *= input.color.a * _Opacity;
                return fixed4(color, alpha);
            }
            ENDCG
        }
    }

    Fallback Off
}
