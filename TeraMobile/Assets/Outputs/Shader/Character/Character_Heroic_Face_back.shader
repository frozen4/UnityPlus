Shader "Character/Character_Heroic_Face_Female" {
    Properties {
    //_MainTex ("", 2D) = "white" {}
    //_Color ("Skin Color Custom", Color) = (0,0.667,0.667,1)
        _BaseRGBA ("Base(RGBA)", 2D) = "grey" {}
        _SkinColor ("Skin Color Custom", Color) = (0,0.667,0.667,1)
        _Skinems ("Skin Ambient Bounce", Color) = (0,0,0,1)
        _SkinDirpow ("Skin Bounce", Range(0, 0.5)) = 0.3
        _Eyecolor ("Eye color Custom", Color) = (0,0.667,0.667,1)
        _Normalmap ("Normalmap", 2D) = "bump" {}
        _Normalscale ("Normalscale", Range(1,5)) = 1
        _SpecularmapRGBA ("Specularmap(RGBA)", 2D) = "white" {}
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1.5
        _MatMask ("Mat Mask(RGB)(R_Skin)(G_Eye)(B_null)(A_Transmision)", 2D) = "red" {}
        _SkinAmbientBounce ("Skin Ambient Bounce", Range(0, 0.5)) = 0
        _Brdfmap ("Brdfmap_soft", 2D) = "white" {}
        _Brdfmap1 ("Brdfmap_hard", 2D) = "white" {}
        _brdfrange("Brdfmap Ramping Range", Range(-1,1)) = 0
        _brdfrange1("Brdfmap Ramping Range", Range(-1,1)) = 0
        _rampmerge("Ramp Merging", Range(0,1)) = 0.5
//        _gloss ("testgloss", Range(0,1)) = 0
//        _lf ("HeadHighlight", Range(0,2)) = 1
        _headlight ("HeadLighting", Range(0.01,1)) = 0
        Notouch("Rim&DeathEffect", Range(0,1)) = 0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0, 5)) = 0

         _OriginOffset ("Origin Position Offset", Vector) = (0.05, -0.15, 0)
		//---//FakeSSSTest
        _TransmissionColor ("TransmissionColor ", Color) = (0.25,0,0,1)
//		_TransmissionRangeAdj ("TransmissionRangeAdj", Vector) = (0.75, -0.1, 1.5, 0.1)
		_TransmissionPointPower("TransmissionPointPower", Range(0, 1)) = 0.25
		_TransmissionRange("TransmissionRange", Range(-1, 1)) = 0
		//---//Light Dir Rim
        _LightDirRimColor ("Light Rim Color", Color) = (1,1,1,1)
        _LightDirRimRange ("Light Rim Range", Range(1, 10)) = 0
        _LightDirRimPower ("Light Rim Power", Range(0, 5)) = 0
		//---//FakeSpecularTest
        _FakeSpecPower ("Fake Spec Power", Range(0, 2)) = 0.15
        _lightheight("lightHeight", Range(-1,10)) = 0.5
        _normalblend("Blend With OriginNormal", Range(0,1)) = 0

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
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "TeraPBRLighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0

            uniform float4 _LightColor0;
            uniform sampler2D _BaseRGBA; uniform float4 _BaseRGBA_ST;
            uniform fixed _RimPower;
            uniform fixed4 _RimColor;
            uniform fixed4 _SkinColor;
            uniform fixed4 _Eyecolor;
            uniform fixed4 _Skinems;
            uniform fixed _SkinDirpow;
            uniform fixed _Normalscale;
            uniform float _brdfrange;
            uniform float _brdfrange1;
            uniform float _gloss;
            uniform float _lf;
            uniform half _headlight;
            uniform sampler2D _MatMask; uniform float4 _MatMask_ST;
            uniform sampler2D _Normalmap; uniform float4 _Normalmap_ST;
            uniform sampler2D _SpecularmapRGBA; uniform float4 _SpecularmapRGBA_ST;
            uniform sampler2D _Brdfmap; uniform float4 _Brdfmap_ST;
            uniform sampler2D _Brdfmap1; uniform float4 _Brdfmap1_ST;
            uniform fixed _SpecularIntensity;
            uniform fixed _SkinAmbientBounce;

            uniform half _lightheight;
            uniform fixed _rampmerge;

            //---//Test Variable
