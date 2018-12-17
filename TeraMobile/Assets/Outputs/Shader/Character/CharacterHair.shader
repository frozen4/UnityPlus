// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Character/CharacterHair"
{
	Properties
	{
		_BaseRGBA("Diffuse (RGB) Alpha (A)", 2D) = "white" {}
		_Nromalmap("BumpMap", 2D) = "black" {}
		_HairColorCustom("Main Color", Color) = (0.30588, 0.32157, 0.33725, 1.00)
		_DiffuseScatteringBias("Scatter Bias", float) = 0.05
		_DiffuseScatteringCol("Scatter Color", Color) = (0.22745, 0.07843, 0.04314, 1.00)
		_DiffuseScatteringContraction("Scatter Contraction", Int) = 10
		_SpecularmapRGBA ("SpecularMapRGB A=Gloss", 2D) = "white" {}
		_spcolorfix ("Specular Color Correction",Range(0,1)) = 0.25
		_AnisoLightDir("Aniso Light Dir", Vector) = (0, 1, 0, 0)
		_Glossiness("Glossiness", Range(0,1)) = 0.645
		_spread1("Highlight Spread", Range(0,64)) = 8
		_Metallic("Metallic", float) = 0.4
		_AnisoOffset2("Aniso Offset1", Range(-1,1)) = 0.5
		_Glossiness2("Aniso Glossiness", float) = 0.645
		_spread2("Highlight Spread1", Range(0,64)) = 8
		_Metallic2("Aniso Metallic", float) = 0.4
		_AnisoOffset("Aniso Offset", Range(-1,1)) = 0.5
		_CutOff("Alpha Cut-Off Threshold", float) = 0.5
		_MatMask ("Mat Mask(RGB)(R_Skin)(G_Eye)(B_null)(A_Transmision)", 2D) = "red" {}
		_Anisoint("Aniso intensity", Range(0,1)) = 0.7
	}
	SubShader
	{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" "Queue"="AlphaTest-50"}
//		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		    Tags {
                "LightMode"="ForwardBase"
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma target 3.0
			
			#include "HLSLSupport.cginc"
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				half4 tangentToWorld[3]	: TEXCOORD2;
				float3 normal : TEXCOORD5;
			};

			sampler2D _BaseRGBA;
			float4 _BaseRGBA_ST;
			sampler2D _Nromalmap;
			sampler2D _SpecularmapRGBA;
			float4 _SpecularmapRGBA_ST;
			sampler2D _MatMask;
			float4 _MatMask_ST;
			//half _PrimaryShift;
			//half _SecondaryShift;
			//sampler2D _AnisoMap;
			fixed4 _HairColorCustom;
			half _CutOff;

			uniform float _DiffuseScatteringBias;
			uniform float4 _DiffuseScatteringCol;
			uniform float _DiffuseScatteringContraction;
			uniform float _Glossiness;
			uniform float _Metallic;
			uniform float4 _AnisoLightDir;
			uniform float _Glossiness2;
			uniform float _Metallic2;
			uniform float _Anisoint;
			uniform float _AnisoOffset;
			uniform float _AnisoOffset2;
			uniform float _spcolorfix;
			uniform float _spread1;
			uniform float _spread2;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _BaseRGBA);
				float3 posWorld = mul(unity_ObjectToWorld, v.vertex);
				half3 normalWorld = UnityObjectToWorldNormal(v.normal);
				float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
				float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
				o.tangentToWorld[0].xyz = tangentToWorld[0];
				o.tangentToWorld[1].xyz = tangentToWorld[1];
				o.tangentToWorld[2].xyz = tangentToWorld[2];

				float3 viewDir = posWorld.xyz - _WorldSpaceCameraPos;
				o.tangentToWorld[0].w = viewDir.x;
				o.tangentToWorld[1].w = viewDir.y;
				o.tangentToWorld[2].w = viewDir.z;

				o.normal = normalWorld;

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			    fixed4 _MatMask_var = tex2D(_MatMask, i.uv);
			    fixed nonehair = 1-_MatMask_var.r;
				fixed4 diffuse_color = tex2D(_BaseRGBA, i.uv);// * _Color;
				half3 haircolor = saturate( (_HairColorCustom.rgb > 0.5 ? (1-(1-(_HairColorCustom.rgb-0.5))*(1-diffuse_color.rgb)) : (2*_HairColorCustom.rgb*diffuse_color.rgb)) ) * _MatMask_var.r;
				half3 diffuse = diffuse_color.rgb * nonehair + haircolor;
                fixed3 spcolor = tex2D(_SpecularmapRGBA,i.uv).rgb;
                       spcolor = saturate( (_HairColorCustom.rgb > 0.5 ? (1-(1-(_HairColorCustom.rgb-0.5))*(1-spcolor.rgb)) : (2*_HairColorCustom.rgb*spcolor.rgb)) ) * _MatMask_var.r;

				float3 viewDir = -normalize(half3(i.tangentToWorld[0].w, i.tangentToWorld[1].w, i.tangentToWorld[2].w));
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

				float attenuation = LIGHT_ATTENUATION(i);
				float3 lightColor = _LightColor0.rgb * attenuation;

				float3 normal_color = UnpackNormal(tex2D(_Nromalmap, i.uv));

				float3 n = normal_color;
				//TangentToWorld(i.tangentToWorld, normal);
				half3 tangent = i.tangentToWorld[0].xyz;
				half3 binormal = i.tangentToWorld[1].xyz;
				half3 normal = i.tangentToWorld[2].xyz;
				n = normalize(tangent * n.x + binormal * n.y + normal * n.z);

				float3 halfway_vec = normalize(lightDir + viewDir);

				half nl = saturate(dot(n, lightDir));
				half nv = saturate(dot(n,viewDir))*(1-nl)*_Anisoint;
				n = i.normal;
				half nh = saturate(dot(n, halfway_vec) + 0.0);
				half nV = saturate(dot(n,viewDir));
				float3 sh = UNITY_LIGHTMODEL_AMBIENT.rgb;

				half light_spec = pow(nh, _Glossiness * 128) * _Metallic * spcolor;
				half3 p1 = lightColor * (nl+nv) + sh;//0.7+0.3
				      
				half3 p2 = lightColor * light_spec;

				half NdotV = dot(n, viewDir);
				NdotV *= NdotV;
				half3 diffuseScatter = _DiffuseScatteringCol.rgb * sh * (exp2(-(NdotV * _DiffuseScatteringContraction)) + _DiffuseScatteringBias);
				diffuseScatter = saturate(diffuseScatter);
