#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityStandardUtils.cginc"
//#include "MainCharacterShadowMap.cginc"

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
half3 ColorCstm(half3 basecolor, half3 Destcolor, fixed colorrange)
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
half g2r(half gloss)
            {
            return max(0.001, 1-gloss);
            }
half g4r(half gloss)
            {
            return max(0.001, 1-pow(gloss,1.3));
            }
////a2(rough)
half a2(half roughness)
            {
            return (roughness * roughness * roughness * roughness);
            }
////pie
fixed pie2(fixed pie)
            {
            return (pie/2);
            }
//reflp(_SpecularmapRGBA_var.rgb,pie)
half reflp(half3 _SpecRGB, fixed p)
            {
            half3 specmncrm = dot(_SpecRGB,float3(0.3,0.59,0.11));
            fixed p2 = pie2(p);
            half refl = sin((specmncrm-1)*p2);
                  refl += 1;
            return max(0,refl);
            }
////AmbientColorGradient(normalDirection)
half3 AmbientColorGradient(float3 n)
            {
//            float3 Ambientcolor = ShadeSHPerPixel(n, half3(0, 0, 0));
            half3 Ambientcolor = ShadeSHPerPixel(n, half3(0, 0, 0), half3(0,0,0));
//                   Ambientcolor *= nv;
            return max(half3(0, 0, 0),Ambientcolor);
            }

////Scattering
////Scattering(Ndotv, 10, 0.05, _MatMask_var.r)
half Scattering(float Ndotv, fixed contraction, fixed feedback, half scatterrange)
             {
             float nv = Ndotv * Ndotv;
             half scattering = exp2(-(nv * contraction)) + feedback;
             return saturate(scattering * scatterrange);
             }
