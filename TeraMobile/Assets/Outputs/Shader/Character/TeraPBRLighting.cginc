#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityStandardUtils.cginc"
#include "MainCharacterShadowMap.cginc"

struct appdata_TeraSimple
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
struct V2f_TeraSimple
            {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                UNITY_FOG_COORDS(9)
            };
V2f_TeraSimple vert_tera_simple(appdata_TeraSimple v) 
            {
                V2f_TeraSimple o = (V2f_TeraSimple)0;
                o.uv0.xy = v.texcoord0;
                o.uv0.zw = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }

struct appdata_TeraDiffuse
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
struct V2f_TeraDiffuse
            {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
V2f_TeraDiffuse vert_diffuse(appdata_TeraDiffuse v) 
            {
                V2f_TeraDiffuse o = (V2f_TeraDiffuse)0;
                o.uv0.xy = v.texcoord0;
                o.uv0.zw = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }

			struct appdata_TeraPBRfx
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                fixed4 vcolor : COLOR0;
            };

// by zsw

////ShadowmapResamplering
////Shadowring
//#ifdef SHADOWS_SCREEN
//#if defined(UNITY_NO_SCREENSPACE_SHADOWS)
//// !TODO use another algorithm
////UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);
//float4 _ShadowMapTexture_TexelSize;
//half tSampleShadow(float4 coord)
//{
//    const float2 offset = float2(0.5,0.5);
//    float2 uv = (coord.xy * _ShadowMapTexture_TexelSize.zw) + offset;
//    float2 base_uv = (floor(uv) - offset) * _ShadowMapTexture_TexelSize.xy;
//    float2 st = frac(uv);
//
//    float2 uw = float2( 3-2*st.x, 1+2*st.x );
//    float2 u = float2( (2-st.x) / uw.x - 1, (st.x)/uw.y + 1 );
//    u *= _ShadowMapTexture_TexelSize.x;
//
//    float2 vw = float2( 3-2*st.y, 1+2*st.y );
//    float2 v = float2( (2-st.y) / vw.x - 1, (st.y)/vw.y + 1);
//    v *= _ShadowMapTexture_TexelSize.y;
//
//    half shadow;
//    half sum = 0;
//
//    sum += uw[0] * vw[0] * UNITY_SAMPLE_SHADOW(_ShadowMapTexture, float3(base_uv + float2(u[0], v[0]), coord.z));
//    sum += uw[1] * vw[0] * UNITY_SAMPLE_SHADOW(_ShadowMapTexture, float3(base_uv + float2(u[1], v[0]), coord.z));
//    sum += uw[0] * vw[1] * UNITY_SAMPLE_SHADOW(_ShadowMapTexture, float3(base_uv + float2(u[0], v[1]), coord.z));
//    sum += uw[1] * vw[1] * UNITY_SAMPLE_SHADOW(_ShadowMapTexture, float3(base_uv + float2(u[1], v[1]), coord.z));
//
//    shadow = sum / 16.0f;
//    shadow = lerp (_LightShadowData.r, 1.0f, shadow);
//
//    return shadow;
//}
//#else
//float4 _ShadowMapTexture_TexelSize;
//
//inline fixed tSampleShadow(unityShadowCoord4 coord)
//{
//#if AAA
//    return unitySampleShadow(coord);
//#else
//    float2 offset = _ShadowMapTexture_TexelSize.xy * 8;
//    half shadow = 0;
//    shadow += tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(coord));
//
//    shadow += tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(float4(coord.x + offset.x, coord.y + offset.y, coord.zw)));
//    shadow += tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(float4(coord.x + offset.x, coord.y - offset.y, coord.zw)));
//
//    shadow += tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(float4(coord.x - offset.x, coord.y - offset.y, coord.zw)));
//    shadow += tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(float4(coord.x - offset.x, coord.y + offset.y, coord.zw)));
//    
//    shadow = shadow / 5.0f;
//    shadow = lerp (_LightShadowData.r, 1.0f, shadow);
//    shadow = shadow * 0.8f + 0.2f;
//
//    return shadow;
//#endif
//}
//
//#endif
//#define TSHADOWATTEN(a) tSampleShadow(a._ShadowCoord)
//#else
//#define TSHADOWATTEN(a) (1)
//#endif



/// RGB2HSVConverting
float Epsilon = 1e-10;

float3 RGBtoHCV(in float3 RGB)
			{
				// Based on work by Sam Hocevar and Emil Persson
				float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
				float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
				float C = Q.x - min(Q.w, Q.y);
				float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
				return float3(H, C, Q.x);
			}

