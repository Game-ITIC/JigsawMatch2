Shader "Fristy/Nature/Rocks"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0)

        [NoScaleOffset]_MainTex ("Albedo (RGB) and Occlusion (A)", 2D) = "white" {}
        _brightness ("Brightness", Range(0,2)) = 1

        [Space(20)]
        [NoScaleOffset][Normal]_MainNorm ("Normal", 2D) = "bump" {}
        _MainNormPow ("Normal Power", Range(-2,2)) = 1
        _uv ("UV", Float) = 1

        [Space(50)]
        _dexTex ("Detail Albedo", 2D) = "white" {}
        _dexTexPow ("Blend Albedo", Range(0,2)) = 0.5
        _uv2 ("Detail Albedo UV", Float) = 1

        [Space(20)]
        [NoScaleOffset][Normal]_DexNorm ("Detail Normal", 2D) = "bump" {}
        _uv3 ("Detail Normal UV", Float) = 1
        _DexNormPow ("Detail Normal Power", Range(-2,2)) = 1

        [Space(20)]
        _occlusionPow ("Occlusion Power", Range(0,1)) = 0.5
        _Glossiness ("Smoothness Highs", Range(0,1)) = 0.5
        _Glossiness1 ("Smoothness Lows", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        LOD 200
        Cull Back
        ZWrite On

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MainNorm);
            SAMPLER(sampler_MainNorm);

            TEXTURE2D(_dexTex);
            SAMPLER(sampler_dexTex);

            TEXTURE2D(_DexNorm);
            SAMPLER(sampler_DexNorm);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;

                half _brightness;

                half _MainNormPow;
                half _uv;

                half _dexTexPow;
                half _uv2;

                half _DexNormPow;
                half _uv3;

                half _occlusionPow;
                half _Glossiness;
                half _Glossiness1;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;

                float3 positionWS : TEXCOORD1;
                half3 normalWS    : TEXCOORD2;
                half4 tangentWS   : TEXCOORD3;
                half fogCoord     : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 BlendHardLight(half4 baseCol, half4 blendCol, half opacity)
            {
                half4 result1 = 1.0h - 2.0h * (1.0h - baseCol) * (1.0h - blendCol);
                half4 result2 = 2.0h * baseCol * blendCol;
                half4 zeroOrOne = step(blendCol, 0.5h);

                half4 result = result2 * zeroOrOne + (1.0h - zeroOrOne) * result1;
                return lerp(baseCol, result, opacity);
            }

            half4 BlendOverlay(half4 baseCol, half4 blendCol, half opacity)
            {
                half4 result1 = 1.0h - 2.0h * (1.0h - baseCol) * (1.0h - blendCol);
                half4 result2 = 2.0h * baseCol * blendCol;
                half4 zeroOrOne = step(baseCol, 0.5h);

                half4 result = result2 * zeroOrOne + (1.0h - zeroOrOne) * result1;
                return lerp(baseCol, result, opacity);
            }

            half3 ApplyNormalPower(half3 n, half power)
            {
                n.xy *= power;
                return normalize(n);
            }

            half3 TangentToWorldNormal(half3 normalTS, half3 normalWS, half4 tangentWS)
            {
                half3 n = normalize(normalWS);
                half3 t = normalize(tangentWS.xyz);
                half3 b = normalize(cross(n, t) * tangentWS.w);

                return normalize(
                    t * normalTS.x +
                    b * normalTS.y +
                    n * normalTS.z
                );
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;

                output.normalWS = normalInputs.normalWS;

                half tangentSign = input.tangentOS.w * GetOddNegativeScale();
                output.tangentWS = half4(normalInputs.tangentWS, tangentSign);

                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 mainUV = input.uv * _uv;

                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                half4 detailTex = SAMPLE_TEXTURE2D(_dexTex, sampler_dexTex, mainUV * _uv2);

                half4 brightColor = BlendHardLight(mainTex, mainTex, _brightness);
                half4 blendedColor = BlendOverlay(brightColor, detailTex, _dexTexPow);

                half3 mainNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_MainNorm, sampler_MainNorm, mainUV));
                mainNormalTS = ApplyNormalPower(mainNormalTS, _MainNormPow);

                half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DexNorm, sampler_DexNorm, mainUV * _uv3));
                detailNormalTS = ApplyNormalPower(detailNormalTS, _DexNormPow);

                half3 combinedNormalTS = normalize(half3(
                    mainNormalTS.xy + detailNormalTS.xy,
                    mainNormalTS.z * detailNormalTS.z
                ));

                half3 normalWS = TangentToWorldNormal(
                    combinedNormalTS,
                    input.normalWS,
                    input.tangentWS
                );

                // Same logic as original:
                // _Color.a = 0 shows flat _Color.
                // _Color.a = 1 shows texture multiplied by _Color.
                half3 albedo = lerp(
                    _Color.rgb,
                    blendedColor.rgb * _Color.rgb,
                    _Color.a
                );

                half occlusion = lerp(1.0h, mainTex.a, _occlusionPow);

                // Original shader used "fixed sm = c;"
                // This effectively behaves like using the red channel.
                half smoothMask = mainTex.r;
                half smoothness = lerp(_Glossiness, _Glossiness1, smoothMask);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = SafeNormalize(GetCameraPositionWS() - input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogCoord;
                inputData.vertexLighting = VertexLighting(input.positionWS, normalWS);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1,1,1,1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = 0.0h;
                surfaceData.specular = half3(0,0,0);
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = combinedNormalTS;
                surfaceData.emission = half3(0,0,0);
                surfaceData.occlusion = occlusion;
                surfaceData.alpha = 1.0h;

                half4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
                finalColor.rgb = MixFog(finalColor.rgb, inputData.fogCoord);
                finalColor.a = 1.0h;

                return finalColor;
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }

    FallBack Off
}