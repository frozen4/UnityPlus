Shader "Character/Character_Heroic_Fashion_Transparent" {
    Properties {
        _BaseRGBA ("Base(RGBA)", 2D) = "white" {}
        _SkinColor ("Skin Color Custom", Color) = (0,0.667,0.667,1)
        _Skinems ("Skin Ambient Bounce", Color) = (1,1,1,1)
        _SkinDirpow ("Skin Bounce", Range(0, 0.5)) = 0.3
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
		_TransmissionPointPower("TransmissionPointPower", Range(0, 1)) = 0.25
		_TransmissionRange("TransmissionRange", Range(-1, 1)) = 0
        Notouch("Rim&DeathEffect", Range(0,1)) = 0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0, 5)) = 0
        _FlakeColor1 ("Flake Color 1", Color) = (1,1,1,1)
        _FlakeColor2 ("Flake Color 2", Color) = (1,1,1,1)
		// Addition
//		_AdditionTex("Addition Diffuse Tex", 2D) = "white" {}
//		_AdditionNormal("Addition Normal Tex", 2D) = "white" {}
//		_AdditionSpecular("Addition Specular Tex", 2D) = "white" {}
//		_AdditionOffset("Addition Tex Offset", Vector) = (0, 0, 1, 1)
    }
    SubShader {
        Tags {
            "IgnorProjector"="True"
            "Queue"="AlphaTest-50"
            "RenderType"="Transparent"
            "DisableBatching"="True"
        }
        LOD 600
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            ZWrite On
            ZTest LEqual
            Cull back
            Blend SrcAlpha OneMinusSrcAlpha
            
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
            uniform fixed _skinpoint;
            uniform fixed4 _FlakeColor1;
            uniform fixed4 _FlakeColor2;
            uniform sampler2D _MatMask; uniform float4 _MatMask_ST;
            uniform sampler2D _Normalmap; uniform float4 _Normalmap_ST;
            uniform sampler2D _SpecularmapRGBA; uniform float4 _SpecularmapRGBA_ST;
            uniform sampler2D _Brdfmap; uniform float4 _Brdfmap_ST;
            uniform float _SpecularIntensity;
            uniform float _SkinDirpow;
            uniform float _gloss;
            uniform float _brdfrange;
            uniform float _ReflectionIntensity;

            uniform fixed4 _TransmissionColor;
			uniform fixed4 _TransmissionRangeAdj;
			uniform fixed _TransmissionPointPower;
			uniform fixed _TransmissionRange;
//			uniform sampler2D _AdditionTex;
//			uniform sampler2D _AdditionNormal;
//			uniform sampler2D _AdditionSpecular;
//			uniform float4 _AdditionOffset;

            float4 frag(V2f_TeraPBR i) : COLOR {
////// Textures:
//#if defined(HERO_ADDITION_ON)
//				float2 additiontex_uv;
//				float4 additiontex_rect;
//
//				additiontex_rect.x = (_AdditionOffset.x / _BaseRGBA_TexelSize.z);
//				additiontex_rect.y = (_AdditionOffset.y / _BaseRGBA_TexelSize.w);
//				additiontex_rect.z = (_AdditionOffset.z / _BaseRGBA_TexelSize.z);
//				additiontex_rect.w = (_AdditionOffset.w / _BaseRGBA_TexelSize.w);
//
//				fixed2 additiontex_size;
//				additiontex_size.x = _AdditionOffset.z - _AdditionOffset.x;
//				additiontex_size.y = _AdditionOffset.w - _AdditionOffset.y;
//
//				additiontex_uv.x = (_BaseRGBA_TexelSize.z / additiontex_size.x) * (i.uv0.x - additiontex_rect.x);
//				additiontex_uv.y = (_BaseRGBA_TexelSize.w / additiontex_size.y) * (i.uv0.y - additiontex_rect.y);
//
//				fixed2 inrange;
//				inrange.x = i.uv0.x >= additiontex_rect.x && i.uv0.x <= additiontex_rect.z ? 1 : 0;
//				inrange.y = i.uv0.y >= additiontex_rect.y && i.uv0.y <= additiontex_rect.w ? 1 : 0;
//				inrange.x += inrange.y;
//#endif
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.05);
                half4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
                half3 _flake1 = lerp(fixed3(1.0,1.0,1.0),_FlakeColor1.rgb,_MatMask_var.b);
                half3 _flake2 = lerp(fixed3(1.0,1.0,1.0),_FlakeColor2.rgb,_MatMask_var.a);
                fixed equiprange = max(0,1-_MatMask_var.r);
                float4 _SpecularmapRGBA_var = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                       _SpecularmapRGBA_var.rgb *= _flake1 * _flake2;
                float3 specmncrm = dot(_SpecularmapRGBA_var.rgb,float3(0.3,0.59,0.11));
////#if defined(HERO_ADDITION_ON)
////				fixed4 combied_color = inrange.x > 1.5 ? tex2D(_AdditionTex, additiontex_uv) : fixed4(0, 0, 0, 0);
////				_BaseRGBA_var.rgb = combied_color.a * combied_color.rgb + (1 - combied_color.a) * _BaseRGBA_var.rgb;
////				fixed4 combied_spcolor = inrange.x > 1.5 ? tex2D(_AdditionSpecular, additiontex_uv) : fixed4(0, 0, 0, 0);
////				_SpecularmapRGBA_var.rgb = combied_color.a * combied_spcolor.rgb + (1 - combied_color.a) * _SpecularmapRGBA_var.rgb;
////#endif
                

// GeometryData:
				i.normalDir = normalize(i.normalDir);
				float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 _Normalmap_var = UnpackNormal(tex2D(_Normalmap, TRANSFORM_TEX(i.uv0, _Normalmap)));
				float3 normalLocal = _Normalmap_var.rgb;
////#if defined(HERO_ADDITION_ON)
////				float3 combied_normal = inrange.x > 1.5 ? UnpackNormal(tex2D(_AdditionNormal, additiontex_uv)) : float3(0, 0, 0);
////				normalLocal = combied_color.a * combied_normal.rgb + (1 - combied_color.a) * normalLocal.rgb;
////#endif
				float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
				float3 blurrednormal = lerp(normalDirection,i.originNormal,0.5);//*i.vcolor.a);
				float3 viewReflectDirection = reflect(-viewDirection, normalDirection);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
                float3 ahalfDirection = normalize(viewDirection-lightDirection);
                float3 lightDir = normalize(float3(_WorldSpaceLightPos0.x,0,_WorldSpaceLightPos0.z));
                float3 halfDir = normalize(viewDirection+lightDir);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = 1;
                float3 attenColor = lerp(attenuation,1,_MatMask_var.r) * lightColor;

///////// Gloss:
                float glossq = min(1,((_SpecularmapRGBA_var.a+_gloss)/(1+_gloss))) * equiprange;
                float glossk = _SpecularmapRGBA_var.a * _MatMask_var.r;
				float gloss = saturate(glossq+glossk);
///// SkinColor:
                float3 _skincolor = ColorCstm(_BaseRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 _skinspcolor = ColorCstm(_SpecularmapRGBA_var.rgb.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 baseRGBA = _BaseRGBA_var.rgb * equiprange;
                       baseRGBA *= _flake1 * _flake2;
                       baseRGBA += _skincolor;
///// Lightness:
                float lightpower = ((lightColor.r*0.299)+(lightColor.g*0.587)+(lightColor.b*0.114));
                float emispower = clamp(lightpower,0.0,1.0);
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float NdotLD = max(0, dot( normalDirection, lightDir ));
                float _NdotL = max(0, dot( normalDirection, -lightDirection ));
                float NdotO = max(0, dot( blurrednormal, lightDirection));
                float VdotO = max(0, dot( blurrednormal, viewDirection));
                float Vdoto = max(0, dot( i.originNormal, viewDirection));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
                float Ndoth = max(0,dot(halfDirection,normalDirection));
                float Ndotah = max(0,dot(ahalfDirection,normalDirection));
                float mrref = max(0,dot(reflect(-lightDirection,normalDirection),viewDirection));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float Ndothd = dot(normalDirection,halfDir);
                float NdotV = max(0,1 - Ndotv);
                float Ndotl = max(0,1 - NdotL);
                float _Ndotl = max(0,1 - _NdotL);
                float3 sh = AmbientColorGradient(normalDirection);
                       sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),NdotL);
                float3 IBL = ImageBasedLighting(gloss,viewReflectDirection);
                       IBL *= _ReflectionIntensity * sh;
                fixed pie = 3.1415926535;
                float3 zis = Frsn(Ndotv,gloss,IBL,equiprange);
                float3 _Rim = Rim(NdotV,_RimPower,_RimColor);
////// Specular:
                float spGGX = SpecularGGX(pie, gloss, _MatMask_var.r, max(Ndothd,Ndoth), NdotV, NdotL);
                float3 specular = CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGX,gloss,attenColor,IBL,equiprange,0,0);
                       specular *= i.pl * _SpecularIntensity;
