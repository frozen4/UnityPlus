Shader "Trail/displacement/additive" {
Properties {
 _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
 _MainTex ("Main Texture", 2D) = "white" {}
 _DispMap ("Displacement Map (RG)", 2D) = "white" {}
 _MaskTex ("Mask(R)", 2D) = "white" {}
 _AlphaTex ("Alpha(R)", 2D) = "white" {}
 _ClipValue  ("Clip Value", Range(0.0, 1.0)) = 0
 _DispScrollSpeedX  ("Map Scroll Speed X", Float) = 0
 _DispScrollSpeedY  ("Map Scroll Speed Y", Float) = 0
 _DispX  ("Displacement Strength X", Float) = 0
 _DispY  ("Displacement Strength Y", Float) = 0.2

}

Category {
 Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching" = "true" }
 Blend SrcAlpha One
 Cull Off Lighting Off ZWrite Off
 BindChannels {
     Bind "Color", color
     Bind "Vertex", vertex
     Bind "TexCoord", texcoord
 }
 
 // ---- Fragment program cards
 SubShader {
     Pass {
     
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #pragma fragmentoption ARB_precision_hint_fastest
         #pragma multi_compile_particles
         
         #include "UnityCG.cginc"

         uniform sampler2D _MainTex;
         float4 _MainTex_ST;
		 uniform sampler2D _DispMap;
		 float4 _DispMap_ST;
		 uniform sampler2D _MaskTex;
		 float4 _MaskTex_ST;
		 uniform sampler2D _AlphaTex;
		 float4 _AlphaTex_ST;
		 uniform fixed _ClipValue;
         uniform half _DispScrollSpeedX;
         uniform half _DispScrollSpeedY;

		 uniform half _DispX;
         uniform half _DispY;


         uniform fixed4 _TintColor;

         
         struct appdata_t {
             float4 vertex : POSITION;
             fixed4 color : COLOR;
			 float2 texcoord : TEXCOORD0;
             float2 param : TEXCOORD1;
         };

         struct v2f {
             float4 vertex : POSITION;
             fixed4 color : COLOR;
             float4 uv0 : TEXCOORD0;
			 float4 uv1 : TEXCOORD1;
			 float2 param : TEXCOORD2;
         }; 
         
         v2f vert (appdata_t v)
         {
             v2f o;
             o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
             o.color = v.color;
             o.uv0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
             o.uv0.zw = TRANSFORM_TEX(v.texcoord, _DispMap);
             o.uv1.xy = TRANSFORM_TEX(v.texcoord, _MaskTex);
             o.uv1.zw = TRANSFORM_TEX(v.texcoord, _AlphaTex);
			 o.param = v.param;
             return o;
         }
         fixed4 frag (v2f i) : COLOR
         {
		     //scroll displacement map.
			 half2 mapoft = half2(_Time.y*_DispScrollSpeedX, _Time.y*_DispScrollSpeedY);

			 //get displacement color
			 half4 dispColor = tex2D(_DispMap, i.uv0.zw + mapoft);

			 //get uv oft
             half2 uvoft = i.uv0.xy;
		     uvoft.x +=  dispColor.r  * _DispX * i.param.x;
			 uvoft.y +=  dispColor.g  * _DispY * i.param.x;

			 //apply displacement
			 fixed4 mainColor = tex2D(_MainTex, uvoft);

			 //get mask;
			 fixed4 mask = tex2D(_MaskTex, i.uv1.xy);

             fixed4 final_color = 2.0f * i.color * _TintColor * mainColor * mask.r;
             fixed alpha = tex2D(_AlphaTex, i.uv1.zw).a;
             final_color.a = alpha - _ClipValue < 0 ? 0 : final_color.a;
             return final_color;
         }
         ENDCG
     }
 }   
}
}