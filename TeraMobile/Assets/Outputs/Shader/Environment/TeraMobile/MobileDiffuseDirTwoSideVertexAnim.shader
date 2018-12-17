Shader "Tera/MobileDiffuseTwoSideVertexAnim"
{
	Properties
	{
	    _MainColor("Main Color", color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
		_DstCull("Backface Cull 0=2Sided 2=1sided", Range(0,2)) = 0
		_lmfb("Lightmap Feedback", Range(0,1)) = 0
	}
		SubShader
	{
		Tags{ "RenderType" = "TransparentCutout" "Queue" = "AlphaTest-100" "IgnoreProjector" = "True" }
		LOD 600

		Pass
		{
			Tags { "LIGHTMODE" = "ForwardBase" }

			Cull [_DstCull]

			CGPROGRAM
			#pragma vertex vert_mobile_diffuse_dirva
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile MOBILE_DYNAMIC_DIRLIGHT_ON MOBILE_DYNAMIC_DIRLIGHT_OFF
			#pragma multi_compile RAIN_SURFACE_OFF RAIN_SURFACE_ON
			#pragma multi_compile SNOW_SURFACE_OFF SNOW_SURFACE_ON

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "MobileDiffuseInc.cginc"
			
			sampler2D _MainTex;
			fixed _Cutoff;

			uniform fixed4 _MainColor;
			uniform float4 _SunAmbientColor;

			fixed4 frag(v2f_mddva i) : SV_Target
			{
				fixed4 final_color = fixed4(0, 0, 0, 1);
				fixed4 diffuse = tex2D(_MainTex, i.uv);
				diffuse.rgb *= i.vcolor;
				diffuse.rgb *= _MainColor.rgb;
				half3 light_color = half3(1, 1, 1);
				half atten = 1;
				final_color.a = diffuse.a;
				clip(diffuse.a - _Cutoff);

#if defined(LIGHTMAP_ON)
				light_color = decodelightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmuv.xy));
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON)
				fixed3 ambient = _SunAmbientColor.rgb * _SunAmbientColor.a;
				final_color.rgb = ShadeWithDynamicLight_mddva(i, diffuse.rgb, light_color, atten, ambient);
#else
				final_color.rgb = diffuse.rgb * light_color;
#endif
#else
				final_color.rgb = ShadeWithDynamicLight_mddva(i, diffuse.rgb, light_color, atten, i.sh);
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
	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout" "Queue" = "AlphaTest-100" "IgnoreProjector" = "True" }
		LOD 400

		Pass
		{
			Tags { "LIGHTMODE" = "ForwardBase" }

			Cull [_DstCull]

			CGPROGRAM
			#pragma vertex vert_mobile_diffuse_dir
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile MOBILE_DYNAMIC_DIRLIGHT_ON MOBILE_DYNAMIC_DIRLIGHT_OFF
			#pragma multi_compile RAIN_SURFACE_OFF RAIN_SURFACE_ON
			#pragma multi_compile SNOW_SURFACE_OFF SNOW_SURFACE_ON

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "MobileDiffuseInc.cginc"
			
			sampler2D _MainTex;
			fixed _Cutoff;

			uniform fixed4 _MainColor;
			uniform float4 _SunAmbientColor;

			fixed4 frag(v2f_mdd i) : SV_Target
			{
				fixed4 final_color = fixed4(0, 0, 0, 1);
				fixed4 diffuse = tex2D(_MainTex, i.uv);
				diffuse.rgb *= i.vcolor;
				diffuse.rgb *= _MainColor.rgb;
				half3 light_color = half3(1, 1, 1);
				half atten = 1;
				final_color.a = diffuse.a;
				clip(diffuse.a - _Cutoff);

#if defined(LIGHTMAP_ON)
				light_color = decodelightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmuv.xy));
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON)
				fixed3 ambient = _SunAmbientColor.rgb * _SunAmbientColor.a;
				final_color.rgb = ShadeWithDynamicLight_mdd(i, diffuse.rgb, light_color, atten, ambient);
#else
				final_color.rgb = diffuse.rgb * light_color;
#endif
#else
				final_color.rgb = ShadeWithDynamicLight_mdd(i, diffuse.rgb, light_color, atten, i.sh);
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
	SubShader
	{
		Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest-100" "IgnoreProjector" = "True" }
		LOD 200

		Pass
		{
			Tags { "LIGHTMODE" = "ForwardBase" }

			Cull [_DstCull]

			CGPROGRAM
			#pragma vertex vert_200
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile MOBILE_DYNAMIC_DIRLIGHT_ON MOBILE_DYNAMIC_DIRLIGHT_OFF

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "MobileDiffuseInc.cginc"
			
			sampler2D _MainTex;
			uniform fixed4 _MainColor;
			fixed _Cutoff;

			uniform float4 _SunAmbientColor;

			fixed4 frag(v2f_mdd200 i) : SV_Target
			{
				fixed4 final_color = fixed4(0, 0, 0, 1);
				fixed4 diffuse = tex2D(_MainTex, i.uv);
				diffuse.rgb *= _MainColor.rgb;
				half3 light_color = half3(1, 1, 1);
				half atten = 1;
				final_color.a = diffuse.a;
				clip(diffuse.a - _Cutoff);

#if defined(LIGHTMAP_ON)
				light_color = decodelightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmuv.xy));
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON)
				fixed3 ambient = _SunAmbientColor.rgb * _SunAmbientColor.a;
				final_color.rgb = ShadeWithLevel200(i, diffuse.rgb, light_color, ambient);
#else
				final_color.rgb = diffuse.rgb * light_color;
#endif
#else
				final_color.rgb = ShadeWithLevel200(i, diffuse.rgb, light_color);
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
	FallBack "Mobile/Diffuse"
}
