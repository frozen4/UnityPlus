// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//This is a mat for the shadows of GImageModel
//Totally transparent with shadow only 

Shader "Hidden/ImgMdShadow"
{
	Properties
	{
		_MainColor ("Color", Color) = (0,0,0,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" /*"Queue"="Transparent"*/}
		LOD 100

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
		    Name "FORWARD"
            Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			//// make fog work
			//#pragma multi_compile_fog
			#define UNITY_PASS_FORWARDBASE
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				SHADOW_COORDS(0)
				//UNITY_FOG_COORDS(1)
				float4 pos : SV_POSITION;
			};

			fixed4 _MainColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				TRANSFER_SHADOW(o);

				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed a = SHADOW_ATTENUATION(i);

				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return fixed4(1, 1, 1, 1-a) * _MainColor;
			}
			ENDCG
		}
	}

	FallBack "VertexLit"
}