half lightwraping(half NdotL, half Ndotv)
             {
             half lightwrap = NdotL * pow(Ndotv, 1 + NdotL);
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
half3 Frsn(float Ndotv, float gloss, half3 ibl, fixed equip)
             {
             half rough = g2r(gloss);
             half at = a2(rough);
             half frsnl = saturate(Ndotv + sqrt((Ndotv - Ndotv * at) * Ndotv * 3 + at));
             half3 fsnl = max(0,1-frsnl);
                   fsnl *= ibl * equip * gloss;
             return max(half3(0, 0, 0),fsnl);
             }

////RimEffect
////Rim(NdotV,_RimPower,_RimColor)
half3 Rim(float NdotV, fixed rimpower, fixed3 rimcolor)
             {
             rimpower = clamp(rimpower,0,5);
             half3 _Rim = pow(NdotV,rimpower) * rimcolor.rgb * saturate(rimpower);
             return max(half3(0, 0, 0),_Rim*0.7);
             }
half FrsnReflectance(float3 h, float3 v, float f0)
             {
             half base = 1 - dot(v,h);
             half exponetial = pow(base,5);
             return exponetial + f0 * (1-exponetial);
             }

///Lighting Functions
////SpecularGGX(pie, gloss, _MatMask.r, Ndoth, NdotV, NdotL)
half SpecularGGX(fixed pie, half gloss, fixed _MatMask, float Ndoth, float NdotV, float NdotL)
            {
            half hf = clamp((gloss-0.5)*2,0,1);
            fixed pi2 = pie2(pie);
            half hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            half roughness = g2r(gloss);
            half a = a2(roughness);
            half ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            half ndf2 = ((roughness*roughness) / (pie * pow((1 + (-0.65+pow(roughness,3))*(Ndoth)),2)) * gloss) * (sqrt(Ndoth)+NdotV);
            half ndf3 = ((roughness*roughness*roughness) / (pie * pow((1 + (-0.5+pow(roughness,3))*(Ndoth)),3))) * hf * 10;
            half ndf4 = sqrt(NdotL) * roughness * roughness * 0.8;
            half D = ((ndf2 + ndf3)*gloss) + (ndf1*hf2lf) + ndf4;
                 D = max(0,D);
            half G = (2 * Ndoth) / (Ndoth + sqrt(a + (1 - a)*Ndoth));
                 G = max(0,lerp(1,G,gloss));
            return max(0,D*G) ;
            }
half SpecularGGXCreate(fixed pie, half gloss, fixed _MatMask, float Ndoth, float NdotV, float NdotL)
            {
            half hf = clamp((gloss-0.5)*2,0,1);
            fixed pi2 = pie2(pie);
            half hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            half roughness = g2r(gloss);
            half a = a2(roughness);
            half ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            half ndf2 = ((roughness*roughness) / (pie * pow((1 + (-0.65+pow(roughness,3))*(Ndoth)),2)) * gloss) * (sqrt(Ndoth)+NdotV);
            half ndf3 = ((roughness*roughness*roughness) / (pie * pow((1 + (-0.5+pow(roughness,3))*(Ndoth)),3))) * hf * 10;
            half ndf4 = sqrt(NdotL) * roughness * roughness * 0.8;
            half D = ((ndf2 + ndf3)*gloss) + (ndf1*hf2lf) + ndf4;
                  D = max(0,D);
            half G = (2 * Ndoth) / (Ndoth + sqrt(a + (1 - a)*Ndoth));
                  G = max(0,lerp(1,G,gloss));
            return max(0,D*G) ;
            }
half SpecularGGXWithHeadLight(fixed pie, half gloss, fixed _MatMask, float Ndoth, float Ndotv,float NdotV, float NdotL, float Ndotl)
            {
            half hf = clamp((gloss-0.5)*2,0,1);
            fixed pi2 = pie2(pie);
            half hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            half roughness = g2r(gloss);
            half a = a2(roughness);
            half ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            half ndf2 = ((roughness*roughness) / (pie * pow((1 + (-0.65+pow(roughness,3))*(Ndoth)),2)) * gloss) * (sqrt(Ndoth)+NdotV);
            half ndf3 = ((roughness*roughness*roughness) / (pie * pow((1 + (-0.5+pow(roughness,3))*(Ndoth)),3))) * hf * 10;
            half ndf4 = sqrt(NdotL) * roughness * roughness * 0.8;
            half nvf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndotv,2)),2));
            half nvf2 = sqrt(Ndotv) * roughness * 0.8;
            half D1 = ((ndf2 + ndf3)*gloss) + (ndf1*hf2lf) + ndf4;
                  D1 = max(0,D1);
            half D2 = max(0,(nvf1*hf2lf) + nvf2);
            half G = (2 * Ndoth) / (Ndoth + sqrt(a + (1 - a)*Ndoth));
                  G = max(0,lerp(1,G,gloss));
            return max(0,D1*G + D2*Ndotl);
            }
half SpecularGGXSimplefiedWithHeadLight(fixed pie, half gloss, fixed _MatMask, float Ndoth, float Ndotv, float NdotL, float ndotl)
            {
            fixed pi2 = pie2(pie);
            half hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            half roughness = g2r(gloss);
            half a = a2(roughness);
            half ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            half ndf4 = sqrt(NdotL) * roughness;
            half nvf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndotv,2)),2));
            half nvf2 = sqrt(Ndotv) * roughness * 0.8;
            half D = max(0,(ndf1*hf2lf) + ndf4);
            half D2 = max(0,(nvf1*hf2lf) + nvf2) * ndotl;
            return max(0,D + D2) ;
            }
half SpecularGGXSimplefied(fixed pie, half gloss, fixed _MatMask, float Ndoth, float Ndotv, float NdotL)
            {
            fixed pi2 = pie2(pie);
            half hf2lf = gloss < lerp(0.7,1,_MatMask) ? sin((gloss*(pi2))/(pi2)) : 1;
            half roughness = g2r(gloss);
            half a = a2(roughness);
            half ndf1 = pow(roughness,2.75) / (pie * pow((1 + (-1+pow(roughness,2))*pow(Ndoth,2)),2));
            half ndf4 = sqrt(NdotL) * roughness;
            half D = (ndf1*hf2lf) + ndf4;
            return max(0,D) ;
            }
