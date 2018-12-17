Shader "Character/Character_Heroic_Hair_Creat" {
    Properties {
        _BaseRGBA ("Base(RGBA)", 2D) = "white" {}
        _boundary ("Light Boundary", Range(-1,1)) = 0
        _Normalmap ("Normalmap", 2D) = "bump" {}
        _HairColorCustom ("Hair Color Custom", Color) = (1,1,1,1)
        _shcolor ("Ambient Color", Color) = (1,1,1,1)
        contraction ("Scatter Contraction", Range(0,60)) = 20
        feedback ("Scatter Bias", Range(0,0.5)) = 0.05
        scattercolor ("Scatter Color", Color) = (0.5,0.5,0.5,1)
        _SpecularmapRGBA ("Specularmap(RGBA)", 2D) = "white" {}
        _SpecularInt ("Specular Intensity", Range(0, 2)) = 0.75
        _ReflectionIntensity ("Reflection Intensity", Range(0, 0.5)) = 0
        _AnisoDir ("Aniso Light Direction", Vector) = (0, 1, 0, 0)
        _Glossiness ("Aniso Gloss lv1", Range(0,1)) = 0.5
        aspc1 ("Aniso Specular Intensity Lv1", Range(0,5)) = 1
        _AnisoOffset2 ("Aniso Offset Lv1", Range(-2,2)) = 0
        _Glossiness2 ("Aniso Gloss lv2", Range(0,1)) = 0.5
        aspc2 ("Aniso Specular Intensity Lv2", Range(0,5)) = 1
        _AnisoOffset ("Aniso Offset Lv2", Range(-2,2)) = 0
        _MatMask ("Mat Mask(RGB)(R_Hair)(G_Reflection)(B_Accessory)", 2D) = "black" {}
        _headlight ("Head Lighting Intensity", Range(0,1)) = 0.5
        Notouch("Rim&DeathEffect", Range(0,1)) = 0
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

            uniform float4 SunDir2;

            float4 frag(V2f_TeraPBR i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDir = normalize(i.posWorld.xyz - _WorldSpaceCameraPos.xyz);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normalmap_var = UnpackNormal(tex2D(_Normalmap,TRANSFORM_TEX(i.uv0, _Normalmap)));
                float3 normalLocal = _Normalmap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 anisoDirction = normalize(float3(0,0.25,0));
//                float3 anisoDirction = normalize(float3(0,_AnisoDir.y,0));
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                fixed plus1 = (1 + ((lightColor-1)*0.2));
                float lightmass = lightColor < 1 ? lightColor : plus1;
                float3 halfDirection = normalize(viewDirection+lightDirection);
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
                float Ndoth = max(0,dot(halfDirection,normalDirection));
                float NdotA = max(-_boundary,dot(normalDirection,anisoDirction) * _MatMask_var.r);
                float Ndotl = max(0,1 - NdotL);
                float plmncrm = max(0,dot(i.pl,float3(0.3,0.59,0.11)));
                float3 _haircolor = ColorCstm(_BaseRGBA_var.rgb,_HairColorCustom.rgb,_MatMask_var.r);
                float3 _hairspcolor = ColorCstm(spcolor.rgb,_HairColorCustom.rgb,_MatMask_var.r);
                float3 baseRGBA = _BaseRGBA_var.rgb * (1-_MatMask_var.r) + _haircolor;
                float3 specularColor = _hairspcolor * _MatMask_var.r;
                float3 directSpecular = SpecAniso(_AnisoDir, viewDirection, normalDirection, _AnisoOffset2, _Glossiness, 8, aspc1);
//                float3 directSpecular = SpecAniso(_AnisoDir, 0, normalDirection, _AnisoOffset2, _Glossiness, 8, aspc1);      
                       directSpecular += SpecAniso(_AnisoDir, viewDirection, normalDirection, _AnisoOffset, _Glossiness2, 8, aspc2);
                       directSpecular *= specularColor * attenColor * i.pl * (NdotA * 0.6 + 0.4);
                       directSpecular += SpecularGGX(3.1415926535897, gloss, 1, Ndoth, NdotV, NdotL) * nohair * spcolor.rgb;
                float3 specular = directSpecular * _SpecularInt;
/////// Diffuse:
                float hairlighting = NdotL* nohair;
                      hairlighting += NdotA + _boundary * _MatMask_var.r;
                float3 directDiffuse = min(1,(hairlighting + (Ndotv*_headlight))) * saturate(attenColor) * lightmass;// * lightpower;
                       _haircolor *= _haircolor;
                float3 indirectDiffuse = _haircolor * _MatMask_var.r * sh;
                       indirectDiffuse += sh * nohair;
                float3 _rim = pow(1.0-max(0,dot(i.normalDir, viewDirection)),_RimPower)*_RimColor.rgb*_RimPower;
                       
                float3 diffuseColor = _rim + baseRGBA;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor * 0.75;//0.5 * lightpower;
                       diffuse += Scattering(Ndotv, contraction, feedback,_MatMask_var.r) * _HairColorCustom.rgb;
/// Final Color:
                float3 finalColor = diffuse + specular;
                       finalColor = max(0,finalColor);

//                       finalColor = normalDirection;
//                       finalColor = SpecAniso(_AnisoDir, viewDirection, normalDirection, _AnisoOffset2, _Glossiness, 8, aspc1);
//                       finalColor += SpecAniso(_AnisoDir, viewDirection, normalDirection, _AnisoOffset, _Glossiness2, 8, aspc2);

                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        UsePass "Hide/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }
    FallBack "Diffuse"
}

