// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/TeraProjector" {
	Properties {
		_ProjectorTex ("_ProjectorTex", 2D) = "white" {}
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uv = mul (unity_Projector, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			sampler2D _ProjectorTex;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texS = tex2Dproj (_ProjectorTex, UNITY_PROJ_COORD(i.uv));
				fixed4 final_color = texS;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, final_color, fixed4(1,1,1,1));
				return final_color;
			}
			ENDCG
		}
	}
}