////Specularskin(gloss, Ndoth, _lf, spGGX, NdotL, attenColor, Ndotv, _headlight, Ndotl, _skinspcolor, specmncrm)
half3 Specularskin(half gloss, float Ndoth, float lf, float spGGX, float NdotL, float3 attenColor, float Ndotv, fixed _headhighlight, float Ndotl, half3 _skinspcolor, half3 specmncrm)
            {
            half sp1 = exp2( gloss * 9 + 1);
            half sp2 = exp2( gloss * 3 + 1);
            half3 spc1 = spGGX * NdotL * attenColor;
            half3 spc2 = pow(Ndotv,exp2( gloss * 9 + 1))*lf;
            half3 spc3 = _skinspcolor * lerp(specmncrm,1,0.5);
            return max(half3(0, 0, 0), (spc1+spc2)*spc3);
//            return max(half3(0, 0, 0), spc1);
            }
////KSskinSpecular(gloss,Ndoth,_Beckmanmap,VdotH,NdotL,halfDirection,_skinspcolor)
half3 KSskinSpecular(float gloss, float Ndoth, sampler2D _Beckmanmap, float V, float NdotL, float3 halfDirection, half3 _skinspcolor)
            {
            half roughness = g2r(gloss);
            half beckmanmapping = tex2D(_Beckmanmap, float2(Ndoth,roughness)) * 2;
            half sp = pow(beckmanmapping,10);
            half frrefl = FrsnReflectance(halfDirection,V,0.028);
            half ndf4 = sqrt(NdotL) * roughness * roughness * 0.8;
            half3 frSpec = sp * frrefl / dot(halfDirection,halfDirection);
                  frSpec = max(half3(0, 0, 0),frSpec+ndf4);
            return frSpec;
            }

////AnisotropicSpecular
////SpecAniso(_AnisoDir, viewDirection, normalDirection, _AnisoOffset, _Glossiness, _spread, aspc)
half SpecAniso(float3 _AnisoDir, float3 viewDir, float3 normalDir, half _AnisoOffset, half _Glossiness, half _spread, half aspc)
             {
             half3 ahalfvec = normalize(_AnisoDir + viewDir);
             half HdotA = max(0,dot(normalDir, ahalfvec));
             half aniso = max(0,sin(radians((HdotA + _AnisoOffset) * 180)));
             half speca = saturate(pow(aniso, _Glossiness * _spread)) * aspc;
             return max(0,speca);
             }
half SpecAniso2(float HdotA, half _AnisoOffset, half _Glossiness, half _spread, half aspc)
             {
             half aniso = max(0,sin(radians((HdotA + _AnisoOffset) * 180)));
             half speca = saturate(pow(aniso, _Glossiness * _spread)) * aspc;
             return max(0,speca);
             }
half SpecAniso3(float ahalfvec, float3 normalDir, half _AnisoOffset, half _Glossiness, half _spread, half aspc)
             {
             half HdotA = max(0,dot(normalDir, ahalfvec));
             half aniso = max(0,sin(radians((HdotA + _AnisoOffset) * 180)));
             half speca = saturate(pow(aniso, _Glossiness * _spread)) * aspc;
             return max(0,speca);
             }
////WardAnisotopicSpecular
////SpecWardAniso(NdotL, Ndoth, Ndotv, _AlphaX, _AlphaY, halfDirection, tangentDir, binormalDirection)
half SpecWardAniso(float3 N,float3 H,float3 T,half shift1,half shift2,float3 NL, float3 NV,float3 NH)
             {
             float3 binormal = cross(N,T);
             half dotht = dot(H,T) / shift1;
             half dothb = dot(H,binormal) / shift2;
             half ward = sqrt(max(0, NL/NV)) * exp(-2*((dotht*dotht)+(dothb*dothb))/(1+NH));
             return max(0,ward);
             }

////StrandTangentSpecular
////StrandSpec(tangentDir,halfDirection,_Glossiness)
half StrandSpec(float3 T, float3 H, half exponet)
             {
             half TdotH = max(0,dot(T,H));
             half sinTH = sqrt(1 - TdotH*TdotH);
             half spatten = smoothstep(-1,0,TdotH);
             return spatten * pow((1-sinTH),exponet);
             }