float3 RGBtoHSV(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float S = HCV.y / (HCV.z + Epsilon);
				return float3(HCV.x, S, HCV.z);
			}

float3 HUEtoRGB(in float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R, G, B));
			}

float3 HSVtoRGB(in float3 HSV)
			{
				float3 RGB = HUEtoRGB(HSV.x);
				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}

float3 Colorconverting(float3 texcolor, float3 color)
            {
            float3 texHSV = RGBtoHSV(texcolor);

            texHSV.x += color.r;
            texHSV.x = texHSV.x > 1 ? (texHSV.x - 1) : texHSV.x;
            texHSV.y *= color.g;
            texHSV.z *= color.b;

            return max(half3(0, 0, 0),HSVtoRGB(texHSV));
            }

////DisappearEffect
////Disappeareffect(_FactorTex,_FactorTex_ST,_DeathDuration,_SinceLevelLoadedTime,_DeathParamters,_DeathParamters,finalRGBA)
half4 Disappear(sampler2D _FactorTex, float4 _FactorTex_ST,float _DeathDuration, float _SinceLevelLoadedTime, fixed4 _DeathParamters, fixed4 _DeathColor, half4 RGBA, float2 factoruv)
              {
              float f = _DeathDuration == 0 ? 0 : clamp((_Time.y - _SinceLevelLoadedTime) / _DeathDuration, 0, 1.1);
		      float min = _DeathDuration == 0 ? 0 : clamp(f - _DeathParamters.x, 0, 1);
			  float max = _DeathDuration == 0 ? 0 : clamp(f + _DeathParamters.y, 0, 1);

		   	  fixed a = tex2D(_FactorTex, TRANSFORM_TEX(factoruv, _FactorTex)).r;

			  RGBA.a = a >= min ? 1 : 0;
			  fixed color_lerp = (a - min) / _DeathParamters.x + _DeathParamters.y;
			  fixed3 death_color = clamp(_DeathColor * exp(color_lerp - _DeathParamters.z) + _DeathParamters.w,0,1);
			  RGBA.rgb = a <= max ? death_color : RGBA.rgb;
              return RGBA;
              }

half3 displacementbynormalmap(float2 uv0, float2 uv1, float2 uv2,half _DispSpeedX, half _DispSpeedY, half _MainSpeedX, half _MainSpeedY, sampler2D _DispMap, half4 _TintColor, sampler2D _MaskTex, half _DispX, half _DispY, sampler2D _BaseTex)
              {
              half2 mapoft = half2(_Time.y*_DispSpeedX, _Time.y*_DispSpeedY);
			  half2 mapoft_main = half2(_Time.y*_MainSpeedX, _Time.y*_MainSpeedY);
  			  //get displacement color
			  half4 dispColor = tex2D(_DispMap, uv0 + mapoft);
			  //get uv oft
              half2 uvoft = uv2;
		      uvoft.x +=  dispColor.r  * _DispX * 1 + mapoft_main.x;
			  uvoft.y +=  dispColor.g  * _DispY * 1 + mapoft_main.y;
			  //apply displacement
			  fixed4 mainColor = tex2D(_BaseTex, uvoft);
			  //get mask;
			  fixed4 mask = tex2D(_MaskTex, uv1);
              half4 dispflowcolor = 2.0f * _TintColor * mainColor * mask.r;
              return max(float3(0,0,0), dispflowcolor.rgb*dispflowcolor.a);
              }
// by zsw


////ColorCustom
////ColorCstm(_BaseRGBA_var.rgb,_ColorCustom.rgb,_MatMask_var.r)
float3 ColorCstm(float3 basecolor, float3 Destcolor, fixed colorrange)
            {
            float3 cc = saturate( (Destcolor > 0.5 ? (1-(1-(Destcolor-0.5))*(1-basecolor)) : (2*Destcolor*basecolor)) );
            //by sun_jj of BH
			half3 detail = saturate(basecolor*1.75); 
			detail = lerp(detail, dot(detail, float3(0.3,0.59,0.11)),0.5);
			detail += ((1 - dot(abs(Destcolor-0.5)*2, float3(0.3,0.59,0.11)))*0.5); 
			detail = saturate(detail);
			return (cc * detail * colorrange);
			//by sun_jj of BH
            }
////Conversions
////gloss2roughness
////g2r(gloss)
float g2r(float gloss)
            {
            return max(0.001, 1-gloss);
            }
float g4r(float gloss)
            {
            return max(0.001, 1-pow(gloss,1.3));
            }
////a2(rough)
float a2(float roughness)
            {
            return (roughness * roughness * roughness * roughness);
            }
////pie
fixed pie2(fixed pie)
            {
            return (pie/2);
            }
