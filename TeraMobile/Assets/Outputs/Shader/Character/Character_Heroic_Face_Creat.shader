Shader "TERA/Character/Heroic_Face_Create" {
        Properties {
        _BaseRGBA ("Base(RGBA)", 2D) = "grey" {}
        _SkinColor ("Skin Color Custom", Color) = (0,0.667,0.667,1)
        _Skinems ("Skin Ambient Bounce", Color) = (0,0,0,1)
        _SkinDirpow ("Skin Bounce", Range(0, 0.5)) = 0.3
        _se ("Skin Emission", Range(0,2)) = 0.3
        _Skinedge ("Skin Edge Color", Color) = (1,1,1,1)
        _gamac ("Skin Gama Correction", Range(0.5,2)) = 1
        _Eyecolor ("Eye color Custom", Color) = (0,0.667,0.667,1)
        _Normalmap ("Normalmap", 2D) = "bump" {}
        _Normalscale ("Normalscale", Range(1,5)) = 1
        _SpecularmapRGBA ("Specularmap(RGBA)", 2D) = "white" {}
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1.5
        _MatMask ("Mat Mask(RGB)(R_Skin)(G_Eye)(B_null)(A_Transmision)", 2D) = "red" {}
        _SkinAmbientBounce ("Skin Ambient Bounce", Range(0, 0.5)) = 0
        _Brdfmap ("Brdfmap", 2D) = "white" {}
        _brdfrange("Brdfmap Effective Range", Range(0,1)) = 1
        _skinpoint("Skin PointLight Intensity", Range(0,1)) = 1
        _headlight ("HeadLighting", Range(0,1)) = 0
        [Header(RimEffect)]
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0, 5)) = 0
        [Header(Tester)]
        _gloss ("Gloss Corection", Range(-5,5)) = 0
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
            Zwrite On
            
            CGPROGRAM
            #pragma vertex vert_tera_pbr
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
//	        #include "3rd/UniqueShadows/Features/UniqueShadow/UniqueShadow_ShadowSample.cginc"
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
            uniform fixed _RimPower;
            uniform fixed4 _RimColor;
            uniform fixed4 _SkinColor;
            uniform fixed4 _Eyecolor;
            uniform fixed4 _Skinems;
            uniform fixed _SkinDirpow;
            uniform fixed _Normalscale;
            uniform fixed _skinpoint;
            uniform float _brdfrange;
            uniform float _gloss;
            uniform float _lf;
            uniform fixed _se;
            uniform half _headlight;
            uniform sampler2D _MatMask; uniform float4 _MatMask_ST;
            uniform sampler2D _Normalmap; uniform float4 _Normalmap_ST;
            uniform sampler2D _SpecularmapRGBA; uniform float4 _SpecularmapRGBA_ST;
            uniform sampler2D _Brdfmap; uniform float4 _Brdfmap_ST;
            uniform fixed _SpecularIntensity;
            uniform fixed _SkinAmbientBounce;
            uniform fixed _gamac;

            uniform float4 _SunDirchar;
            uniform int _shadowmod;
            uniform fixed4 _Skinedge;

            half4 frag(V2f_TeraPBR i) : COLOR {
/////// Texture:
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                float4 _SpecularmapRGBA_var = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                float4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
// Geometrydata:
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normalmap_var = UnpackNormal(tex2D(_Normalmap, TRANSFORM_TEX(i.uv0, _Normalmap)));
                float3 _Normalyeye = UnpackNormal(tex2D(_Normalmap,TRANSFORM_TEX(i.uv0, _Normalmap)));
                float3 normalLocal = _Normalmap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect(-viewDirection, normalDirection);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightDir = normalize(float3(_WorldSpaceLightPos0.x,0,_WorldSpaceLightPos0.z));
                float3 lightColor = lerp(_LightColor0.rgb,_RimColor.rgb,_RimPower);
                float3 halfDirection = normalize(viewDirection+lightDirection);
                float3 charSunDirection = normalize(_SunDirchar.xyz);
                float3 halfsunchar = normalize(viewDirection+charSunDirection);
////// Lighting:
//                float attenuation = LIGHT_ATTENUATION(i);
                float attenuation = 1;

///// Lightness:
                float lightpower = ((lightColor.r*0.299)+(lightColor.g*0.587)+(lightColor.b*0.114));
                float emispower = saturate(lightpower);
                float NdotL = max(-_brdfrange, dot(normalDirection, lightDirection));
                float NdotLD = max(0, dot( normalDirection, lightDir ));
                float _NdotL = max(0, dot( normalDirection, -lightDirection ));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
                float Ndoth = max(0,dot(halfDirection,normalDirection));
                float NdotSunchar = max(0,dot(charSunDirection,normalDirection));
                float HdotSunchar = max(0,dot(halfsunchar,normalDirection));
                float NdotV = max(0,1 - Ndotv);
                float Ndotl = max(0,1 - NdotL);
                float3 sh = AmbientColorGradient(normalDirection);
                       sh = lerp(sh,dot(sh.rgb,float3(0.3,0.59,0.11)),saturate(NdotL + Ndotv));
                float3 attenColor = lerp(lightColor,lightpower,_MatMask_var.r);// * lightColor;
///////// Gloss:
                float gloss = _SpecularmapRGBA_var.a;
                      gloss = saturate(((_SpecularmapRGBA_var.a+_gloss)/(1+_gloss)));
                fixed pie = 3.1415926535;
                fixed _lf = 1.0f;
//                fixed pie2 = pie/2;
///// SkinColor:
                float3 _skincolor = pow(ColorCstm(_BaseRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r),_gamac);
                float3 _skinspcolor = ColorCstm(_SpecularmapRGBA_var.rgb.rgb,_SkinColor.rgb,_MatMask_var.r);
////// Specular:
                float3 specmncrm = dot(_SpecularmapRGBA_var.rgb,float3(0.3,0.59,0.11));
                float spGGX = SpecularGGX(pie, gloss, _MatMask_var.r, Ndoth, NdotV, NdotL);
                float spGGXadd1 = SpecularGGX(pie, gloss, 1, HdotSunchar, NdotV, NdotSunchar);
                float spGGXadd2 = SpecularGGX(pie, gloss, 1, NdotSunchar, NdotV, NdotSunchar)*0;
                float3 specColor = Specularskin(gloss, Ndoth, _lf, spGGX, NdotL, attenColor, Ndotv, _headlight, Ndotl, _skinspcolor, specmncrm);

//                float _SP = pow(8192,gloss);
//                float d = (_SP + 2) / (8 * pie) * pow(Ndoth,_SP);
//                float f = _skinspcolor + (1 - _skinspcolor)*pow(2,-10 * max(0, dot( halfDirection, lightDir )));
//                float k = min(1, gloss + 0.545);
//                float v = 1 / (k*max(0, dot( viewDirection, halfDirection ))*max(0, dot( viewDirection, halfDirection ))) + (1 - k);
//
//                       specColor = d*f*v;


                float3 directSpecular = attenColor * specColor;
                float3 specular = directSpecular * _SpecularIntensity * i.pl;

/////// Diffuse:
                float3 NdotLs = LightingbyBRDFmap2(_Brdfmap, min(NdotL,1), Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow,_Skinedge.rgb);
                       NdotLs = lerp(LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow),NdotLs,_shadowmod*_MatMask_var.r);