////StrandSpecAniso(normalDirection,i.tangentDir,tangentDir1,halfDirection,128,_AnisoOffset,_AnisoOffset2);
half StrandSpecAniso(float3 N, float3 td, half3 shiftTex, float3 H, half exponet, half _AnisoOffset)
             {
              half shift = shiftTex.g - 0.5;
              half3 T = -normalize(cross(N,td));
              half3 t1 = normalize(T + (_AnisoOffset + shift) * N);
              float TdotH = dot( t1, H );
              float sinTH = sqrt(1 - TdotH*TdotH);
              half diratten = smoothstep(-1,0,TdotH);
              half strandsp1 = max(0,diratten * pow(sinTH,exponet));
              return max(0,diratten * pow(sinTH,exponet));
             }

half StrandSpecAniso2layer(float3 N, float3 td, float3 shiftTex, float3 H, float exponet, half _AnisoOffset, half _AnisoOffset2)
             {
              half shift = shiftTex.g - 0.5;
              half3 T = -normalize(cross(N,td));
              half3 t1 = normalize(T + (_AnisoOffset + shift) * N);
              half3 t2 = normalize(T + (_AnisoOffset2 + shift) * N);

              float TdotH = dot( t1, H );
              float sinTH = sqrt(1 - TdotH*TdotH);
              float diratten = smoothstep(-1,0,TdotH);
              float strandsp1 = max(0,diratten * pow(sinTH,exponet));

              float Tdoth = dot( t2, H );
              float sinTh = sqrt(1 - Tdoth*Tdoth);
              float diratten2 = smoothstep(-1,0,Tdoth);
              float strandsp2 = max(0,diratten2 * pow(sinTh,exponet));
              return max(0,strandsp1 + strandsp2);
             }
////LightingWithHeadLight(NdotL, Ndotv, _headlight)
half LightingWithHeadLight(float NdotL, float Ndotv, fixed _headlight)
             {
             half headlighting = sqrt(Ndotv) * (1-NdotL) * _headlight;
                   headlighting = max(0,headlighting);
             half nl = (NdotL*0.75) + headlighting;
             return max(0,nl);
             }
////BRDFMapping
////LightingbyBRDFmap(_Brdfmap, NdotL, Ndotv, Ndotl, _brdfrange, _brdfmod, _headlight, _SkinDirpow)
half3 LightingbyBRDFmap(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, half _brdfrange, half _brdfmod, fixed _headlight, fixed SkinDirpow)
             {
             float2 brdfuv = float2(0,0);
                    brdfuv.x = NdotL + _brdfrange + (Ndotv*_headlight*Ndotl);
//                    brdfuv.x *= (Ndotv*0.8 + 0.2);
                    brdfuv.y = _brdfmod;
//             float3 brdfcolor = tex2D(_Brdfmap, brdfuv).rgb * (0.7 + (NdotL*(SkinDirpow)));
//             return brdfcolor * (pow(NdotL,0.7)*0.5+0.5);
             return tex2D(_Brdfmap, brdfuv).rgb*(0.7+NdotL*0.1) + (NdotL*(SkinDirpow));
             }