//			uniform float3 _OriginOffset;
			uniform fixed4 _TransmissionColor;
			uniform fixed4 _TransmissionRangeAdj;
			uniform fixed _TransmissionPointPower;
			uniform fixed _TransmissionRange;
			uniform fixed4  _LightDirRimColor;
			uniform fixed _LightDirRimRange;
			uniform fixed _LightDirRimPower;
			uniform fixed _normalblend;
			//---//

            float4 frag(V2f_TeraPBR i) : COLOR {
/////// Texture:
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                float4 _SpecularmapRGBA_var = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                float4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
// Geometrydata:
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 viewDir = normalize(float3(viewDirection.x,0,viewDirection.z));
                float3 normalLocal = UnpackNormal(tex2D(_Normalmap,TRANSFORM_TEX(i.uv0, _Normalmap)));
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 blurrednormal = lerp(i.originNormal,normalDirection,_normalblend);
                float3 viewReflectDirection = reflect(-viewDirection, normalDirection);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightDir = normalize(float3(_WorldSpaceLightPos0.x,0,_WorldSpaceLightPos0.z));
                float3 redirectedlight = normalize(float3(_WorldSpaceLightPos0.x,_lightheight,_WorldSpaceLightPos0.z));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
                float3 ahalfDirection = normalize(viewDirection-lightDirection);
////// Lighting:
//                float attenuation = LIGHT_ATTENUATION(i);
                float attenuation = SAMPLE_MCSHADOW(i);
                float3 sh = AmbientColorGradient(normalDirection);
                float3 attenColor = attenuation * lightColor;
///// Lightness:
                float lightpower = ((lightColor.r*0.299)+(lightColor.g*0.587)+(lightColor.b*0.114));
                float emispower = saturate(lightpower);
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float NdotLT = max(-_TransmissionRange, dot( normalDirection, lightDirection ));
                float _NdotL = max(0, dot( normalDirection, -lightDirection ));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
                float VdotH = max(0,dot(viewDirection,halfDirection));
                float Ndoth = max(0,dot(halfDirection,normalDirection));
                float Ndotah = max(0,dot(ahalfDirection,normalDirection));
                float NdotLD = max(0, dot( normalDirection, lightDir ));
                float NdotO = max(0, dot( blurrednormal, lightDirection));
                float HdotLD = max(0, dot( normalDirection, halfDirection ));
                float ndotL = max(0, dot( i.normalDir, lightDirection));
                float ndotv = max(0, dot( normalDirection, viewDir));
                float NdotR = max(0, dot( normalDirection, redirectedlight));
                float mrref = max(0,dot(reflect(-lightDirection,normalDirection),viewDirection));
                float NdotV = 1 - Ndotv;
                float Ndotl = 1 - NdotL;
                float3 _Rim = Rim(NdotV,_RimPower,_RimColor);
                float Transmitpower = _MatMask_var.b + NdotL;

//                float lightatten = saturate((NdotL + sqrt(NdotL))*0.5);
                float lightatten = saturate(NdotL*0.7 + sqrt(NdotL)*0.3);
                float viewatten = saturate((Ndotv + sqrt(Ndotv))*0.5);

             float atten1 = lightatten+_TransmissionRange + (viewatten*_TransmissionPointPower*Ndotl);
             float atten2 = NdotL+_TransmissionRange + (Ndotv*_TransmissionPointPower*Ndotl);
///////// Gloss:
                float gloss = _SpecularmapRGBA_var.a;
                fixed pie = 3.1415926535;
                fixed pie2 = pie/2;
///// SkinColor:
//                float3 _skincolor = Colorconverting(_BaseRGBA_var.rgb,_SkColor.rgb) * _MatMask_var.r;
//                float3 _skinspcolor = Colorconverting(_SpecularmapRGBA_var.rgb,_SkColor.rgb) * _MatMask_var.r;
                float3 _skincolor = ColorCstm(_BaseRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 _skinspcolor = ColorCstm(_SpecularmapRGBA_var.rgb.rgb,_SkinColor.rgb,_MatMask_var.r);
////// Specular:
                float3 specmncrm = dot(_SpecularmapRGBA_var.rgb,float3(0.3,0.59,0.11));
                float spGGX = SpecularGGX(pie, gloss, _MatMask_var.r, Ndoth, NdotV, NdotL);
                float3 specColor = Specularskin(gloss, Ndoth, _lf, spGGX, NdotL, attenColor, ndotv, _headlight, Ndotl, _skinspcolor, specmncrm) * lerp(1,0.85,NdotL);
                float3 directSpecular = attenColor * specColor;
                float3 specular = directSpecular * _SpecularIntensity;

/////// Diffuse:
//                float3 NdotLs = LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow);
//                float3 NdotLs = GradientLightingbyBRDFmap(_Brdfmap,NdotL,Ndotv,Ndotl,_brdfrange,_brdfrange1,0.5,_headlight,_SkinDirpow,_rampmerge);
                float3 softramp = LightingbyhsBRDFmap(_Brdfmap, NdotR, Ndotv, Ndotl, _brdfrange, 0.5, _headlight, _SkinDirpow);
                float3 hardramp = LightingbyhsBRDFmap(_Brdfmap1, NdotR, Ndotv, Ndotl, _brdfrange1, 0.5, _headlight, _SkinDirpow);
                float3 NdotLs = lerp(softramp,hardramp,max(0.01,_rampmerge));
//                float3 fsss = fastSSSbydepthmask(NdotL,Ndotl,1,NdotO,_TransmissionRange);
//                float fakesss = fastSSSbydepthmask(NdotL,Ndotl,1,NdotO,_TransmissionRange);
//                       fsss = fastSSSbydepthmask1(_MatMask_var, NdotL, Ndotl, NdotO, _TransmissionRange, sh, _TransmissionColor, NdotLD);
                half3 ssscolormerge = lerp(1,lerp(_TransmissionColor.rgb,dot(_TransmissionColor.rgb,float3(0.3,0.59,0.11)),0.5),_MatMask_var.b + NdotL);
//                      ssscolormerge = lerp(1,lerp(skintransmit,dot(_TransmissionColor.rgb,float3(0.3,0.59,0.11)),0.5),_MatMask_var.b + NdotL);
                      ssscolormerge = lerp(1,ssscolormerge,1) * _MatMask_var.r;
                      ssscolormerge = lerp(ssscolormerge,1,(1-(_MatMask_var.b + NdotL)));
//                      ssscolormerge = 1;
                      ssscolormerge = lerp(1,ssscolormerge,saturate(NdotLD+Ndotv));
                half3 ssscoloradd = _TransmissionColor.rgb*(_MatMask_var.b + NdotL);
//                      ssscoloradd = skintransmit*(_MatMask_var.b + NdotL);
                      ssscoloradd = ssscoloradd * 1 * _MatMask_var.r;
                      ssscoloradd = lerp(ssscoloradd,0,saturate(atten2)) * _MatMask_var.b;// * NdotLD;
                      ssscoloradd *= (1-HdotLD) * (NdotLD*0.3+0.7);
                      ssscoloradd = min(0.5,ssscoloradd);
                float3 sssmerge = lerp(1,1,1-(_MatMask_var.b + NdotL));
//                float3 fakesssmerge = SSSColorMerge(NdotLD,Ndotv,_TransmissionRange,_TransmissionColor.rgb,Transmitpower,1);
//                float3 fakesssadd = 
                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;
                float3 diffuse = CalculateDiffuse(NdotLs*ssscolormerge,0,scattering,attenColor,sh,_skincolor);
                float3 sssadd = lerp(1,0,saturate(atten2)) * _MatMask_var.b;
                       diffuse += ssscoloradd;
///// Eye Color:
                float3 _EyeColor = Colorconverting(_BaseRGBA_var.rgb,_Eyecolor.rgb);
                float3 eye = Eye(_EyeColor,_MatMask_var,emispower,NdotL,Ndotv,sh);
/// Final Color:
                float3 finalColor = diffuse + specular;
                       finalColor *= _MatMask_var.r;
                       finalColor += eye;
//                       finalColor = NdotR;
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