//reflp(_SpecularmapRGBA_var.rgb,pie)
float reflp(float3 _SpecRGB, fixed p)
            {
            float3 specmncrm = dot(_SpecRGB,float3(0.3,0.59,0.11));
            fixed p2 = pie2(p);
            float refl = sin((specmncrm-1)*p2);
                  refl += 1;
            return max(0,refl);
            }
////AmbientColorGradient(normalDirection)
float3 AmbientColorGradient(float3 n)
            {
//            float3 Ambientcolor = ShadeSHPerPixel(n, half3(0, 0, 0));
            float3 Ambientcolor = ShadeSHPerPixel(n, half3(0, 0, 0), half3(0,0,0));
//                   Ambientcolor *= nv;
            return max(half3(0, 0, 0),Ambientcolor);
            }

////Scattering
////Scattering(Ndotv, 10, 0.05, _MatMask_var.r)
float Scattering(float Ndotv, fixed contraction, fixed feedback, float scatterrange)
             {
             float nv = Ndotv * Ndotv;
             half scattering = exp2(-(nv * contraction)) + feedback;
             return saturate(scattering * scatterrange);
             }
float lightwraping(float NdotL, float Ndotv)
             {
             float lightwrap = NdotL * pow(Ndotv, 1 + NdotL);
             return max(0,lightwrap);
             }
//by sun_jj of BH
float FakeSSS(float3 onormal, float3 normalDir, float3 light, float3 view, half sssmask, fixed4 Trasmission, fixed _TransmissionPower)
             {
             float3 blurredNormal = lerp(onormal, normalDir,0.25);
             half blurredNdotL = dot(blurredNormal,light);
             half blurredNdotV = dot(blurredNormal,view);
             half ndotl = dot( normalDir,light);
             float3 sss = saturate(((1-ndotl)*Trasmission.r + Trasmission.g)*(ndotl*Trasmission.b + Trasmission.a));
                    sss += saturate(((1-blurredNdotL)*Trasmission.r + Trasmission.g)*(blurredNdotL*Trasmission.b + Trasmission.a));
                    sss += (saturate(1-(blurredNdotV*2.5)) * saturate(1-blurredNdotL) * saturate(blurredNdotL*0.7+0.25) * saturate(sssmask*2.5+0.05));
             return saturate(sss * (1+sssmask) + (sssmask * _TransmissionPower));
             }
//by sun_jj of BH
////SubsurfaceScattering
float Transmission(float Vdotb, fixed trange)
             {
             float vb = max(0,1-Vdotb);
             return vb * (vb + trange);// * mask.r * mask.b;
             }
////Fresnl
////Frsn(Ndotv,gloss,IBL,equiprange)
float3 Frsn(float Ndotv, float gloss, float3 ibl, fixed equip)
             {
             float rough = g2r(gloss);
             float at = a2(rough);
             float frsnl = saturate(Ndotv + sqrt((Ndotv - Ndotv * at) * Ndotv * 3 + at));
             float3 fsnl = max(0,1-frsnl);
                    fsnl *= ibl * equip * gloss;
             return max(half3(0, 0, 0),fsnl);
             }

////RimEffect
////Rim(NdotV,_RimPower,_RimColor)
float3 Rim(float NdotV, float rimpower, float3 rimcolor)
             {
             float3 _Rim = pow(NdotV,rimpower) * rimcolor.rgb * saturate(rimpower);
             return max(half3(0, 0, 0),_Rim*0.7);
             }
float FrsnReflectance(float3 h, float3 v, float f0)
             {
             float base = 1 - dot(v,h);
             float exponetial = pow(base,5);
             return exponetial + f0 * (1-exponetial);
             }

///Lighting Functions
////SpecularGGX(pie, gloss, _MatMask.r, Ndoth, NdotV, NdotL)
float SpecularGGX(fixed pie, float gloss, fixed _MatMask, float Ndoth, float NdotV, float NdotL)
            {
            float hf = clamp((gloss-0.5)*2,0,1);
            fixed pi2 = pie2(pie);
            float hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            float roughness = g2r(gloss);
            float a = a2(roughness);
            float ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            float ndf2 = ((roughness*roughness) / (pie * pow((1 + (-0.65+pow(roughness,3))*(Ndoth)),2)) * gloss) * (sqrt(Ndoth)+NdotV);
            float ndf3 = ((roughness*roughness*roughness) / (pie * pow((1 + (-0.5+pow(roughness,3))*(Ndoth)),3))) * hf * 10;
            float ndf4 = sqrt(NdotL) * roughness * roughness * 0.8;
            float D = ((ndf2 + ndf3)*gloss) + (ndf1*hf2lf) + ndf4;
                  D = max(0,D);
            float G = (2 * Ndoth) / (Ndoth + sqrt(a + (1 - a)*Ndoth));
                  G = max(0,lerp(1,G,gloss));
            return max(0,D*G) ;
            }
