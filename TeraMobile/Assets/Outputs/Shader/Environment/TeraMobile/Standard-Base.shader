Shader "Hidden/TerrainEngine/Splatmap/Standard-Base"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MetallicTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry-100" }
		LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "TeraUtil.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 tc_Control : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
#if defined(LIGHTMAP_OFF)
				fixed3 sh : COLOR0;
#endif
				float3 normal : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _SunColor;
			uniform float4 _SunDir;
			uniform float4 _SunAmbientColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.tc_Control.xy = TRANSFORM_TEX(v.uv, _MainTex);
#if defined(LIGHTMAP_ON)
				o.tc_Control.zw = v.texcoord1 * unity_LightmapST.xy + unity_LightmapST.zw;;
#endif
				o.normal = UnityObjectToWorldNormal(v.normal);;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 final_color = (fixed4)0;
				fixed4 diffuse = tex2D(_MainTex, i.tc_Control.xy);
				
				half3 lightDir = normalize(_SunDir.xyz);
				half3 light_color = _SunColor.rgb * _SunColor.a;
				fixed3 sh = _SunAmbientColor.rgb * _SunAmbientColor.a;
#if defined(LIGHTMAP_ON)
                half3 lmcolor = decodelightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.tc_Control.zw));
                light_color *= lmcolor;
				
				sh = (sh * 0.5) + (sh * 0.5 * lmcolor);
#endif
				fixed nl = max(0, dot(i.normal, lightDir));
				fixed3 p1 = nl * light_color + sh;
				final_color.rgb = diffuse.rgb * p1;
				//final_color.rgb = i.normal;
				UNITY_APPLY_FOG(i.fogCoord, final_color);
				return final_color;
			}
			ENDCG
		}
	}
}