//				diffuse_color.rgb += diffuseScatter;

				// spec2
				float3 n2 = normal_color;
				n2 = normalize(tangent * n2.x + binormal * n2.y + normal * n2.z);
				//n2 = i.normal;

				//float3 anisoDir = tex2D(_AnisoMap, i.uv.yx);
				//n2 += anisoDir;

//				float n3 = pow(sqrt(1 - dot(pow(HdotA,2))),gloss * 128);

				halfway_vec = normalize(_AnisoLightDir + viewDir);
				float HdotA = max(0, dot(n2, halfway_vec));
				float aniso = max(0, sin(radians((HdotA + _AnisoOffset2) * 180)));

				float spec = saturate(pow(aniso, _Glossiness * _spread1*8) * _Metallic);
				      aniso = max(0, sin(radians((HdotA + _AnisoOffset) * 180)));
				      spec += saturate(pow(aniso, _Glossiness2 * _spread2*8) * _Metallic2);
				      spec *= lightColor * spcolor * _MatMask_var.r;
				
				fixed3 final_color = diffuse * p1 * 0.55 + spec;
				       final_color += diffuseScatter;

				fixed4 oColor = fixed4(final_color, diffuse_color.a);
				clip(oColor.a - _CutOff);

				UNITY_APPLY_FOG(i.fogCoord, oColor);
				return oColor;
			}
			ENDCG
		}
				Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
				float2 uv : TEXCOORD1;
			};

			float4 _MainTex_ST;

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			sampler2D _MainTex;
			fixed _Cutoff;

			float4 frag(v2f i) : SV_Target
			{
				fixed4 texcol = tex2D(_MainTex, i.uv);
				clip(texcol.a - _Cutoff);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}		
	}
}