/////// Diffuse:
                float NdotLq = NdotL * 0.8 * equiprange;
                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;
                float3 diffuse = CalculateDiffuse(0,NdotLq,scattering,attenColor,sh,baseRGBA) * lerp(i.pl,1,_skinpoint) * equiprange;
//////// Emissive:
                float3 emission = baseRGBA * _MatMask_var.a;
/// Final Color:
                float reflerp = reflp(_SpecularmapRGBA_var.rgb,pie);
                      reflerp *= equiprange;
                float3 finalColor = FinalColor(diffuse,specular,reflerp,_Rim,zis);
                half4 finalRGBA = half4(finalColor,_BaseRGBA_var.a+scatter);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        UsePass "Hide/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }

        SubShader {
        
        Tags {
            "IgnorProjector"="True"
            "Queue"="AlphaTest-50"
            "RenderType"="Transparent"
            "DisableBatching"="True"
        }
        LOD 400
        Pass {
            Name "CHARACTERFORWARDGGXFLAKETRANS"
            Tags {
                "LightMode"="ForwardBase"
            }
            
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
            uniform fixed _se;
            uniform fixed _skinpoint;
            uniform fixed4 _FlakeColor1;
            uniform fixed4 _FlakeColor2;
            uniform sampler2D _MatMask; uniform float4 _MatMask_ST;
            uniform sampler2D _Normalmap; uniform float4 _Normalmap_ST;
            uniform sampler2D _SpecularmapRGBA; uniform float4 _SpecularmapRGBA_ST;
            uniform sampler2D _Brdfmap; uniform float4 _Brdfmap_ST;
            uniform float _SpecularIntensity;
            uniform float _SkinDirpow;
            uniform float _gloss;
            uniform float _brdfrange;
            uniform float _ReflectionIntensity;

            uniform fixed4 _TransmissionColor;
			uniform fixed4 _TransmissionRangeAdj;
			uniform fixed _TransmissionPointPower;
			uniform fixed _TransmissionRange;

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
                float3 lightDir = normalize(float3(_WorldSpaceLightPos0.x,0,_WorldSpaceLightPos0.z));
                float3 halfDir = normalize(viewDirection+lightDir);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = 1;
////// Textures:
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                half4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
                half3 _flake1 = lerp(fixed3(1.0,1.0,1.0),_FlakeColor1.rgb,_MatMask_var.b);
                half3 _flake2 = lerp(fixed3(1.0,1.0,1.0),_FlakeColor2.rgb,_MatMask_var.a);
                float4 _SpecularmapRGBA_var = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                       _SpecularmapRGBA_var.rgb *= _flake1 * _flake2;
                fixed equiprange = max(0,1-_MatMask_var.r);
                float3 specmncrm = dot(_SpecularmapRGBA_var.rgb,float3(0.3,0.59,0.11));
                float3 attenColor = lerp(attenuation,1,_MatMask_var.r) * lightColor;
///////// Gloss:
                float glossq = min(1,((_SpecularmapRGBA_var.a+_gloss)/(1+_gloss))) * equiprange;
                float glossk = _SpecularmapRGBA_var.a * _MatMask_var.r;
				float gloss = saturate(glossq+glossk);
///// SkinColor:
                float3 _skincolor = ColorCstm(_BaseRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 _skinspcolor = ColorCstm(_SpecularmapRGBA_var.rgb.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 baseRGBA = _BaseRGBA_var.rgb * equiprange;
                       baseRGBA *= _flake1 * _flake2;
                       baseRGBA += _skincolor;
///// Lightness:
                float lightpower = ((lightColor.r*0.299)+(lightColor.g*0.587)+(lightColor.b*0.114));
                float emispower = clamp(lightpower,0.0,1.0);
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float _NdotL = max(0, dot( normalDirection, -lightDirection ));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
                float Ndoth = max(0,dot(halfDirection,normalDirection));
                float Ndothd = dot(normalDirection,halfDir);
                float NdotV = max(0,1 - Ndotv);
                float Ndotl = max(0,1 - NdotL);
                float _Ndotl = max(0,1 - _NdotL);
                float3 sh = AmbientColorGradient(normalDirection);
                       sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),NdotL);
                fixed pie = 3.1415926535;
                float3 _Rim = Rim(NdotV,_RimPower,_RimColor);
////// Specular:
                float spGGX = SpecularGGX(pie, gloss, _MatMask_var.r, max(Ndothd,Ndoth), NdotV, NdotL);
                float3 sPc_skin = Specularskin(gloss, max(Ndothd,Ndoth), _lf, spGGX, NdotL, attenColor, Ndotv, _MatMask_var.r, Ndotl, _skinspcolor, specmncrm);
                float3 specular = CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGX,gloss,attenColor,0,equiprange,sPc_skin,0);
                       specular *= _SpecularIntensity;
/////// Diffuse:
                float NdotLq = NdotL * 0.8 * equiprange;
                       NdotL = max(-_brdfrange, NdotL);
                float3 NdotLs = LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, 0.5, 1, _SkinDirpow) * _MatMask_var.r;

                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;
                float3 diffuse = CalculateDiffuse(NdotLs,NdotLq,scattering,attenColor,sh,baseRGBA) * lerp(i.pl,1,_skinpoint);
                       diffuse = lerp(diffuse,_skincolor*(1+_se*NdotL),NdotL*_MatMask_var.r);
//////// Emissive:
//                float3 emission = baseRGBA * _MatMask_var.a;
/// Final Color:
                float reflerp = reflp(_SpecularmapRGBA_var.rgb,pie);
                      reflerp *= equiprange;
                float3 finalColor = FinalColor(diffuse,specular,reflerp,_Rim,0);
                half4 finalRGBA = half4(finalColor,_BaseRGBA_var.a+scatter);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        UsePass "Hide/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }

    SubShader {
        
        Tags {
            "IgnorProjector"="True"
            "Queue"="AlphaTest-50"
            "RenderType"="Transparent"
            "DisableBatching"="True"
        }
        LOD 200

        UsePass "Hide/Character/CharacterPass/CHARACTERFORWARDBASEFLAKE"
        UsePass "Hide/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }
    FallBack "Diffuse"
}