float3 LightingbyBRDFmap2(sampler2D _Brdfmap, float NdotL, float Ndotv, float Ndotl, float _brdfrange, float _brdfmod, fixed _headlight, fixed SkinDirpow, fixed3 edgec)
             {
             half edge = exp2(-(Ndotv*Ndotv*8));
             float2 brdfuv = float2(0,0);
                    brdfuv.x = NdotL + _brdfrange + (Ndotv*_headlight*Ndotl);// + edge;
//                    brdfuv.x = NdotL + _brdfrange + (Ndotv*_headlight*Ndotl);// + edge;saturate(sqrt(NdotL)*1.15)
//                    brdfuv.x = brdfuv.x*0.5+0.5;
                    brdfuv.y = _brdfmod;
             half3 scat = lerp(fixed3(1,1,1),edgec,edge);
                   scat = lerp(fixed3(1,1,1),scat,saturate(sqrt(NdotL)*1.15));
//             return tex2D(_Brdfmap, brdfuv).rgb * (0.7 + (NdotL*(SkinDirpow)));
             return tex2D(_Brdfmap, brdfuv).rgb*(0.7+NdotL*0.1)*scat + (NdotL*(SkinDirpow));
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
////Eye(_Eyecolor.rgb,_MatMask_var,emispower,NdotL,Ndotv)
half3 Eye(half3 basergb, fixed4 _MatMask_var, fixed emispower, float NdotL, float Ndotv , half3 sh)
             {
             fixed eyerange = saturate(_MatMask_var.g*10);
             half3 _EyeColorcstm = basergb * eyerange;
             fixed3 _eyebase = (NdotL*0.7+0.3) * emispower;
                    _eyebase += dot(sh,float3(0.3,0.59,0.11))*0.8;
                    _eyebase *=  _EyeColorcstm;
             half3 eyecolor = _eyebase + (1 - eyerange -_MatMask_var.r) * basergb;
             half3 eyerim = pow((1-Ndotv),5.0) * eyerange * 0.2;
             return max(half3(0, 0, 0),eyecolor+eyerim) * eyerange;
             }
half3 Eyenpc(half3 basergb, fixed4 _MatMask_var, fixed emispower, float NdotL, float Ndotv, half3 sh)
             {
             fixed eyerange = _MatMask_var.b < 0.5 ? _MatMask_var.b : 0;
                   eyerange = saturate(eyerange*3);
             half3 _EyeColorcstm = basergb * eyerange;
             half3 eyespec = pow(Ndotv,512) * eyerange * 0.4;
             fixed3 _eyebase = (NdotL*0.7+0.3) * emispower;
                    _eyebase += dot(sh,float3(0.3,0.59,0.11))*0.8;
                    _eyebase *= _EyeColorcstm;
             half3 eyecolor = _eyebase + (1 - eyerange -_MatMask_var.r) * basergb;
             half3 eyerim = pow((1-Ndotv),5.0) * eyerange * 0.2;
             return max(half3(0, 0, 0),eyecolor+eyerim) * eyerange;
             }
////Gathering
////CalculateSpecular(_SpecularmapRGBA_var.rgb,spGGX,gloss,attenColor,IBL,equiprange,sPc_skin,0)
half3 CalculateSpecular(half3 spc, half spGGX, half gloss, half3 attenColor, half3 IBL, half equiprange, half3 sp_skin, half3 sph)
             {
             half3 spBase = spc.rgb * equiprange;
             half3 directSpecular = spGGX * (0.25+gloss*1.5);
                   directSpecular = max(0,directSpecular) * attenColor;
             half3 indirectSpecular = IBL * equiprange * sqrt(gloss);
                   indirectSpecular *= max(0,1-directSpecular);
                   indirectSpecular = max(0,indirectSpecular);
             half3 spec = directSpecular + indirectSpecular + sph;
                   spec *= spBase;
             return max(half3(0, 0, 0),spec+sp_skin);
             }
////CalculateDiffuse(NdotLs,NdotLq,scattering,attenColor,sh,baseRGBA)
half3 CalculateDiffuse(half3 NdotLs, half NdotLq, half3 scattering, half3 attenColor, half3 sh, half3 base)
             {
             half3 ndf = NdotLs + NdotLq;
             half3 directDiffuse = ndf * attenColor;
             half3 indirectDiffuse = sh;
                   indirectDiffuse *= 0.7;
                   indirectDiffuse += scattering;
             half3 dif = directDiffuse + indirectDiffuse;// + i.pl*0;
             return max(half3(0, 0, 0),dif*base);
             }
//CalculateHairDiffuse(NdotL,Ndotl,_NdotL,Ndotv,_HairColorCustom.rgb,attenColor,_haircolor,_MatMask_var.r,CCparam.y);
half3 CalculateHairDiffuse(half NL,half Nl,half _NL,half nv,fixed3 hc, half3 lc,half3 hcc,half mask,half sat, half value)
             {
             half Ndotv2 = pow(nv,1.5)*Nl;
             half NDotL = max(0.04,(pow(NL,2)+(Ndotv2*lerp(0.75,0.7*_NL,sat))))*mask;
             half hcg = max(0.04,hc.g);
             half huefix = max(0.04,1-atan(hcg*1.5)+(asin(hcg)/1.575*0.2));
             half hrbfix = hc.r + hc.b;
                  hrbfix = hrbfix < 1.6 ? 0 : ((hrbfix-1.6)*0.3);
             half satfix = sat < 0.5 ? 0 : (sat-0.5);
                  satfix = NDotL*(satfix + max(0.5,value)*0.5);
                  satfix *= lerp(hcc,fixed3(1,1,1),satfix*0.1);
             half satb = 1 - sat;
             half3 hairgrdt = lerp(pow(hcc,(1+hcg*2)),fixed3(1,1,1),NL+Nl*satb*(1-value)) * (huefix*0.2+hrbfix+0.8);
                   hairgrdt *= lerp((max(1,sat+value)+1)*0.5,1,NL+Ndotv2);
             return max(fixed3(0.04,0.04,0.04),hcc * hairgrdt + satfix*huefix*lc);
             }
half3 CalculateHairDiffuseNPC(half NL,half Nl,half _NL,half nv,half3 lc,half3 hcc,half mask,half sat, half value)
             {
             half Ndotv2 = pow(nv,1.5)*Nl;
             half NDotL = max(0.04,(pow(NL,2)+(Ndotv2*lerp(0.75,0.7*_NL,sat))))*mask;
             half hcg = max(0.04,hcc.g);
             half huefix = max(0.04,1-atan(hcg*1.5)+(asin(hcg)/1.575*0.2));
             half hrbfix = hcc.r + hcc.b;
                  hrbfix = hrbfix < 1.6 ? 0 : ((hrbfix-1.6)*0.3);
             half satfix = sat < 0.5 ? 0 : (sat-0.5);
                  satfix = NDotL*(satfix + max(0.5,value)*0.5);
             half satb = 1 - sat;
             half3 hairgrdt = lerp(pow(hcc,(1+hcg*2)),fixed3(1,1,1),NL+Nl*satb*(1-value)) * (huefix*0.2+hrbfix+0.8);
                   hairgrdt *= lerp((max(1,sat+value)+1)*0.5,1,NL+Ndotv2);
             return max(fixed3(0.04,0.04,0.04),hcc * hairgrdt + satfix*huefix*lc);
             }
float3 FastSSSforskin(float _nl,float nv,fixed3 edgec)
             {
             float wraping = _nl*nv;
             return lerp(fixed3(1,1,1),edgec,wraping);
             }
float3 FakeSSSLightingforSkin(float nl, float nv, float3 sh, fixed3 edgec, float sct)
             {
             float halfnl = nl*0.5+0.5;
             float env = dot(sh,float3(0.3,0.59,0.11))*0.5;
//             float3 volm = lerp(lerp(edgec,fixed3(1,1,1),saturate(pow(nv,0.5))*0.5+0.5),fixed3(1,1,1),nl);
             float3 volm = lerp(lerp(fixed3(1,1,1),edgec,exp2(-(nv*nv*6))),fixed3(1,1,1),nl);
//             float3 volm = FastSSSforskin(_nl,nv,edgec);
             float3 sctr = min(sct*nl,0.5)*sh;
             return max(fixed3(0,0,0),(halfnl+env) * volm + sctr);
             }
float3 FakeSSSLightingforSkin1(float nl, float nv, float3 sh, fixed3 edgec, float sct, float _nl)
             {
             float halfnl = saturate(nl+nv*0)*0.5+0.5;
             float env = dot(sh,float3(0.3,0.59,0.11))*0.5;
             float3 volm = edgec;
             float3 sctr = min(sct*nl,0.5)*sh;
             return max(fixed3(0,0,0),(halfnl+env) * volm + sctr);
//             return volm;
             }
fixed4 _SunColorchar;
float4 _SunDirchar1;
fixed4 _ReflectionColor;
fixed _Addon;
//fixed _AddOn = saturate(_Addon);
half3 AdditionalLightColor(float nladd)
            {
//            return max(fixed3(0,0,0),_SunColorchar_test.rgb * _SunColorchar.a * nladd * _Addon);
            return max(fixed3(0,0,0),_ReflectionColor.rgb * _SunDirchar1.a * nladd * _Addon);
            }
half3 AdditionalLightColorCreat(float nladd)
            {
            return max(fixed3(0.04,0.04,0.04),_SunColorchar.rgb * _SunColorchar.a * nladd);
            }
half3 CalculateDiffuseAddon(half3 NdotLs, half NdotLq, half3 scattering, half3 attenColor, half3 sh, half3 base,half add)
             {
             half3 ndf = (NdotLs+NdotLq);
             half3 directDiffuse = ndf * attenColor + AdditionalLightColor(add);
             half3 indirectDiffuse = sh;
                   indirectDiffuse *= 0.7;
//                   indirectDiffuse *= 0.7*(NdotLs + (pow(NdotLq,0.7)*0.5+0.5));
                   indirectDiffuse += scattering;
             half3 dif = directDiffuse + indirectDiffuse;// + i.pl*0;
             return max(half3(0, 0, 0),dif*base);
             }
half3 CalculateDiffuseAddonf(half3 NdotLs, half NdotLq, half3 scattering, half3 attenColor, half3 sh, half3 base,half add,half mask)
             {
             half3 ndf = (NdotLs+NdotLq);
             half3 directDiffuse = ndf * attenColor + AdditionalLightColor(add);
             half3 indirectDiffuse = sh*0.7*(1-mask);
//             half3 indirectDiffuse = lerp(sh*0.7,dot(sh,float3(0.3,0.59,0.11))*0.5,mask);
//                   indirectDiffuse += dot(sh,float3(0.3,0.59,0.11))*0.5*mask;
//                   indirectDiffuse *= 0.7*(NdotLs + (pow(NdotLq,0.7)*0.5+0.5));
                   indirectDiffuse += scattering;
             half3 dif = directDiffuse + indirectDiffuse;// + i.pl*0;
             return max(half3(0, 0, 0),dif*base);
             }
half3 CalculateDiffuseAddonCreat(half3 NdotLs, half NdotLq, half3 scattering, half3 attenColor, half3 sh, half3 base,half add)
             {
             half3 ndf = NdotLs + NdotLq;
             half3 directDiffuse = ndf * attenColor + AdditionalLightColorCreat(add);
             half3 indirectDiffuse = sh;
                   indirectDiffuse *= 0.7;
                   indirectDiffuse += scattering;
             half3 dif = directDiffuse + indirectDiffuse;// + i.pl*0;
             return max(half3(0, 0, 0),dif*base);
             }
half3 CalculateSpecularCreate(half3 spc, half spGGX, half gloss, half3 attenColor, half3 IBL, half equiprange, half3 sp_skin, half spGGX2)
             {
             half3 spBase = spc.rgb * equiprange;
             half3 directSpecular = spGGX * (0.25+gloss*1.5);
                   directSpecular += spGGX2 * _SunColorchar.rgb * _SunColorchar.a;
                   directSpecular = max(0,directSpecular);
             half3 indirectSpecular = IBL * equiprange;
                   indirectSpecular *= max(0,1-directSpecular);
                   indirectSpecular = max(0,indirectSpecular);
             half3 spec = directSpecular + indirectSpecular;
                   spec *= spBase;
             return max(half3(0, 0, 0),spec+sp_skin);
             }
////FinalColor
////FinalColor(diffuse,specular,reflerp,_Rim,zis)
half3 FinalColor(half3 diffuse, half3 specular, float reflerp, half3 rim, half3 frsn)
              {
              half3 c = diffuse + specular;
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
                UNITY_SHADOW_COORDS(6)
//                MCSHADOW_TC(6)
//                LIGHTING_COORDS(6,7)
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
                TRANSFER_SHADOW(o)
//                TRANSFER_VERTEX_TO_FRAGMENT(o)
//                TRANSFER_MCSHADOW_TC(o, o.normalDir, shadowlightDir);
                return o;
            }

uniform sampler2D _MainTex;
float4 _MainTex_ST;
uniform sampler2D _DispMap;
float4 _DispMap_ST;
uniform sampler2D _MaskTex;
float4 _MaskTex_ST;

struct appdata_TeraPBRfx
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                fixed4 vcolor : COLOR0;
            };
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
//                MCSHADOW_TC(6)
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
//                TRANSFER_MCSHADOW_TC(o, o.normalDir, shadowlightDir);
                return o;
            }