float SpecularGGXWithHeadLight(fixed pie, float gloss, fixed _MatMask, float Ndoth, float Ndotv,float NdotV, float NdotL, float Ndotl)
            {
            float hf = clamp((gloss-0.5)*2,0,1);
            fixed pi2 = pie2(pie);
            float hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            float roughness = g2r(gloss);
            float a = a2(roughness);
            float ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            float ndf2 = ((roughness*roughness) / (pie * pow((1 + (-0.65+pow(roughness,3))*(Ndoth)),2)) * gloss) * (sqrt(Ndoth)+NdotV);
            float ndf3 = ((roughness*roughness*roughness) / (pie * pow((1 + (-0.5+pow(roughness,3))*(Ndoth)),3))) * hf * 10;
            float ndf4 = sqrt(NdotL) * roughness * roughness * 0.8;
            float nvf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndotv,2)),2));
            float nvf2 = sqrt(Ndotv) * roughness * 0.8;
            float D1 = ((ndf2 + ndf3)*gloss) + (ndf1*hf2lf) + ndf4;
                  D1 = max(0,D1);
            float D2 = max(0,(nvf1*hf2lf) + nvf2);
            float G = (2 * Ndoth) / (Ndoth + sqrt(a + (1 - a)*Ndoth));
                  G = max(0,lerp(1,G,gloss));
            return max(0,D1*G + D2*Ndotl);
            }
float SpecularGGXSimplefiedWithHeadLight(fixed pie, float gloss, fixed _MatMask, float Ndoth, float Ndotv, float NdotL, float ndotl)
            {
            fixed pi2 = pie2(pie);
            float hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            float roughness = g2r(gloss);
            float a = a2(roughness);
            float ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            float ndf4 = sqrt(NdotL) * roughness;
            float nvf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndotv,2)),2));
            float nvf2 = sqrt(Ndotv) * roughness * 0.8;
            float D = max(0,(ndf1*hf2lf) + ndf4);
            float D2 = max(0,(nvf1*hf2lf) + nvf2) * ndotl;
            return max(0,D + D2) ;
            }
float SpecularGGXSimplefied(fixed pie, float gloss, fixed _MatMask, float Ndoth, float Ndotv, float NdotL)
            {
            fixed pi2 = pie2(pie);
            float hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            float roughness = g2r(gloss);
            float a = a2(roughness);
            float ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            float ndf4 = sqrt(NdotL) * roughness;
            float D = (ndf1*hf2lf) + ndf4;
            return max(0,D) ;
            }
////Specularskin(gloss, Ndoth, _lf, spGGX, NdotL, attenColor, Ndotv, _headlight, Ndotl, _skinspcolor, specmncrm)
float3 Specularskin(float gloss, float Ndoth, float lf, float spGGX, float NdotL, float3 attenColor, float Ndotv, fixed _headhighlight, float Ndotl, float3 _skinspcolor, float3 specmncrm)
            {
            float sp1 = exp2( gloss * 9 + 1);
            float sp2 = exp2( gloss * 3 + 1);
            float3 spc1 = spGGX * NdotL * attenColor;
            float3 spc2 = pow(Ndotv,exp2( gloss * 9 + 1));
            float3 spc3 = _skinspcolor * lerp(specmncrm,1,0.5);
            return max(half3(0, 0, 0), (spc1+spc2)*spc3);
            }
////KSskinSpecular(gloss,Ndoth,_Beckmanmap,VdotH,NdotL,halfDirection,_skinspcolor)
float3 KSskinSpecular(float gloss, float Ndoth, sampler2D _Beckmanmap, float V, float NdotL, float3 halfDirection, float3 _skinspcolor)
            {
            float roughness = g2r(gloss);
            float beckmanmapping = tex2D(_Beckmanmap, float2(Ndoth,roughness)) * 2;
            float sp = pow(beckmanmapping,10);
            float frrefl = FrsnReflectance(halfDirection,V,0.028);
            float ndf4 = sqrt(NdotL) * roughness * roughness * 0.8;
            float frSpec = sp * frrefl / dot(halfDirection,halfDirection);
                  frSpec = max(half3(0, 0, 0),frSpec+ndf4);
            return frSpec;
            }

