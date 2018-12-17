Shader "Character/Character_Heroic3" {
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
        _headlight ("HeadLighting", Range(0,1)) = 0
        _OriginOffset ("Origin Position Offset", Vector) = (0, 0.125, 0)
        _TransmissionColor ("TransmissionColor ", Color) = (0.25,0,0,1)
//		_TransmissionRangeAdj ("TransmissionRangeAdj", Vector) = (0.75, -0.1, 1.5, 0.1)
		_TransmissionPointPower("TransmissionPointPower", Range(0, 1)) = 0.25
		_TransmissionRange("TransmissionRange", Range(-1, 1)) = 0
		_normalblend("Blend With OriginNormal", Range(0,1)) = 0
		_ssspower("SSSPower", Range(0,1)) = 0
		_sssmerge("SSSMerge", Range(0,1)) = 0
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
            uniform fixed4 _TransmissionColor;
			uniform fixed4 _TransmissionRangeAdj;
			uniform fixed _TransmissionPointPower;
			uniform fixed _TransmissionRange;
			uniform fixed _normalblend;
			uniform fixed _ssspower;
			uniform fixed _sssmerge;

            float4 frag(V2f_TeraPBR i) : COLOR {
// GeometryData:
				i.normalDir = normalize(i.normalDir);
				float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 _Normalmap_var = UnpackNormal(tex2D(_Normalmap, TRANSFORM_TEX(i.uv0, _Normalmap)));
				float3 normalLocal = _Normalmap_var.rgb;
				float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
				float3 blurrednormal = lerp(normalDirection,i.originNormal,_normalblend);//*i.vcolor.a);
				float3 viewReflectDirection = reflect(-viewDirection, normalDirection);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
                float3 ahalfDirection = normalize(viewDirection-lightDirection);
                float3 lightDir = normalize(float3(_WorldSpaceLightPos0.x,0,_WorldSpaceLightPos0.z));
                float3 halfDir = normalize(viewDirection+lightDir);
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
                float AO = AmbietnOcclusion(NdotL,Ndotv,_ao);
                float3 sh = AmbientColorGradient(normalDirection);
                       sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),NdotL);
                float3 IBL = ImageBasedLighting(gloss,viewReflectDirection);
                       IBL *= _ReflectionIntensity * sh;
                fixed pie = 3.1415926535;
                float3 zis = Frsn(Ndotv,gloss,IBL,equiprange);
                float3 _Rim = Rim(NdotV,_RimPower,_RimColor);

////// Specular:
                float spGGX = SpecularGGX(pie, gloss, _MatMask_var.r, max(Ndothd,Ndoth), NdotV, NdotL);
                float3 sPc_skin = Specularskin(gloss, max(Ndothd,Ndoth), _lf, spGGX, NdotL, attenColor, Ndotv, _headlight, Ndotl, _skinspcolor, specmncrm);
                float3 specular = CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGX,gloss,attenColor,IBL,equiprange,sPc_skin,0);
                       specular *= i.pl * _SpecularIntensity;
//                       specular *= lerp(1,_SpecularIntensity,equiprange);
/////// Diffuse:
                float NdotLq = LightingWithHeadLight(NdotL, Ndotv, _headlight) * equiprange;
                       NdotL = max(-_brdfrange, NdotL);
                //float3 NdotLs = LightingbyBRDFmap(_Brdfmap, NdotO, VdotO, max(0,1-NdotO), _brdfrange, 0.5, _headlight, 0.1) * _MatMask_var.r;
                float3 NdotLs = LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, 0.5, _headlight, 0.3) * _MatMask_var.r;
                       //NdotLs *= lerp(NdotLs,1,NdotO+(VdotO*max(0,1-NdotO)));
                
                float T = Transmission(VdotO,_TransmissionRange);
                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;// * (1-T);
                float3 sssadd = _MatMask_var.b * (T*_MatMask_var.r+NdotO) * _TransmissionColor.rgb;
                float3 diffuse = CalculateDiffuse(NdotLs,NdotLq,scattering,attenColor,sh,baseRGBA) * lerp(i.pl,1,_skinpoint);
                       diffuse = lerp(diffuse,_skincolor*(1+_SkinDirpow*NdotL),NdotL*_MatMask_var.r);
                       diffuse += sssadd;
//////// Emissive:
//                float3 emission = baseRGBA * _MatMask_var.a;
/// Final Color:
                float reflerp = reflp(_SpecularmapRGBA_var.rgb,pie);
                      reflerp *= equiprange;
                float3 finalColor = FinalColor(diffuse,specular,reflerp,_Rim,zis);
                       //finalColor = _skincolor*(1+_SkinDirpow*NdotL);
                       finalColor = NdotLs;

                half4 finalRGBA = half4(finalColor,1);
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