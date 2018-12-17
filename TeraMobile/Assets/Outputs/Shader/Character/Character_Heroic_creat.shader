Shader "Character/Character_Heroic_Creat" {
    Properties {
        _BaseRGBA ("Base(RGBA)", 2D) = "white" {}
        _SkinColor ("Skin Color Custom", Color) = (0,0.667,0.667,1)
        _Skinems ("Skin Ambient Bounce", Color) = (1,1,1,1)
        _SkinDirpow ("Skin Bounce", Range(0, 0.5)) = 0.3
        _ao ("Ambient Occlution", Range(0,2)) = 0
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
        Notouch("Rim&DeathEffect", Range(0,1)) = 0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0, 5)) = 0

        shadowmod("shadowmod",Range(0,1)) = 0
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
//            #pragma multi_compile_UNIQUE_SHADOW UNIQUE_SHADOW_LIGHT_COOKIE
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
            uniform fixed _ao;
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

            uniform int _shadowmod;
            //uniform float4 _SunDir;
            uniform fixed4 _SunColor;
            uniform float4 _SunDirchar;
            uniform fixed4 _SunColorchar;

            float4 frag(V2f_TeraPBR i) : COLOR {
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
                float3 ahalfDirection = normalize(viewDirection-lightDirection);
                float3 SunDirection = normalize(_SunDir.xyz);
                float3 charSunDirection = normalize(_SunDirchar.xyz);
                float3 halfsun = normalize(viewDirection+SunDirection);
                float3 halfsunchar = normalize(viewDirection+charSunDirection);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                //float attenuation = LIGHT_ATTENUATION(i);
//                float attenuation = TSHADOWATTEN(i);
//                float attenuation = lerp(TSHADOWATTEN(i),SHADOW_ATTENUATION(i),_shadowmod);
                float attenuation = 1;

                
////// Textures:
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                float4 _SpecularmapRGBA_var = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                half4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
                _headlight *= _MatMask_var.r;
                fixed equiprange = max(0,1-_MatMask_var.r);
                float3 specmncrm = dot(_SpecularmapRGBA_var.rgb,float3(0.3,0.59,0.11));
                
                float3 attenColor = lerp(attenuation,1,_MatMask_var.r) * lightColor;
///////// Gloss:
                float glossq = min(1,((_SpecularmapRGBA_var.a+_gloss)/(1+_gloss))) * equiprange;
                float glossk = _SpecularmapRGBA_var.a * _MatMask_var.r;
				float gloss = saturate(glossq+glossk);
///// SkinColor:
//                float3 _skincolor = Colorconverting(_BaseRGBA_var.rgb,_SkColor.rgb) * _MatMask_var.r;
//                float3 _skinspcolor = Colorconverting(_SpecularmapRGBA_var.rgb,_SkColor.rgb) * _MatMask_var.r;
                float3 _skincolor = ColorCstm(_BaseRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 _skinspcolor = ColorCstm(_SpecularmapRGBA_var.rgb.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 baseRGBA = _BaseRGBA_var.rgb * equiprange;
                       baseRGBA += _skincolor;
///// Lightness:
                float lightpower = ((lightColor.r*0.299)+(lightColor.g*0.587)+(lightColor.b*0.114));
                float emispower = clamp(lightpower,0.0,1.0);
                float NdotL = max(-_brdfrange, dot( normalDirection, lightDirection ));
                float _NdotL = max(0, dot( normalDirection, -lightDirection ));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
                float Ndoth = max(0,dot(halfDirection,normalDirection));
                float Ndotah = max(0,dot(ahalfDirection,normalDirection));
                float mrref = max(0,dot(reflect(-lightDirection,normalDirection),viewDirection));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float NdotSun = max(0,dot(SunDirection,normalDirection));
                float HdotSun = max(0,dot(halfsun,normalDirection));
                float NdotSunchar = max(0,dot(charSunDirection,normalDirection));
                float HdotSunchar = max(0,dot(halfsunchar,normalDirection));
                float NdotV = max(0,1 - Ndotv);
                float Ndotl = max(0,1 - NdotL);
                float _Ndotl = max(0,1 - _NdotL);
                float AO = AmbietnOcclusion(NdotL,Ndotv,_ao);

                float3 sh = AmbientColorGradient(normalDirection);
                       sh = lerp(sh,fixed3(1,1,1),NdotL);
                float3 IBL = ImageBasedLighting(gloss,viewReflectDirection);
                       IBL *= _ReflectionIntensity * sh;
                fixed pie = 3.1415926535;
                float3 zis = Frsn(Ndotv,gloss,IBL,equiprange);
                float3 _Rim = Rim(NdotV,_RimPower,_RimColor);



                float3 additionallightcolor = _SunColor.rgb * _SunColor.a * NdotSun;
                       additionallightcolor += _SunColorchar.rgb * _SunColorchar.a * NdotSunchar;
////// Specular:
                float spGGX = SpecularGGX(pie, gloss, _MatMask_var.r, Ndoth, NdotV, NdotL);
                float spGGXadd1 = SpecularGGX(pie, gloss, 1, HdotSun, NdotV, NdotL);
                float spGGXadd2 = SpecularGGX(pie, gloss, 1, HdotSunchar, NdotV, NdotL);
                float3 combinedlightcolor = lerp(1,additionallightcolor,saturate(NdotSun+NdotSunchar)) * attenColor;
                float3 sPc_skin = Specularskin(gloss, Ndoth, _lf, spGGX, NdotL, attenColor, Ndotv, _headlight, Ndotl, _skinspcolor, specmncrm);
                float3 specular = CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGX,gloss,attenColor,IBL,equiprange,sPc_skin,0);
//                       specular += CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGXadd1,gloss,_SunColor.rgb*_SunColor.a,IBL,equiprange,0,0);
//                       specular += CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGXadd2,gloss,_SunColorchar.rgb*_SunColorchar.a,IBL,equiprange,0,0);
                       specular *= i.pl * _SpecularIntensity;
//                       specular *= lerp(1,addtionallightcolor,saturate(NdotSun + NdotSunchar));
//                       specular *= lerp(1,_SpecularIntensity,equiprange);
/////// Diffuse:
                float3 NdotLs = LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow) * _MatMask_var.r;
                float NdotLq = LightingWithHeadLight(NdotL, Ndotv, _headlight) * equiprange;
//                      NdotLq += NdotSun * equiprange;
                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;
//                float3 combinedlightcolor = lerp(attenColor,_SunColor.rgb,NdotSun);
                float3 diffuse = CalculateDiffuse(NdotLs,NdotLq,scattering,attenColor,sh,baseRGBA);
                       diffuse += additionallightcolor * (NdotV + 0.0);
                       diffuse *= lerp(i.pl,1,_skinpoint);
//////// Emissive:
//                float3 emission = baseRGBA * _MatMask_var.a;
/// Final Color:
                float reflerp = reflp(_SpecularmapRGBA_var.rgb,pie);
                      reflerp *= equiprange;
                float3 finalColor = FinalColor(diffuse,specular,reflerp,_Rim,zis);

//                       finalColor = diffuse;
//                       finalColor = specular;
//                       finalColor = _BaseRGBA_var.rgb*1.3 + specular;
//                       finalColor = sh * 0.7;

                float4 finalRGBA = float4(finalColor,1);
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