////AnisotropicSpecular
////SpecAniso(_AnisoDir, viewDirection, normalDirection, _AnisoOffset, _Glossiness, _spread, aspc)
float SpecAniso(float3 _AnisoDir, float3 viewDir, float3 normalDir, float _AnisoOffset, float _Glossiness, float _spread, float aspc)
             {
             float3 ahalfvec = normalize(_AnisoDir + viewDir);
             float HdotA = max(0,dot(normalDir, ahalfvec));
             float aniso = max(0,sin(radians((HdotA + _AnisoOffset) * 180)));
             float speca = saturate(pow(aniso, _Glossiness * 128)) * aspc;
             return max(0,speca);
             }
float SpecAniso2(float HdotA, float _AnisoOffset, float _Glossiness, float _spread, float aspc)
             {
             float aniso = max(0,sin(radians((HdotA + _AnisoOffset) * 180)));
             float speca = saturate(pow(aniso, _Glossiness * _spread)) * aspc;
             return max(0,speca);
             }
////WardAnisotopicSpecular
////SpecWardAniso(NdotL, Ndoth, Ndotv, _AlphaX, _AlphaY, halfDirection, tangentDir, binormalDirection)
float SpecWardAniso(float3 N,float3 H,float3 T,float shift1,float shift2,float3 NL, float3 NV,float3 NH)
             {
             float3 binormal = cross(N,T);
             float dotht = dot(H,T) / shift1;
             float dothb = dot(H,binormal) / shift2;
             float ward = sqrt(max(0, NL/NV)) * exp(-2*((dotht*dotht)+(dothb*dothb))/(1+NH));
             return max(0,ward);
             }

////StrandTangentSpecular
////StrandSpec(tangentDir,halfDirection,_Glossiness)
float StrandSpec(float3 T, float3 H, float exponet)
             {
             float TdotH = max(0,dot(T,H));
             float sinTH = sqrt(1 - TdotH*TdotH);
             float spatten = smoothstep(-1,0,TdotH);
             return spatten * pow((1-sinTH),exponet);
             }
////StrandSpecAniso(normalDirection,i.tangentDir,tangentDir1,halfDirection,128,_AnisoOffset,_AnisoOffset2);
float StrandSpecAniso(float3 N, float3 td, float3 shiftTex, float3 H, float exponet, half _AnisoOffset, half _AnisoOffset2)
             {
              half shift = shiftTex.g - 0.5;
              half3 T = -normalize(cross(N,td));
              half3 t1 = normalize(T + (_AnisoOffset + shift) * N);
              half3 t2 = normalize(T + (_AnisoOffset2 + shift) * N);

              float TdotH = dot( t1, H );
              float sinTH = sqrt(1 - TdotH*TdotH);
              float diratten = smoothstep(-1,0,TdotH);
              return max(0,diratten * pow(sinTH,exponet));
             }
////LightingWithHeadLight(NdotL, Ndotv, _headlight)
float LightingWithHeadLight(float NdotL, float Ndotv, fixed _headlight)
             {
             float headlighting = sqrt(Ndotv) * (1-NdotL) * _headlight;
                   headlighting = max(0,headlighting);
             float nl = (NdotL*0.75) + headlighting;
             return max(0,nl);
             }
////BRDFMapping
////LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, _brdfmod, _headlight, _SkinDirpow)
float3 LightingbyBRDFmap(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, float _brdfrange, float _brdfmod, fixed _headlight, fixed SkinDirpow)
             {
             float2 brdfuv = float2(0,0);
                    brdfuv.x = NdotL + _brdfrange + (Ndotv*_headlight*Ndotl);
//                    brdfuv.x *= (Ndotv*0.8 + 0.2);
                    brdfuv.y = _brdfmod;
//             return tex2D(_Brdfmap, brdfuv).rgb * (0.7 + (NdotL*(SkinDirpow)));
             return tex2D(_Brdfmap, brdfuv).rgb*(0.7+NdotL*0.1) + (NdotL*(SkinDirpow));
             }
float3 LightingbyBRDFmap2(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, float _brdfrange, float _brdfmod, fixed _headlight, fixed SkinDirpow)
             {
             float2 brdfuv = float2(0,0);
                    brdfuv.x = NdotL + _brdfrange + (Ndotv*_headlight*Ndotl);
//                    brdfuv.x *= (Ndotv*0.8 + 0.2);
                    brdfuv.y = _brdfmod;
//             return tex2D(_Brdfmap, brdfuv).rgb * (0.7 + (NdotL*(SkinDirpow)));
             return tex2D(_Brdfmap, brdfuv).rgb*(0.7+NdotL*0.1) + (NdotL*(SkinDirpow));
             }

