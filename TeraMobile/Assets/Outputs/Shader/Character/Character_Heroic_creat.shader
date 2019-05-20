Shader "TERA/Character/Heroic_Create" {
    Properties {
        _BaseRGBA ("Base(RGBA)", 2D) = "white" {}
        _SkinColor ("Skin Color Custom", Color) = (0,0.667,0.667,1)
        _Skinems ("Skin Ambient Bounce", Color) = (1,1,1,1)
        _SkinDirpow ("Skin Bounce", Range(0, 0.5)) = 0.3
        _se ("Skin Emission", Range(0,2)) = 0.3
        _Skinedge ("Skin Edge Color", Color) = (1,1,1,1)
        _Normalmap ("Normalmap", 2D) = "bump" {}
        _SpecularmapRGBA ("Specularmap(RGBA)", 2D) = "white" {}
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1
        _gloss("Gloss Correction",Range(0,5)) = 0
        _ReflectionIntensity ("Reflection Intensity", Range(0, 5)) = 1
        _lf ("lf", Range(0,2)) = 1
        _MatMask ("Mat Mask(RGB)(R_Skin)(G_Reflection)(B_Flake)", 2D) = "black" {}
        _Brdfmap ("Brdfmap", 2D) = "white" {}
//        _brdfmod("Brdfmap Range", Range(0,1)) = 0.5
        _brdfrange("Brdfmap Effective Range", Range(0,1)) = 0
        _headlight ("Head Lighting Intensity", Range(0,1)) = 0.5
        _skinpoint("Skin PointLight Intensity", Range(0,1)) = 1
        [Header(RimEffect)]
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0, 5)) = 0
        [Header(Tester)]
        [MaterialToggle] _shadowmod("shadowmod",Range(0,1)) = 0
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
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert_tera_pbr
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
//            #include "UniqueShadow_ShadowSample.cginc"
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
            uniform float _brdfmod;
            uniform fixed _headlight;
            uniform fixed4 _Skinedge;
            uniform int _shadowmod;
            uniform fixed4 _SunColor;
            uniform float4 _SunDirchar;

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
                float3 charSunDirection = normalize(_SunDirchar.xyz);
                float3 halfsunchar = normalize(viewDirection+charSunDirection);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                //float attenuation = LIGHT_ATTENUATION(i);
//                float attenuation = TSHADOWATTEN(i);
//                float attenuation = lerp(TSHADOWATTEN(i),SHADOW_ATTENUATION(i),_shadowmod);
                float attenuation = 1;
                      attenuation = UNITY_SHADOW_ATTENUATION(i, i.posWorld);
////// Textures:
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                float4 _SpecularmapRGBA_var = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                half4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
                _headlight *= _MatMask_var.r;
                fixed equiprange = max(0,1-_MatMask_var.r);
                float3 specmncrm = dot(_SpecularmapRGBA_var.rgb,float3(0.3,0.59,0.11));
                
//                float3 attenColor = lerp(attenuation,1,_MatMask_var.r) * lightColor;
///////// Gloss:
                float glossq = min(1,((_SpecularmapRGBA_var.a+_gloss)/(1+_gloss))) * equiprange;
                float glossk = _SpecularmapRGBA_var.a * _MatMask_var.r;
				float gloss = saturate(glossq+glossk);
