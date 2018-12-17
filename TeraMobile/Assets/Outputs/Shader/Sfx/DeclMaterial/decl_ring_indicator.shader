// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Decl/decl_ring_indicator" {
	Properties {
		_MainTex ("MainTex", 2D) = "white" {}
		_MainColor("MainColor", Color) = (1, 1, 1, 1)
		_OuterRadius("OuterRadius", float) = 1
		_InnerRadius("InnerRadius", float) = 0
		_WihteEdgeLength("WihteEdgeLength", float) = 0
		_EdgeColor("EdgeColor", Color) = (1, 1, 1, 1)
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
			uniform half _OuterRadius;
			uniform half _InnerRadius;
			uniform half _WihteEdgeLength;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uv = mul (unity_Projector, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			sampler2D _MainTex;
			fixed4 _EdgeColor;
			fixed4 _MainColor;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 mainColor = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uv)) * _MainColor;
				
				float2 uvPos = i.uv - float2(0.5, 0.5);
				float l = length(uvPos);

				float clip_range = _InnerRadius / _OuterRadius / 2;
				mainColor.a = l < clip_range ? 0 : mainColor.a;

				float inner_white_range = (_InnerRadius + _WihteEdgeLength) / _OuterRadius / 2;
				mainColor = l > clip_range && l < inner_white_range ? _EdgeColor : mainColor;

				float outer_white_range = (_OuterRadius - _WihteEdgeLength) / _OuterRadius / 2;
				mainColor = l > outer_white_range && l < 0.5 ? _EdgeColor : mainColor;

				fixed4 final_color = mainColor;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, final_color, fixed4(1, 1, 1, 1));
				return final_color;
			}
			ENDCG
		}
	}
}
