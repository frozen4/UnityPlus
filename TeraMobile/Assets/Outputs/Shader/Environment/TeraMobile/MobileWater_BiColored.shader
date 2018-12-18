Shader "AQUAS/Mobile/Bicolored" {
    Properties {
        _DepthTex ("Depth Texture", 2D) = "white" {}
        [NoScaleOffset]_NormalTexture ("Normal Texture", 2D) = "white" {}
        _NormalTiling ("Normal Tiling", Float ) = 1
        _LightHeight ("Light Height", Range(0,1)) = 0.5
        _DeepWaterColor ("Deep Water Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _ShallowWaterColor ("Shallow Water Color", Color) = (0.4411765,0.9537525,1,1)
        _DepthTransparency ("Depth Transparency", Float ) = 0
        _ShoreFade ("Shore Fade", Float ) = 0.3
        _ShoreTransparency ("Shore Transparency", Float ) = 0.04
        _ShallowDeepBlend ("Shallow-Deep-Blend", Float ) = 3.6
        _Fade ("Shallow-Deep-Fade", Float ) = 3.1
        [HideInInspector]_ReflectionTex ("Reflection Tex", 2D) = "white" {}
        [MaterialToggle] _UseReflections ("Enable Reflections", Float ) = 0
        _Reflectionintensity ("Reflection intensity", Range(0, 1)) = 0.5
        _Distortion ("Distortion", Range(0, 2)) = 0.3
        _Specular ("Specular1", Float ) = 1
        _Specular1 ("Specular2", Float ) = 1
        _SpecularColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
        _Gloss ("Gloss", Float ) = 0.8
        _LightWrapping ("Light Wrapping", Float ) = 1.5
        _Refraction ("Refraction", Range(0, 5)) = 0.5
        _WaveSpeed ("Wave Speed", Float ) = 40
        _WaveSpeed2 ("Wave Speed2", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Ztest LEqual
            ColorMask RGB
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform fixed _LightHeight;
            uniform float4 _TimeEditor;
            uniform float4 _DeepWaterColor;
            uniform float4 _ShallowWaterColor;
            uniform float _ShallowDeepBlend;
            uniform float _Fade;
            uniform sampler2D _ReflectionTex; uniform float4 _ReflectionTex_ST;
            uniform float _Reflectionintensity;
            uniform fixed _UseReflections;
            uniform float _DepthTransparency;
            uniform float _Specular;
            uniform float _Specular1;
            uniform float _Gloss;
            uniform float _LightWrapping;
            uniform sampler2D _DepthTex; uniform float4 _DepthTex_ST;
            uniform sampler2D _NormalTexture; uniform float4 _NormalTexture_ST;
            uniform float _Refraction;
            uniform float _WaveSpeed;
            uniform float _WaveSpeed2;
            uniform float _Distortion;
            uniform float4 _SpecularColor;
            uniform float _NormalTiling;
            uniform float _ShoreFade;
            uniform float _ShoreTransparency;

            uniform fixed4 _ReflectionColor;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                float4 screenPos : TEXCOORD6;
                float4 projPos : TEXCOORD7;
                UNITY_FOG_COORDS(8)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
//                o.projPos = ComputeScreenPos (o.pos);
//                COMPUTE_EYEDEPTH(o.projPos.z);
                o.screenPos = o.pos;
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float _rotator_ang = 1.5708;
                float _rotator_spd = 1.0;
                float _rotator_cos = cos(_rotator_spd*_rotator_ang);
                float _rotator_sin = sin(_rotator_spd*_rotator_ang);
                float2 _rotator_piv = float2(0.5,0.5);
                float2 _rotator = (mul(i.uv0-_rotator_piv,float2x2( _rotator_cos, -_rotator_sin, _rotator_sin, _rotator_cos))+_rotator_piv);
                float2 _division1 = _NormalTiling;
                float4 _timer = _Time + _TimeEditor;
                float3 _multiplier3 = (float3((_WaveSpeed/_division1),0.0)*_timer.r);
                float2 _multiplier2 = ((_rotator+_multiplier3)*_division1);
                float3 _texture1 = UnpackNormal(tex2D(_NormalTexture,TRANSFORM_TEX(_multiplier2, _NormalTexture)));
                float2 _multiplier1 = ((i.uv0+(_multiplier3*_WaveSpeed2))*_division1);
                float3 _texture2 = UnpackNormal(tex2D(_NormalTexture,TRANSFORM_TEX(_multiplier1, _NormalTexture)));
                float3 _subtractor = (_texture1.rgb-_texture2.rgb);
                float _Refract = lerp(0.05,_Refraction*1.5,pow(max(0,dot(i.normalDir,viewDirection)),1.5));
                float3 normalLocal = lerp(float3(0,0,1),(_texture1+_texture2),_Refract);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals      
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightDir = normalize(float3(_WorldSpaceLightPos0.x,_LightHeight,_WorldSpaceLightPos0.z));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDir);
//                float3 halfDirection = normalize(viewDirection+lightDirection);
                float4 depth = tex2D(_DepthTex,i.uv1);
                float waterdepth = pow(saturate(depth.r*_DepthTransparency),_ShoreFade)*saturate(depth.r*_ShoreTransparency);
                      waterdepth = saturate(waterdepth);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float specPow = exp2( _Gloss * 3);
                float specp = exp2( _Gloss * 8+1 );
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
                float Ndoth = max(0,dot(halfDirection,normalDirection));
                float ref = max(0,dot(reflect(-lightDirection,normalDirection),viewDirection));
                float3 specularColor = _SpecularColor * _LightColor0.rgb;
                float3 spec1 = pow(Ndoth,specPow) * _Specular * pow((1-Ndotv),1.5);
                       spec1 = clamp(spec1,0,(spec1*0.5)) * UNITY_LIGHTMODEL_AMBIENT.rgb;
                float3 spec2 = pow(Ndoth,specp) * _Specular1 * 0.5;
                float3 directSpecular = spec1 + spec2;
                float3 specular = directSpecular;
/////// Diffuse:
                float3 w = float3(_LightWrapping,_LightWrapping,_LightWrapping)*0.5; // Light wrapping
                float3 NdotLWrap = NdotL * ( 1.0 - w );
                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
                float3 directDiffuse = forwardLight * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                       indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 _power = pow(saturate(max(_DeepWaterColor.rgb,(_ShallowWaterColor.rgb*(saturate(depth/_ShallowDeepBlend)*-1.0+1.0)))),_Fade);
                float2 _componentMask = _subtractor.rg;
                float2 _remap = ((i.screenPos.rg+(float2(_componentMask.r,_componentMask.g)*_Distortion))*0.5+0.5);
                float4 _ReflectionTex_var = tex2D(_ReflectionTex,TRANSFORM_TEX(_remap, _ReflectionTex)) * NdotL;
//                float frsnl = pow((1-Ndotv),2) * _Reflectionintensity;
                float frsnl = saturate(pow((1-Ndotv),2) * _Reflectionintensity * waterdepth);
                float3 reflectioncolor = lerp(_ReflectionColor.rgb,_ReflectionTex_var.rgb,_UseReflections);
                float3 diffuseColor = lerp(_power,reflectioncolor,frsnl);
                float3 diffuse = (directDiffuse + indirectDiffuse);
                       diffuse = lerp(diffuse,half3(1,1,1),frsnl) * diffuseColor;
                       diffuse = saturate(diffuse);
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,waterdepth);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
