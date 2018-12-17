Shader "Character/Character_Heroic_Hair_test" {
    Properties {
        _BaseRGBA ("Base(RGBA)", 2D) = "white" {}
//        _boundary ("Light Boundary", Range(-1,1)) = 0
        _Normalmap ("Normalmap", 2D) = "bump" {}
        _combmap ("Combmap", 2D) = "bump" {}
        _shiftmap ("Tangentshiftmap", 2D) = "bump" {}
        _HairColorCustom ("Hair Color Custom", Color) = (1,1,1,1)
//        _shcolor ("Ambient Color", Color) = (1,1,1,1)
        contraction ("Scatter Contraction", Range(0,60)) = 20
        feedback ("Scatter Bias", Range(0,0.5)) = 0.05
        scattercolor ("Scatter Color", Color) = (0.5,0.5,0.5,1)
        _SpecularmapRGBA ("Specularmap(RGBA)", 2D) = "white" {}
        _SpecularInt ("Specular Intensity", Range(0, 2)) = 0.75
//        _ReflectionIntensity ("Reflection Intensity", Range(0, 0.5)) = 0
        _AnisoDir ("Aniso Light Direction", Vector) = (0, 1, 0, 0)
//        _AlphaX("Shift1", Range(0,1)) = 1
//        _AlphaY("Aniso gloss1", Range(0,5)) = 1
//        _AlphaZ("Detail Replacement", Range(0,5)) = 1
        _Glossiness ("Aniso Gloss lv1", Range(0,8)) = 0.5
        aspc1 ("Aniso Specular Intensity Lv1", Range(0,5)) = 1
        _AnisoOffset2 ("Aniso Offset Lv1", Range(-2,2)) = 0
        _Glossiness2 ("Aniso Gloss lv2", Range(0,8)) = 0.5
        aspc2 ("Aniso Specular Intensity Lv2", Range(0,5)) = 1
        _spread2 ("Aniso Specular Spread Lv2", Range(0,256)) = 1
        _AnisoOffset ("Aniso Offset Lv2", Range(-2,2)) = 0
        _MatMask ("Mat Mask(RGB)(R_Hair)(G_Reflection)(B_Accessory)", 2D) = "black" {}
        _headlight ("Head Lighting Intensity", Range(0,1)) = 0.5
//        Notouch("Rim&DeathEffect", Range(0,1)) = 0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0, 5)) = 0
    }
    SubShader {
        Tags {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest-50"
            "DisableBatching"="True"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert_tera_pbr
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
			#include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "TeraPBRLighting.cginc"

            uniform float4 _LightColor0;
            uniform sampler2D _BaseRGBA; uniform float4 _BaseRGBA_ST;
            uniform float _RimPower;
            uniform fixed4 _RimColor;
            uniform fixed4 _HairColorCustom;
            uniform fixed4 _shcolor;
            uniform sampler2D _MatMask; uniform float4 _MatMask_ST;
            uniform sampler2D _Normalmap; uniform float4 _Normalmap_ST;
            uniform sampler2D _combmap; uniform float4 _combmap_ST;
            uniform sampler2D _shiftmap; uniform float4 _shiftmap_ST;
            uniform sampler2D _SpecularmapRGBA; uniform float4 _SpecularmapRGBA_ST;
            uniform fixed _boundary;
            uniform float contraction;
            uniform float feedback;
            uniform fixed4 scattercolor;
            uniform fixed _headlight;
            uniform fixed _SpecularInt;
            uniform fixed _ReflectionIntensity;
            uniform float4 _AnisoDir;
            uniform float _AnisoOffset;
            uniform float _AnisoOffset2;
            uniform float _spread1;
            uniform float _spread2;
            uniform float aspc1;
            uniform float aspc2;
            uniform float _Glossiness;
            uniform float _Glossiness2;
            uniform float _AlphaX;
            uniform float _AlphaY;
            uniform float _AlphaZ;

            uniform float4 SunDir2;

            float4 frag(V2f_TeraPBR i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                i.tangentDir = normalize(i.tangentDir);
                float3 tangentDir = UnpackNormal(tex2D(_combmap,TRANSFORM_TEX(i.uv0, _combmap)));
                float3 tangentDir1 = tex2D(_combmap,TRANSFORM_TEX(i.uv0, _combmap));
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3x3 tangentTransform1 = float3x3( tangentDir, i.bitangentDir, i.normalDir);
//                float3 viewDir = normalize(i.posWorld.xyz - _WorldSpaceCameraPos.xyz);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normalmap_var = UnpackNormal(tex2D(_Normalmap,TRANSFORM_TEX(i.uv0, _Normalmap)));
                float3 normalLocal = _Normalmap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 tangentDirection = normalize(mul( tangentDir.rgb, tangentTransform ));
                float3 redirected = normalize(mul(normalLocal+tangentDir.rgb, tangentTransform ));
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 anisoDirection = normalize(tangentDir1);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                fixed plus1 = (1 + ((lightColor-1)*0.2));
                float lightmass = lightColor < 1 ? lightColor : plus1;
                float3 halfDirection = normalize(viewDirection+lightDirection);
                float3 halfaDir = normalize(viewDirection+anisoDirection);

                float3 halfanisoDir = normalize(anisoDirection+lightDirection);

                float3 binormalDirection = cross(normalDirection,tangentDirection);

                float3 viewanisoDirection = normalize(viewDirection + anisoDirection);

                float3 shifmap = UnpackNormal(tex2D(_shiftmap,TRANSFORM_TEX(i.uv0, _shiftmap)));
                float3 shiftedtangent = lerp(tangentDir, shifmap, _AlphaZ);
//                       shiftedtangent = lerp(normalDirection, shifmap, _AlphaZ);
                       tangentDirection = normalize(mul(shiftedtangent, tangentTransform ));
                       tangentDirection = normalize(mul(shifmap, tangentTransform ));
                       tangentDirection = normalize(mul(tangentDir, tangentTransform ));
                float3 tangentDirect = normalize(mul(shiftedtangent, tangentTransform ));
//                       shiftedtangent = tangentDir + shifmap * _AlphaX;
//                       tangentDirection = normalize(shiftedtangent);
//                       tangentDirection = normalize(mul(shiftedtangent, tangentTransform ));
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
                float3 sh = AmbientColorGradient(normalDirection);
                float3 lightpower = dot(lightColor,float3(0.3,0.59,0.11));
///////// Gloss:
                fixed4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                float4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
                float nohair = 1-_MatMask_var.r;
                float4 spcolor = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                float gloss = spcolor.a;
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float _NdotL = max(0, dot( normalDirection, -lightDirection ));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
                float NdotV = max(0,1 - Ndotv);

                float NdotT = max(0, dot( normalDirection, tangentDir1));

                float Ndoth = max(0,dot(halfDirection,normalDirection));

                float NdotA = max(-_boundary,dot(normalDirection,lightDirection) * _MatMask_var.r);
                float hdota = max(0,dot(normalDirection,viewanisoDirection));
                float Ndotl = max(0,1 - NdotL);
                float NdotB = max(0,dot(float3(0,-0.5,0),normalDirection));
                float Ndota = max(0,dot(float3(0,1,0),normalDirection));
                float plmncrm = max(0,dot(i.pl,float3(0.3,0.59,0.11)));
                float3 _haircolor = ColorCstm(_BaseRGBA_var.rgb,_HairColorCustom.rgb,_MatMask_var.r);
                float3 _hairspcolor = ColorCstm(spcolor.rgb,_HairColorCustom.rgb,_MatMask_var.r);
                float3 baseRGBA = _BaseRGBA_var.rgb * (1-_MatMask_var.r) + _haircolor;
                float3 specularColor = _hairspcolor * _MatMask_var.r;

                float spwad = SpecWardAniso(normalDirection, halfDirection, i.tangentDir, _AlphaX, _AlphaY, NdotL, Ndotv, Ndoth);

//                float3 directSpecular = SpecAniso(halfDirection, halfDirection, normalDirection, _AnisoOffset2, _Glossiness, 64, aspc1);
//                float3 directSpecular = SpecAniso(_AnisoDir, 0, normalDirection, _AnisoOffset2, _Glossiness, 8, aspc1);      
//                       directSpecular += SpecAniso(_AnisoDir, viewDirection, normalDirection, _AnisoOffset, _Glossiness2, 8, aspc2);
//                       directSpecular += SpecAniso(halfanisoDir, halfDirection, normalDirection, _AnisoOffset, _Glossiness2, 64, aspc2);
//                       directSpecular = SpecAniso(lightDirection, viewanisoDirection, normalDirection, _AnisoOffset, _Glossiness2, 64, aspc2);
//                       directSpecular = SpecAniso(tangentDir1, viewDirection, redirected, _AnisoOffset, _Glossiness2, 64, aspc2);
                float3 directSpecular = SpecAniso(lightDirection, viewDirection, redirected, _AnisoOffset, _Glossiness2, 64, aspc2);
                       directSpecular += SpecAniso(lightDirection, viewDirection, redirected, _AnisoOffset2, _Glossiness, 64, aspc1);

//                       directSpecular += SpecularGGX(3.1415926535897, gloss, 1, Ndoth, NdotV, NdotL) * nohair * spcolor.rgb;
//                       directSpecular *= 0;
//                       directSpecular = SpecWardAniso(normalDirection, halfDirection, i.tangentDir, _AlphaX, _AlphaY, NdotL, Ndotv, Ndoth);
//                       directSpecular = SpecAniso2(spwad, _AnisoOffset, _Glossiness, _spread2, aspc2);
//                       directSpecular = StrandSpec(tangentDirect,halfDirection,_Glossiness);
//                       directSpecular = lightDirection;
//                       directSpecular = dot(anisoDirection,normalDirection);
//                       directSpecular = pow(directSpecular,8);


                       directSpecular *= specularColor * attenColor * i.pl;// * (NdotA * 0.6 + 0.4);

                float3 specular = directSpecular * _SpecularInt;

//                float spaniso = max(0,sin(radians(Ndoth + _AnisoOffset2) * 90));
////                      spaniso = max(0,Ndoth);
//                      spaniso = pow(spaniso, _Glossiness * 128) * aspc1;
//                      spaniso = pow(spaniso, exp2(_Glossiness * 10+1)) * aspc1;


/////// Diffuse:
                float hairlighting = NdotL * nohair;
                      hairlighting += NdotA + _boundary * _MatMask_var.r;
                float3 directDiffuse = min(1,(hairlighting + (Ndotv*_headlight))) * saturate(attenColor) * lightmass;// * lightpower;
                       _haircolor *= _haircolor;
                float3 indirectDiffuse = _haircolor * _MatMask_var.r * sh;
                       indirectDiffuse += sh * nohair;
                float3 _rim = pow(1.0-max(0,dot(i.normalDir, viewDirection)),_RimPower)*_RimColor.rgb*_RimPower;
                       
                float3 diffuseColor = _rim + baseRGBA;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor * 0.75;
                       diffuse += Scattering(Ndotv, contraction, feedback,_MatMask_var.r) * _HairColorCustom.rgb;
/// Final Color:
                float3 finalColor = diffuse + specular;
                       finalColor = max(0,finalColor);
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _BaseRGBA; uniform float4 _BaseRGBA_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
