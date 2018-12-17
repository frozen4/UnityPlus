Shader "Tera/MobileCamtransOpaque"
{
	Properties
	{
		_MainColor("Main Color", color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DstCull("BackFaceCull 0=2-Sided 1=1-Sided", Range(0,1)) = 0
	}
	SubShader
	{
		Tags { "IgnoreProjector"="True" "Queue"="Transparent" "RenderType" = "Transparent" }
		Cull back
		Pass
		{
			Tags { "LIGHTMODE" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile MOBILE_DYNAMIC_DIRLIGHT_ON MOBILE_DYNAMIC_DIRLIGHT_OFF

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
				float4 tangent : TANGENT;
#endif
				float2 uv : TEXCOORD0;
#if defined(LIGHTMAP_ON)
				float2 lmuv : TEXCOORD1;
#endif
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 lmuv : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float3 ambient : COLOR0;
				UNITY_FOG_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform float4 _SunColor;
			uniform float4 _SunDir;
			uniform float4 _MainColor;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#if defined(LIGHTMAP_ON)
				o.lmuv.xy = v.lmuv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 diffuse = tex2D(_MainTex, i.uv);
				fixed4 final_color = fixed4(0, 0, 0, 0.3);
				       diffuse *= _MainColor;
				half3 light_color = half3(1, 1, 1);
				half atten = 1;

#if defined(LIGHTMAP_ON)
					light_color = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmuv.xy));
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON)
					light_color *= _SunColor.rgb;
					half3 p1 = light_color;

					final_color.rgb = (diffuse.rgb * p1) * atten;
#else
					final_color.rgb = diffuse.rgb * light_color;
#endif
#else
					light_color *= _SunColor.rgb;

					half3 p1 = light_color ;

					final_color.rgb = (diffuse.rgb * p1 ) * atten;
#endif
				UNITY_APPLY_FOG(i.fogCoord, final_color);
				return final_color;
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

			float4 frag(v2f i) : SV_Target
			{
				fixed4 texcol = tex2D(_MainTex, i.uv);
				clip(texcol.a - 0.075);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}	
	}
	FallBack "Mobile/Diffuse"
}