float3 LightingbyhsBRDFmap(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, float _brdfrange, float _brdfmod, fixed _headlight, fixed SkinDirpow)
             {
             float2 brdfuv = float2(0,0);
                    brdfuv.x = NdotL + _brdfrange + (Ndotv*_headlight*(Ndotl*0.5));
                    brdfuv.y = _brdfmod;
             return tex2D(_Brdfmap, brdfuv).rgb * 0.7 + (NdotL*(SkinDirpow));
//             return tex2D(_Brdfmap, brdfuv).rgb * (0.7 + SkinDirpow);
             }
float3 LightingbytransBRDFmap(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, float _brdfrange,fixed _brdfrange1, float _brdfmod, fixed _headlight, fixed SkinDirpow, fixed merge, fixed lighttrans)
             {
             float lighttransmit = (NdotL+_brdfrange)*merge + (saturate(1 - (lighttrans - NdotL) * (1/lighttrans))+_brdfrange1)*(1-merge);
             float2 brdfuv = float2(0,0);
                    brdfuv.x = lighttransmit + (Ndotv*_headlight*(Ndotl*0.5));
                    brdfuv.y = _brdfmod;
             return tex2D(_Brdfmap, brdfuv).rgb * 0.7 + (NdotL*(0.3+SkinDirpow));
             }
float3 ViewDirLightingbyBRDFmap(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, float _brdfrange, float _brdfmod, fixed _headlight, fixed SkinDirpow)
             {
                    Ndotl = Ndotl * 0.3 + 0.7;
             fixed _SkinDirpow = SkinDirpow;
             float2 brdfuv = float2(0,0);
                    brdfuv.x = (Ndotv+_brdfrange) * Ndotl;
                    brdfuv.y = _brdfmod;
             return tex2D(_Brdfmap, brdfuv).rgb * (0.7 + _SkinDirpow);
             }
float3 binaryLightingbyBRDFmap(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, float _brdfrange, float _brdfmod, fixed _headlight, fixed SkinDirpow)
             {
//                    Ndotl = Ndotl * 0.3 + 0.7;
             fixed _SkinDirpow = SkinDirpow;
             float2 brdfuv = float2(0,0);
                    brdfuv.x = (NdotL+_brdfrange);
                    brdfuv.y = Ndotv;
             return tex2D(_Brdfmap, brdfuv).rgb * (0.7 + _SkinDirpow);
             }
float3 GradientLightingbyBRDFmap(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, fixed _brdfrange,fixed _brdfrange1, float _brdfmod, fixed _headlight, fixed SkinDirpow, fixed merge)
             {
//                   NdotL = NdotL+_brdfrange;
             float lightatten = saturate(((NdotL+_brdfrange) * merge) + sqrt(NdotL+_brdfrange1)*(1-merge));
             float viewatten = saturate((Ndotv * merge) + sqrt(Ndotv)*(1-merge));
             float2 brdfuv = float2(0,0);
                    brdfuv.x = lightatten + (viewatten*_headlight*Ndotl);
                    brdfuv.y = _brdfmod;
//             return tex2D(_Brdfmap, brdfuv).rgb * 0.7 + (lightatten*(0.3+SkinDirpow));
             return tex2D(_Brdfmap, brdfuv).rgb * (0.7 + lightatten*(0.3+SkinDirpow));
             }

////AmbientOcclusion
////AmbietnOcclusion(NdotL,Ndotv,_ao)
float AmbietnOcclusion(float NdotL, float Ndotv, fixed _ao)
            {
            float nv = pow(Ndotv,2);
            float ao = nv * _ao + (1-_ao);
                  ao += NdotL * _ao + (1-_ao);
            return saturate(ao);
            }
////Gathering
////CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGX,gloss,attenColor,IBL,equiprange,sPc_skin,0)
float3 CalculateSpecular(float3 spc, float spGGX, float gloss, float3 attenColor, float3 IBL, float equiprange, float3 sp_skin, float sph)
             {
             float3 spBase = spc.rgb * equiprange;
             float3 directSpecular = spGGX * (0.25+gloss*1.5);
                    directSpecular = max(0,directSpecular) * attenColor;
             float3 indirectSpecular = IBL * equiprange;
                    indirectSpecular *= max(0,1-directSpecular);
                    indirectSpecular = max(0,indirectSpecular);
             float3 spec = directSpecular + indirectSpecular + sph;
                    spec *= spBase;
             return max(half3(0, 0, 0),spec+sp_skin);
             }

////CalculateDiffuse(NdotLs,NdotLq,scattering,attenColor,sh,baseRGBA)
float3 CalculateDiffuse(float3 NdotLs, float NdotLq, float3 scattering, float3 attenColor, float3 sh, float3 base)
             {
             float3 ndf = NdotLs + NdotLq;
             float3 directDiffuse = ndf * attenColor;
             float3 indirectDiffuse = sh;
                    indirectDiffuse *= 0.7;
                    indirectDiffuse += scattering;
             float3 dif = directDiffuse + indirectDiffuse;// + i.pl*0;
             return max(half3(0, 0, 0),dif*base);
             }
