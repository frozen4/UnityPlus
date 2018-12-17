Shader "Tera/MobileDiffuseBumpNight"
{
	Properties
	{
		_MainColor("Base Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_Shininess("Shininess", Range(0.03, 1)) = 1
		_lmfb("Lightmap Feedback", Range(0,1)) = 0
		_Masktex("Lighted Mask R = Lighted", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 400

		Pass
		{
			Tags { "LIGHTMODE" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert_mobile_diffuse_bump_dir_partnight
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

			fixed4 frag(v2f_mdbdn i) : SV_Target
			{
				fixed4 final_color = fixed4(0, 0, 0, 1);
				fixed4 diffuse = tex2D(_MainTex, i.uv);
				diffuse.rgb *= _MainColor.rgb;
				diffuse.a *= _MainColor.a;
				half3 light_color = half3(1, 1, 1);
				half atten = 1;

#if defined(LIGHTMAP_ON)
				light_color = decodelightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmuv.xy));
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON)
				fixed3 ambient = _SunAmbientColor.rgb * _SunAmbientColor.a;

				final_color.rgb = ShadeWithDynamicLight_mdbdn(i, diffuse.rgb, diffuse.a, light_color, atten, ambient,_Masktex);
#else
				final_color.rgb = diffuse.rgb * light_color;
#endif
#else
				final_color.rgb = ShadeWithDynamicLight_mdbdn(i, diffuse.rgb, diffuse.a, light_color, atten, i.sh,_Masktex);
#endif
				UNITY_APPLY_FOG(i.fogCoord, final_color);
//				final_color.rgb = decodelightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmuv.xy));
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
