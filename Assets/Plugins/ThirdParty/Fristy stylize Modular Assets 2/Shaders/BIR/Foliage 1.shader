Shader "Fristy/Nature/Leaves 2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _InternalColor ("Internal Color / SSS Color", Color) = (1,1,1,1)

        _MainTex ("Albedo (RGB) Alpha (A)", 2D) = "white" {}

        _Cutout ("Distance Cutout", Range(0,2000)) = 10
        _sat ("Saturate", Range(0,4)) = 1
        _sss ("SSS Amount", Range(0,4)) = 0.5

        [Normal]_MainNormal ("Normal", 2D) = "bump" {}
        _MainNormalInt ("Normal Intensity", Range(-4,4)) = 1

        _Maps ("Maps / SSS Thickness", 2D) = "white" {}

        _windSpeed ("Wind Speed", Range(0,10)) = 1
        _WindDensity ("Wind Density", Range(0,10)) = 1
        _windStrengh ("Wind Strength", Range(0,10)) = 1

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _OcclusionPow ("Occlusion Power", Range(-1,0)) = 0
        _Metallic ("Metallic", Range(0,1)) = 0.0

        Vector1_8838B166 ("Offset", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
        }

        LOD 200
        Cull Off
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


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MainNormal);
            SAMPLER(sampler_MainNormal);

            TEXTURE2D(_Maps);
            SAMPLER(sampler_Maps);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _InternalColor;
                float4 _MainTex_ST;

                half _Cutout;
                half _sat;
                half _sss;

                half _MainNormalInt;

                half _windSpeed;
                half _WindDensity;
                half _windStrengh;

                half _Glossiness;
                half _OcclusionPow;
                half _Metallic;

                half Vector1_8838B166;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;

                float3 positionWS : TEXCOORD1;
                half3 normalWS    : TEXCOORD2;
                half4 tangentWS   : TEXCOORD3;
                half4 color       : COLOR;
                half fogCoord     : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half3 Saturation(half3 color, half saturation)
            {
                half luma = dot(color, half3(0.2126729h, 0.7151522h, 0.0721750h));
                return luma.xxx + saturation.xxx * (color - luma.xxx);
            }

            float2 GradientNoiseDir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;

                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float GradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);

                float d00 = dot(GradientNoiseDir(ip), fp);
                float d01 = dot(GradientNoiseDir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(GradientNoiseDir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(GradientNoiseDir(ip + float2(1, 1)), fp - float2(1, 1));

                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);

                return lerp(
                    lerp(d00, d01, fp.y),
                    lerp(d10, d11, fp.y),
                    fp.x
                );
            }

            float GradientNoise01(float2 uv, float scale)
            {
                return GradientNoise(uv * scale) + 0.5;
            }

            float3 ApplyWind(float3 positionOS, float4 vertexColor)
            {
                float3 worldPos = TransformObjectToWorld(positionOS);

                float2 windUV = worldPos.xz + (_Time.yy * _windSpeed);
                float noiseOut = GradientNoise01(windUV, _WindDensity);

                float noiseMinus = noiseOut - 0.2;
                float finalWind = noiseMinus * _windStrengh;

                // Red vertex color controls wind.
                positionOS.x += lerp(0.0, finalWind, vertexColor.r);

                return positionOS;
            }

            half3 ApplyNormalIntensity(half3 normalTS, half intensity)
            {
                normalTS.xy *= intensity;
                return normalize(normalTS);
            }

            half3 TangentToWorld(half3 normalTS, half3 normalWS, half4 tangentWS)
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

            half3 FakeSSS(
                half3 color,
                half3 normalWS,
                half3 viewDirWS,
                half thickness,
                half3 lightDirWS,
                half3 lightColor
            )
            {
                half3 vLTLight = lightDirWS + normalWS;
                half fLTDot = pow(saturate(dot(viewDirWS, -vLTLight)), 3.5h) * 1.5h;
                half3 fLT = (fLTDot + 1.2h) * thickness;

                return color * ((lightColor * fLT) * 0.4h);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 animatedPositionOS = ApplyWind(input.positionOS.xyz, input.color);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(animatedPositionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;

                output.normalWS = normalInputs.normalWS;

                half tangentSign = input.tangentOS.w * GetOddNegativeScale();
                output.tangentWS = half4(normalInputs.tangentWS, tangentSign);

                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Original Leaves 2 distance-based cutout.
                half cutoutDistance = max(_Cutout, 0.001h);
                half distToCamera = distance(input.positionWS, GetCameraPositionWS());
                half cameraCutout = saturate(distToCamera / cutoutDistance);

                clip(c.a - cameraCutout);

                half3 saturatedColor = Saturation(c.rgb, _sat);

                // Same logic as original:
                // _Color alpha blends between textured color and flat _Color.
                half3 albedo = lerp(
                    saturatedColor * _Color.rgb,
                    _Color.rgb,
                    _Color.a
                );

                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_MainNormal, sampler_MainNormal, input.uv));
                normalTS = ApplyNormalIntensity(normalTS, _MainNormalInt);

                half3 normalWS = TangentToWorld(normalTS, input.normalWS, input.tangentWS);

                // Fix backside lighting/color difference.
                half faceSign = IS_FRONT_VFACE(isFrontFace, 1.0h, -1.0h);
                normalWS = normalize(normalWS * faceSign);

                half3 viewDirWS = SafeNormalize(GetCameraPositionWS() - input.positionWS);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogCoord;
                inputData.vertexLighting = VertexLighting(input.positionWS, normalWS);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1,1,1,1);

                Light mainLight = GetMainLight(inputData.shadowCoord);

                half4 maps = SAMPLE_TEXTURE2D(_Maps, sampler_Maps, input.uv);

                half thickness = maps.r * _sss * saturate(input.color.r);

                half3 sssEmission = FakeSSS(
                    _InternalColor.rgb * c.rgb,
                    normalWS,
                    viewDirWS,
                    thickness,
                    mainLight.direction,
                    mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation
                );

                sssEmission *= c.rgb;

                half occlusion = saturate(1.0h + maps.g * _OcclusionPow);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0,0,0);
                surfaceData.smoothness = _Glossiness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = sssEmission;
                surfaceData.occlusion = occlusion;
                surfaceData.alpha = c.a;

                half4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
                finalColor.rgb = MixFog(finalColor.rgb, inputData.fogCoord);
                finalColor.a = c.a;

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
            Cull Off

            HLSLPROGRAM

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _InternalColor;
                float4 _MainTex_ST;

                half _Cutout;
                half _sat;
                half _sss;

                half _MainNormalInt;

                half _windSpeed;
                half _WindDensity;
                half _windStrengh;

                half _Glossiness;
                half _OcclusionPow;
                half _Metallic;

                half Vector1_8838B166;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 GradientNoiseDir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;

                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float GradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);

                float d00 = dot(GradientNoiseDir(ip), fp);
                float d01 = dot(GradientNoiseDir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(GradientNoiseDir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(GradientNoiseDir(ip + float2(1, 1)), fp - float2(1, 1));

                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);

                return lerp(
                    lerp(d00, d01, fp.y),
                    lerp(d10, d11, fp.y),
                    fp.x
                );
            }

            float GradientNoise01(float2 uv, float scale)
            {
                return GradientNoise(uv * scale) + 0.5;
            }

            float3 ApplyWind(float3 positionOS, float4 vertexColor)
            {
                float3 worldPos = TransformObjectToWorld(positionOS);

                float2 windUV = worldPos.xz + (_Time.yy * _windSpeed);
                float noiseOut = GradientNoise01(windUV, _WindDensity);

                float noiseMinus = noiseOut - 0.2;
                float finalWind = noiseMinus * _windStrengh;

                positionOS.x += lerp(0.0, finalWind, vertexColor.r);

                return positionOS;
            }

            float4 GetShadowPositionHClip(float3 positionOS)
            {
                float3 positionWS = TransformObjectToWorld(positionOS);
                float4 positionCS = TransformWorldToHClip(positionWS);

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 animatedPositionOS = ApplyWind(input.positionOS.xyz, input.color);

                output.positionWS = TransformObjectToWorld(animatedPositionOS);
                output.positionCS = GetShadowPositionHClip(animatedPositionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                half cutoutDistance = max(_Cutout, 0.001h);
                half distToCamera = distance(input.positionWS, GetCameraPositionWS());
                half cameraCutout = saturate(distToCamera / cutoutDistance);

                clip(c.a - cameraCutout);

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
            Cull Off

            HLSLPROGRAM

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _InternalColor;
                float4 _MainTex_ST;

                half _Cutout;
                half _sat;
                half _sss;

                half _MainNormalInt;

                half _windSpeed;
                half _WindDensity;
                half _windStrengh;

                half _Glossiness;
                half _OcclusionPow;
                half _Metallic;

                half Vector1_8838B166;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 GradientNoiseDir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;

                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float GradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);

                float d00 = dot(GradientNoiseDir(ip), fp);
                float d01 = dot(GradientNoiseDir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(GradientNoiseDir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(GradientNoiseDir(ip + float2(1, 1)), fp - float2(1, 1));

                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);

                return lerp(
                    lerp(d00, d01, fp.y),
                    lerp(d10, d11, fp.y),
                    fp.x
                );
            }

            float GradientNoise01(float2 uv, float scale)
            {
                return GradientNoise(uv * scale) + 0.5;
            }

            float3 ApplyWind(float3 positionOS, float4 vertexColor)
            {
                float3 worldPos = TransformObjectToWorld(positionOS);

                float2 windUV = worldPos.xz + (_Time.yy * _windSpeed);
                float noiseOut = GradientNoise01(windUV, _WindDensity);

                float noiseMinus = noiseOut - 0.2;
                float finalWind = noiseMinus * _windStrengh;

                positionOS.x += lerp(0.0, finalWind, vertexColor.r);

                return positionOS;
            }

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 animatedPositionOS = ApplyWind(input.positionOS.xyz, input.color);

                output.positionCS = TransformObjectToHClip(animatedPositionOS);
                output.positionWS = TransformObjectToWorld(animatedPositionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                half cutoutDistance = max(_Cutout, 0.001h);
                half distToCamera = distance(input.positionWS, GetCameraPositionWS());
                half cameraCutout = saturate(distToCamera / cutoutDistance);

                clip(c.a - cameraCutout);

                return 0;
            }

            ENDHLSL
        }
    }

    FallBack Off
}