///// SkinColor:
                float3 _skincolor = ColorCstm(_BaseRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 _skinspcolor = ColorCstm(_SpecularmapRGBA_var.rgb.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 baseRGBA = _BaseRGBA_var.rgb * equiprange;
                       baseRGBA += _skincolor;
///// Lightness:
                float lightpower = ((lightColor.r*0.299)+(lightColor.g*0.587)+(lightColor.b*0.114));
                float emispower = clamp(lightpower,0.0,1.0);
                float NdotL = max(-_brdfrange, dot(normalDirection, lightDirection));
                float _NdotL = max(0, dot(normalDirection, -lightDirection));
                float Ndotv = max(0, dot(normalDirection, viewDirection));
                float Ndoth = max(0,dot(halfDirection,normalDirection));                
                float NdotV = max(0,1 - Ndotv);
                float Ndotl = max(0,1 - NdotL);
                float NdotSunchar = max(0,dot(charSunDirection,normalDirection));// * NdotV);
                float HdotSunchar = max(0,dot(halfsunchar,normalDirection));
                float3 attenColor = lerp(attenuation,fixed3(1,1,1),_MatMask_var.r)*lightColor;// * (attenuation*0.4+0.6);
                float3 sh = AmbientColorGradient(normalDirection);
                       sh = lerp(sh,dot(sh.rgb,float3(0.3,0.59,0.11)),saturate(NdotL + Ndotv));//* (attenuation*0.4+0.6);
                float3 IBL = ImageBasedLighting(gloss,viewReflectDirection);
                       IBL *= _ReflectionIntensity * sh;
                fixed pie = 3.1415926535;
                float3 zis = Frsn(Ndotv,gloss,IBL,equiprange);
////// Specular:
                float spGGX = SpecularGGX(pie, gloss, _MatMask_var.r, Ndoth, NdotV, NdotL);
                float spGGXadd1 = SpecularGGX(pie, gloss, 1, HdotSunchar, NdotV, NdotSunchar);
                float spGGXadd2 = SpecularGGX(pie, gloss, 1, NdotSunchar, NdotV, NdotSunchar)*equiprange;
                float3 sPc_skin = Specularskin(gloss, Ndoth, _lf, spGGX, NdotL, attenColor, Ndotv, _headlight, Ndotl, _skinspcolor, specmncrm);
                float3 specular = CalculateSpecularCreate(_SpecularmapRGBA_var.rgb,spGGX,gloss,attenColor,IBL,equiprange,sPc_skin,spGGXadd1);
                       specular *= i.pl * _SpecularIntensity;
/////// Diffuse:

                half nls = NdotL + _brdfrange + (Ndotv*_headlight*Ndotl);
                float3 NdotLs = LightingbyBRDFmap2(_Brdfmap, min(NdotL,1), Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow,_Skinedge.rgb);
                       NdotLs = lerp(LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow),NdotLs,_shadowmod*_MatMask_var.r);
                       NdotLs *= _MatMask_var.r;
//                float3 NdotLs = LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow) * _MatMask_var.r;
                float NdotLq = LightingWithHeadLight(NdotL, Ndotv, _headlight) * equiprange;
                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;
                half3 scat = lerp(fixed3(1,1,1),_Skinedge.rgb,exp2(-(Ndotv*Ndotv*8)));
//                      scat = lerp(NdotLs,scat,NdotL);
                       sh *= lerp(fixed3(1,1,1),(NdotLs+scat)*0.5,_shadowmod*_MatMask_var.r);
                float3 diffuse = CalculateDiffuseAddonCreat(NdotLs,NdotLq,scattering,attenColor,sh,baseRGBA,spGGXadd2);
                       diffuse = lerp(diffuse,_skincolor*(1+_se*NdotL),NdotL*_MatMask_var.r);
//                       diffuse = lerp(diffuse,lerp(_skincolor,pow(_skincolor,2),1-(NdotL + _brdfrange + (Ndotv*_headlight*Ndotl)))*(1+_se*NdotL),NdotL*_MatMask_var.r);
//                       diffuse = lerp(diffuse,_skincolor*(0.5+(_se+0.5)*NdotLs),NdotL*_MatMask_var.r);
//                       diffuse = lerp(diffuse,_skincolor*(1+_se)*NdotLs,NdotL*_MatMask_var.r);
                       diffuse *= lerp(i.pl,1,_skinpoint);
//////// Emissive:
//                float3 emission = baseRGBA * _MatMask_var.a;
/// Final Color:
                float reflerp = reflp(_SpecularmapRGBA_var.rgb,pie);
                      reflerp *= equiprange;
                float3 finalColor = FinalColor(diffuse,specular,reflerp,fixed3(0,0,0),zis);

//                       finalColor = exp2(-(Ndotv*Ndotv*10));
//                       finalColor = lerp(fixed3(1,1,1),_SpecularmapRGBA_var.rgb,exp2(-(Ndotv*Ndotv*8)));
//                       finalColor = NdotL;//*0.5+0.5;
//                       finalColor = scat*0.75+0.25+NdotLs;
//                       finalColor *= 0.5;
//                       finalColor *= scat*0.5+0.5;
//                       finalColor = NdotLs;
//                       finalColor *= _MatMask_var.r;
//                       finalColor = scat*_MatMask_var.r;
//                       finalColor = NdotLs;//saturate(sqrt(NdotL)*1.15);//dot(normalDirection,normalize(lightDirection-viewDirection));
//                       finalColor = _skincolor*(0.5+(_se+0.5)*NdotLs);
//                       finalColor = CalculateDiffuseAddonCreat(NdotLs,NdotLq,scattering,attenColor,sh,baseRGBA,spGGXadd2);
//                 half edge = exp2(-(Ndotv*Ndotv*8));
//                 half3 scat1 = lerp(fixed3(1,1,1),_Skinedge.rgb,edge);
//                   scat1 = lerp(fixed3(1,1,1),scat,saturate(sqrt(NdotL)*1.15));
//                         finalColor = NdotL + _brdfrange + (Ndotv*_headlight*Ndotl);
//                         finalColor = lerp(_skincolor,pow(_skincolor,2),1-finalColor);
//                       finalColor = LightingbyBRDFmap3(_Brdfmap, min(NdotL,1), Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow,_Skinedge.rgb);


                float4 finalRGBA = float4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }

        UsePass "Hidden/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }
    //FallBack "Diffuse"
}