////ImageBasedLighting
////ImageBasedLighting(gloss,viewReflectDirection)
half3 ImageBasedLighting(float gloss, float3 viewDir)
            {
            half refpow = max(0,exp(gloss*5-3)*0.14);
            half ref = lerp(10,1,refpow);
            half3 IBL = texCUBElod(_EnvMap, half4(viewDir,ref));
            return lerp(fixed3(0.25,0.25,0.25),IBL,_Refint);
            }
half3 ImageBasedLightingNPC(float gloss, float3 viewDir)
            {
            half refpow = max(0,exp(gloss*5-3)*0.14);
            half ref = lerp(10,3,refpow);
            half3 IBL = texCUBElod(_EnvMap, half4(viewDir,ref));
            return lerp(fixed3(0.25,0.25,0.25),IBL,_Refint*0.75);
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

half3 CalculateHairDiffuse1(float nl,float nv,float nh,float lh,float roughness,half3 attenColor,fixed3 F1,half3 sh,half3 _rim,half3 F0,fixed nohair,fixed mask,fixed3 haircolor)
            {
             half3 diffuseterm = DisneyDiffuse(nv,nl,lh,roughness)*0.5 * attenColor * nl;//(nl*0.5+0.5);
             half3 indirectDiffuse = haircolor*mask*sh;
                    indirectDiffuse += nohair * sh;
             half3 diffterm = (diffuseterm + indirectDiffuse) * (nv*0.3+0.7);
             half Specularterm = SpecularTerm(nl, nv, nh, roughness);
             half3 frsnterm = F0 + (1 - F0)*pow(1-lh,5);
             half3 spcterm = (Specularterm*frsnterm/3.1415926539f);
             return max(0,(diffterm+spcterm)*F1 + _rim);
            }

float2 uvRotation(float2 uv,float4 time,float angv, float2 center)
            {
            float2 pivot = center;
			float cosAngle = cos(time.x * angv);
			float sinAngle = sin(time.x * angv);
			float2x2 rot = float2x2(cosAngle,-sinAngle,sinAngle,cosAngle);
				uv = uv - pivot;
				uv = mul(rot,uv);
			return uv + pivot;
            }

uniform sampler2D _combmap; 
uniform float4 _combmap_ST;
struct appdata_Terahair
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                fixed4 vcolor : COLOR0;
            };
struct V2f_Terahair
            {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                float3 tangent2 : TEXCOORD8;
                fixed4 vcolor : COLOR0;
                fixed3 pl : COLOR1;
//                MCSHADOW_TC(6)
                //SHADOW_COORDS(6)
                UNITY_FOG_COORDS(9)
            };
V2f_Terahair vert_tera_hair(appdata_Terahair v) 
            {
                V2f_Terahair o = (V2f_Terahair)0;
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
//		        float3 tangentDir = tex2D(_combmap,TRANSFORM_TEX(o.uv0.xy, _combmap)).xyz;
//		        o.tangent2 = TRANSFORM_TEX(o.uv0.xy, _combmap);
//		        o.tangent2 = normalize( mul( unity_ObjectToWorld, float4( tangentDir.xyz, 0.0 ) ).xyz );
                UNITY_TRANSFER_FOG(o,o.pos);
//                TRANSFER_MCSHADOW_TC(o, o.normalDir, shadowlightDir);
                return o;
            }