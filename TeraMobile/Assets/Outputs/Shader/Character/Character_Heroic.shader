Shader "TERA/Character/Heroic" {
    Properties {
        _BaseRGBA ("Base(RGBA)", 2D) = "white" {}
        _SkinColor ("Skin Color Custom", Color) = (0,0.667,0.667,1)
        _Skinems ("Skin Ambient Bounce", Color) = (1,1,1,1)
        _SkinDirpow ("Skin Bounce", Range(0, 1)) = 0.3
        _se ("Skin Emission", Range(0,2)) = 0.3
        _Normalmap ("Normalmap", 2D) = "bump" {}
        _SpecularmapRGBA ("Specularmap(RGBA)", 2D) = "white" {}
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1
        _gloss("Gloss Correction",Range(0,5)) = 0
        _ReflectionIntensity ("Reflection Intensity", Range(0, 5)) = 1
        _lf ("lf", Range(0,2)) = 1
        _MatMask ("Mat Mask(RGB)(R_Skin)(G_Reflection)(B_Flake)", 2D) = "black" {}
        _Brdfmap ("Brdfmap", 2D) = "white" {}
        _brdfrange("Brdfmap Effective Range", Range(0,1)) = 0
        _skinpoint("Skin PointLight Intensity", Range(0,1)) = 1
        _OriginOffset ("Origin Position Offset", Vector) = (0, 0.125, 0)
        _TransmissionColor ("TransmissionColor ", Color) = (0.25,0,0,1)
		_TransmissionPointPower("TransmissionAddPower", Range(0, 1)) = 0.25
		_TransmissionRange("TransmissionRange", Range(-1, 1)) = 0
		_TransmissionRange1("TransmissionMergeRange", Range(0, 1)) = 0
        [Header(RimEffect)]
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0, 5)) = 0
        [Header(test)]
        _ao ("testAO", Range(0,1)) = 1
        _ab ("testAO dest", Range(0,0.5)) = 0.5
    }
    SubShader {
        
        Tags {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest-50"
            "DisableBatching"="True"
        }
        LOD 600
        Pass {
            Name "CHARACTERFORWARDPBR"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            CGPROGRAM
            #pragma vertex vert_tera_pbr
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
//            #pragma multi_compile _ UNIQUE_SHADOW UNIQUE_SHADOW_LIGHT_COOKIE
//	        #include "UniqueShadow_ShadowSample.cginc"
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "TeraPBRLighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0

            uniform float4 _LightColor0;
            uniform sampler2D _BaseRGBA;
			uniform float4 _BaseRGBA_TexelSize;
			uniform float4 _BaseRGBA_ST;
            uniform float _RimPower;
            uniform fixed4 _RimColor;
            uniform fixed4 _SkinColor;
            uniform fixed4 _Skinems;
            uniform fixed _lf;
            uniform fixed _se;
            uniform fixed _skinpoint;
            uniform sampler2D _MatMask; uniform float4 _MatMask_ST;
            uniform sampler2D _Normalmap; uniform float4 _Normalmap_ST;
            uniform sampler2D _SpecularmapRGBA; uniform float4 _SpecularmapRGBA_ST;
            uniform sampler2D _Brdfmap; uniform float4 _Brdfmap_ST;
            uniform float _SpecularIntensity;
            uniform float _SkinDirpow;
            uniform float _gloss;
            uniform float _brdfrange;
            uniform float _ReflectionIntensity;
            uniform fixed _ao;
            uniform fixed _ab;

            uniform fixed4 _TransmissionColor;
			uniform fixed4 _TransmissionRangeAdj;
			uniform fixed _TransmissionPointPower;
			uniform fixed _TransmissionRange;
			uniform fixed _TransmissionRange1;
            half4 frag(V2f_TeraPBR i) : COLOR {
// GeometryData:
				i.normalDir = normalize(i.normalDir);
				float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 _Normalmap_var = UnpackNormal(tex2D(_Normalmap, TRANSFORM_TEX(i.uv0, _Normalmap)));
				float3 normalLocal = _Normalmap_var.rgb;
				float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
				float3 viewReflectDirection = reflect(-viewDirection, normalDirection);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
                float3 charSunDirection = normalize(_SunDirchar1.xyz);
                float3 lightDir = normalize(float3(_WorldSpaceLightPos0.x,0,_WorldSpaceLightPos0.z));
                float3 halfDir = normalize(viewDirection+lightDir);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = UNITY_SHADOW_ATTENUATION(i, i.posWorld);
//                float attenuation = SAMPLE_MCSHADOW(i);
//                float attenuation = LIGHT_ATTENUATION(i);
////// Textures:
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                float4 _SpecularmapRGBA_var = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                half4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
                fixed equiprange = max(0,1-_MatMask_var.r);
                float3 specmncrm = dot(_SpecularmapRGBA_var.rgb,float3(0.3,0.59,0.11));
                

///////// Gloss:
                float glossq = min(1,((_SpecularmapRGBA_var.a+_gloss)/(1+_gloss))) * equiprange;
                float glossk = _SpecularmapRGBA_var.a * _MatMask_var.r;
				float gloss = saturate(glossq+glossk);
///// SkinColor:
                float3 _skincolor = ColorCstm(_BaseRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 _skinspcolor = ColorCstm(_SpecularmapRGBA_var.rgb.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 baseRGBA = _BaseRGBA_var.rgb * equiprange;
//                fixed basegrey = 0.2125*_BaseRGBA_var.r + 0.7154*_BaseRGBA_var.g + 0.0721*_BaseRGBA_var.b;
//                       baseRGBA = lerp(fixed3(basegrey,basegrey,basegrey),baseRGBA,2);
//                       baseRGBA *= equiprange;
///// Lightness:
                float lightpower = ((lightColor.r*0.299)+(lightColor.g*0.587)+(lightColor.b*0.114));
                float emispower = clamp(lightpower,0.0,1.0);
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float _NdotL = max(0, dot( normalDirection, -lightDirection ));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
                float NdotO = lerp(0.5,pow(NdotL,0.75),0.4) + NdotL*0.2;


                float Ndoth = max(0,dot(halfDirection,normalDirection));
                float Ndothd = dot(normalDirection,halfDir);
                float NdotV = max(0,1 - Ndotv);
//                float Vdoto = max(0, lerp(0.5,pow(Ndotv,0.75),0.5) + Ndotv*0.0);
                float Vdoto = max(0,lerp(0.5,pow(Ndotv,0.75),0.5));
                float Ndotl = max(0,1 - NdotL);
                float NdotSunchar = max(0,dot(charSunDirection,normalDirection)*NdotV);
                float3 attenColor = lerp(attenuation,1,_MatMask_var.r) * lightColor;
                       attenuation *= NdotL;
                float3 sh = AmbientColorGradient(normalDirection);
                       sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),NdotL);
                float3 IBL = ImageBasedLighting(gloss,viewReflectDirection);
                       IBL *= _ReflectionIntensity * sh * (attenuation+NdotV*gloss);
                half AO = AmbietnOcclusion(NdotL,Ndotv,_ao);
                fixed pie = 3.1415926535;
                float3 zis = Frsn(Ndotv,gloss,IBL,equiprange)*(attenuation*0.5+0.5);
                float3 _Rim = Rim(NdotV,_RimPower,_RimColor);
////// Specular:
                float spGGX = SpecularGGXWithHeadLight(pie, gloss, _MatMask_var.r, max(Ndothd,Ndoth), Ndotv, NdotV, NdotL,_NdotL);
                float3 sPc_skin = Specularskin(gloss, max(Ndothd,Ndoth), _lf, spGGX, NdotL, attenColor, Ndotv, _MatMask_var.r, Ndotl, _skinspcolor, specmncrm);
                float3 specular = CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGX,gloss,attenColor,IBL,equiprange,sPc_skin,0);
                       specular *= i.pl * _SpecularIntensity;//*(pow(NdotL*shadowatten,0.7)*0.75+0.25);

/////// Diffuse:
                float NdotLq = NdotL *attenuation * 0.8 * equiprange;
//                      NdotLq = NdotL *0.8* equiprange;
                       NdotL = max(-_brdfrange, NdotL);
                float3 NdotLs = LightingbyBRDFmap(_Brdfmap, NdotL, sqrt(Ndotv), Ndotl, _brdfrange, 0.5, 1, _SkinDirpow) * _MatMask_var.r;
//                       NdotLs *= lerp(NdotLs,1,NdotL+(Ndotv*max(0,1-NdotL)));
                float T = Transmission(Vdoto,_TransmissionRange);
                      T *= 1-NdotO;
                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;
                float3 sssadd = T*_MatMask_var.r*(1+NdotO) * _TransmissionColor.rgb * _TransmissionPointPower;
                       sssadd = T*_MatMask_var.r*(1+saturate(NdotO)) * _TransmissionColor.rgb * _TransmissionPointPower;
                       baseRGBA += _skincolor + (sssadd*0.5);
                float3 diffuse = CalculateDiffuseAddon(NdotLs,NdotLq,scattering,lightColor,sh,baseRGBA,NdotSunchar) * lerp(i.pl,1,_skinpoint);
                       diffuse = lerp(diffuse,_skincolor*(1+_se*NdotL),NdotL*_MatMask_var.r);
                       diffuse += sssadd*0.5;
//////// Emissive:
//                float3 emission = baseRGBA * _MatMask_var.a;
/// Final Color:
                float reflerp = reflp(_SpecularmapRGBA_var.rgb,pie);
                      reflerp *= equiprange;
                float3 finalColor = FinalColor(diffuse,specular,reflerp,_Rim,zis);
//                       finalColor = Transmission(Vdoto,_TransmissionRange);
//                       finalColor = Vdoto;
//                float curvature = length(fwidth(mul(unity_ObjectToWorld,float4(normalDirection,0)))) / length(fwidth(i.posWorld)) * 0.01;
//                       finalColor = curvature;


//                       finalColor = attenColor;//NdotL*UNITY_SHADOW_ATTENUATION(i, i.posWorld);
//                fixed finalgrey = 0.2125*_BaseRGBA_var.r + 0.7154*_BaseRGBA_var.g + 0.0721*_BaseRGBA_var.b;
//                      finalColor = lerp(fixed3(finalgrey,finalgrey,finalgrey),finalColor,1.5);
//                      finalColor = clamp(finalColor,0.04,1.2);
                half4 finalRGBA = half4(max(0.001,finalColor),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }

//        Pass



        UsePass "Hidden/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }

    SubShader {
        
        Tags {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest-50"
            "DisableBatching"="True"
             }
        LOD 400

        UsePass "Hidden/Character/CharacterPass/CHARACTERFORWARDGGX"
        UsePass "Hidden/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }

    SubShader {
        
        Tags {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest-50"
            "DisableBatching"="True"
             }
        LOD 200

        UsePass "Hidden/Character/CharacterPass/CHARACTERFORWARDBASE"
        UsePass "Hidden/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }
    //FallBack "Diffuse"
}