//                float3 NdotLs = LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow);
                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;
                half3 scat = lerp(fixed3(1,1,1),_SpecularmapRGBA_var.rgb,exp2(-(Ndotv*Ndotv*8)));
                       sh *= lerp(fixed3(1,1,1),(NdotLs+scat)*0.5,_shadowmod*_MatMask_var.r);
                float3 diffuse = CalculateDiffuseAddonCreat(NdotLs,0,scattering,attenColor,sh,_skincolor,spGGXadd2);
                       diffuse = lerp(diffuse,_skincolor*(1+_se*NdotL),NdotL*_MatMask_var.r);
                       diffuse *= lerp(i.pl,1,_skinpoint);
///// Eye Color:
                float3 _EyeColor = ColorCstm(_BaseRGBA_var.rgb,_Eyecolor.rgb,_MatMask_var.g);
                float3 eye = Eye(_EyeColor,_MatMask_var,emispower,NdotLD,Ndotv,sh);
/// Final Color:
                float3 finalColor = diffuse + specular;
                       finalColor *= _MatMask_var.r;
                       finalColor += eye;
                       finalColor = max(0,finalColor);

//                       finalColor = NdotLs;//*lerp(fixed3(1,1,1),NdotLs,scattering-0.05);

//                       float curvature = length(fwidth(mul(unity_ObjectToWorld,float4(normalDirection,0)))) / length(fwidth(i.posWorld)) * 0.01;
//                       finalColor = curvature;//dot(curvature,normalDirection);

                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        
        UsePass "Hidden/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }
    //FallBack "Diffuse"
}

