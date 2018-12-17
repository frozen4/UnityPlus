Shader "Tera/MobileDiffuse"
{
	Properties
	{
		_MainColor("Base Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_lmfb("Lightmap Feedback", Range(0,1)) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 400

		Pass
		{
			Tags { "LIGHTMODE" = "ForwardBase" }

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
			uniform fixed4 _MainColor;

			uniform float4 _SunAmbientColor;

			fixed4 frag(v2f_mdd i) : SV_Target
			{
				fixed4 final_color = fixed4(0, 0, 0, 1);
				fixed4 diffuse = tex2D(_MainTex, i.uv);
				diffuse.rgb *= _MainColor.rgb;
				half3 light_color = half3(1, 1, 1);
				half atten = 1;

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
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			Tags { "LIGHTMODE" = "ForwardBase" }

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

			uniform float4 _SunAmbientColor;

			fixed4 frag(v2f_mdd200 i) : SV_Target
			{
				fixed4 final_color = fixed4(0, 0, 0, 1);
				fixed4 diffuse = tex2D(_MainTex, i.uv);
				diffuse.rgb *= _MainColor.rgb;
				half3 light_color = half3(1, 1, 1);
				half atten = 1;

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
	}
	FallBack "Mobile/Diffuse"
}