////Eye(_Eyecolor.rgb,_MatMask_var,emispower,NdotL,Ndotv)
float3 Eye(float3 basergb, fixed4 _MatMask_var, fixed emispower, float NdotL, float Ndotv , float3 sh)
             {
             fixed eyerange = saturate(_MatMask_var.g*10);
             float3 _EyeColorcstm = basergb * eyerange;
             fixed3 _eyebase = (NdotL*0.7+0.3) * emispower;
                    _eyebase += dot(sh,float3(0.3,0.59,0.11))*0.8;
                    _eyebase *=  _EyeColorcstm;
             float3 eyecolor = _eyebase + (1 - eyerange -_MatMask_var.r) * basergb;
             float3 eyerim = pow((1-Ndotv),5.0) * eyerange * 0.2;
             return max(half3(0, 0, 0),eyecolor+eyerim) * eyerange;
             }
float3 Eyenpc(float3 basergb, fixed4 _MatMask_var, fixed emispower, float NdotL, float Ndotv, float3 sh)
             {
             fixed eyerange = _MatMask_var.b < 0.5 ? _MatMask_var.b : 0;
                   eyerange = saturate(eyerange*3);
             float3 _EyeColorcstm = basergb * eyerange;
             half3 eyespec = pow(Ndotv,512) * eyerange * 0.4;
             fixed3 _eyebase = (NdotL*0.7+0.3) * emispower;
                    _eyebase += dot(sh,float3(0.3,0.59,0.11))*0.8;
                    _eyebase *= _EyeColorcstm;
             float3 eyecolor = _eyebase + (1 - eyerange -_MatMask_var.r) * basergb;
             float3 eyerim = pow((1-Ndotv),5.0) * eyerange * 0.2;
             return max(half3(0, 0, 0),eyecolor+eyerim) * eyerange;
             }
////FinalColor
////FinalColor(diffuse,specular,reflerp,_Rim,zis)
float3 FinalColor(float3 diffuse, float3 specular, float reflerp, float3 rim, float3 frsn)
              {
              float3 c = diffuse + specular;
                     c = lerp(c,specular,reflerp) + rim + frsn;
              return max(half3(0, 0, 0),c);
              }

uniform half4 _OriginOffset;

uniform samplerCUBE _EnvMap;
uniform fixed _Refint;
uniform float4 _SunDir;

struct appdata_TeraPBR
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                fixed4 vcolor : COLOR0;
            };
struct V2f_TeraPBR
            {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                float3 originNormal : TEXCOORD8;
                fixed4 vcolor : COLOR0;
                fixed3 pl : COLOR1;
                MCSHADOW_TC(6)
                //SHADOW_COORDS(6)
                UNITY_FOG_COORDS(9)
            };
V2f_TeraPBR vert_tera_pbr(appdata_TeraPBR v) 
            {
                V2f_TeraPBR o = (V2f_TeraPBR)0;
                o.uv0.xy = v.texcoord0;
                o.uv0.zw = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                o.vcolor = v.vcolor;
                o.pl = Shade4PointLights (
		             unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		             unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		             unity_4LightAtten0, o.posWorld, o.normalDir);
		        o.pl = ShadeSHPerVertex(o.normalDir,o.pl);
		        o.pl = lerp(1,o.pl,max(0,dot(o.pl,float3(0.3,0.59,0.11))));
		        float3 shadowlightDir = normalize(_SunDir.xyz);
//		        _OriginOffset.y = lerp(1,_OriginOffset.y,o.vcolor.a);
		        float3 objectOrigin = mul(unity_ObjectToWorld, float4(0,0,0,1));
		        o.originNormal = normalize(o.posWorld.xyz  - objectOrigin.xyz + _OriginOffset.xyz);
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_MCSHADOW_TC(o, o.normalDir, shadowlightDir);
                return o;
            }

uniform sampler2D _MainTex;
float4 _MainTex_ST;
uniform sampler2D _DispMap;
float4 _DispMap_ST;
uniform sampler2D _MaskTex;
float4 _MaskTex_ST;

struct V2f_TeraPBRfx
            {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                float3 originNormal : TEXCOORD8;
                float4 uv3 : TEXCOORD7;
                fixed4 vcolor : COLOR0;
                fixed3 pl : COLOR1;
                MCSHADOW_TC(6)
                //SHADOW_COORDS(6)
                UNITY_FOG_COORDS(9)
            };
