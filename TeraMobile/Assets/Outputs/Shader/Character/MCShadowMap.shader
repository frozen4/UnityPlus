Shader "Unlit/MCShadowMap"
{
	Properties
	{

	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "MainCharacterShadowMap.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				MCSHADOW_TC(1)
				float3 normalWorld : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _SunDir;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 normalWorld = UnityObjectToWorldNormal(v.normal);
				float3 lightDir = normalize(_SunDir.xyz);

				o.normalWorld = normalWorld;

				TRANSFER_MCSHADOW_TC(o, normalWorld, lightDir);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 lightDir = normalize(_SunDir.xyz);
				//col *= saturate(dot(i.normalWorld, lightDir)) * 3;

				float shadow = SAMPLE_MCSHADOW(i);

				fixed4 final_color = col * shadow;

				return final_color;
			}
			ENDCG
		}
	}
}