V2f_TeraPBRfx vert_tera_pbrfx(appdata_TeraPBRfx v) 
            {
                V2f_TeraPBRfx o = (V2f_TeraPBRfx)0;
                o.uv0.xy = v.texcoord0;
                o.uv0.zw = v.texcoord0;
                o.uv1.xy = v.texcoord1;
                o.uv1.zw = TRANSFORM_TEX(v.texcoord0, _MaskTex);
                o.uv3.xy = TRANSFORM_TEX(v.texcoord0, _MainTex);
                o.uv3.zw = TRANSFORM_TEX(v.texcoord0, _DispMap);
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                o.vcolor = v.vcolor;
                o.pl = Shade4PointLights (
		             unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		             unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		             unity_4LightAtten0, o.posWorld, o.normalDir);
		        o.pl = ShadeSHPerVertex(o.normalDir,o.pl);
		        o.pl = lerp(1,o.pl,max(0,dot(o.pl,float3(0.3,0.59,0.11))));
		        float3 shadowlightDir = normalize(_SunDir.xyz);
//		        _OriginOffset.y = lerp(1,_OriginOffset.y,o.vcolor.a);
		        float3 objectOrigin = mul(unity_ObjectToWorld, float4(0,0,0,1));
		        o.originNormal = normalize(o.posWorld.xyz  - objectOrigin.xyz + _OriginOffset.xyz);
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_MCSHADOW_TC(o, o.normalDir, shadowlightDir);
                return o;
            }

////ImageBasedLighting
////ImageBasedLighting(gloss,viewReflectDirection)
float3 ImageBasedLighting(float gloss, float3 viewDir)
            {
            float refpow = max(0,exp(gloss*5-3)*0.14);
            float ref = lerp(10,3,refpow);
            float3 IBL = texCUBElod(_EnvMap, half4(viewDir,ref));
            return lerp(fixed3(0.25,0.25,0.25),IBL,_Refint);
            }

half DisneyDiffuse(half nv, half nl, half lh, half roughness)
            {
	        half fd90 = 0.5 + 2 * lh * lh * roughness;
	        // Two schlick fresnel term
	        half lightScatter = (1 + (fd90 - 1) * pow((1 - nl),5));
	        half viewScatter = (1 + (fd90 - 1) * pow((1 - nv),5));

	        return lightScatter * viewScatter;
            }

////HairGGXTerm(Ndoth,gloss)
float HairGGXTerm(float nh, float roughness)
            {
                   roughness = pow(roughness,2.5);
             float ggx = (nh * roughness - nh) * nh + 1.0f;
             return roughness / (ggx*ggx + 1e-7f);
            }

float SmithJointGGXVisTerm(float nl, float nv, float roughness)
            {
             roughness = pow(roughness,4);
             float lambdaV = nl * sqrt((-nv*roughness + nv)*nv + roughness);
             float lambdaL = nl * sqrt((-nl*roughness + nl)*nl + roughness);
             return 0.5f / (lambdaV + lambdaL + 1e-5f);
            }
float SmithJointGGXVisTermMobile(float nl, float nv, float roughness)
            {
             float lambdaV = nl * (nv*(1-roughness) + roughness);
             float lambdaL = nv * (nl*(1-roughness) + roughness);
             return 0.5f / (lambdaV + lambdaL + 1e-5f);
            }
////SpecularTerm(NdotL,Ndotv,Ndoth,roughness)
float SpecularTerm(float nl, float nv,float nh,float roughness)
            {
             float V = SmithJointGGXVisTermMobile(nl, nv, roughness);
             float D = HairGGXTerm(nh,roughness);
             return max(0,V * D * (3.1415926539f/2) * nl);
            }

half3 CalculateHairDiffuse(float nl,float nv,float nh,float lh,float roughness,half3 attenColor,fixed3 baseRGBA,half3 sh,half3 _rim,half3 F0,fixed nohair,fixed4 mask)
            {
             float3 diffuseterm = DisneyDiffuse(nv,nl,lh,roughness)*0.25 * nl * attenColor;
             float Specularterm = SpecularTerm(nl, nv, nh, roughness);
             float3 frsnterm = F0 + (1 - F0)*pow(1-lh,5);
             float3 indirectDiffuse = (baseRGBA*mask.r+nohair) * sh;
             float3 diffterm = (diffuseterm + indirectDiffuse) * (nv*0.6+0.4) * baseRGBA;
             float3 spcterm = (Specularterm*frsnterm/3.1415926539f);
//             return max(0,(diffterm+spcterm)*baseRGBA + _rim);
             return indirectDiffuse;
            }