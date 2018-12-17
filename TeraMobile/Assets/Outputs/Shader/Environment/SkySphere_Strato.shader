// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Tera/SkySphere_Strato" {

Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_DetailColor1 ("Color" ,Color) = (1,1,1,1)
	_Speed ("DetailTex1 2 Speed",Vector) = (0, 0, 0, 0)
	_FogHeight ("Fog Density", Float) = 1.5
	_FogColor ("Fog Color", Color) = (1,1,1,1)
	_Transition ("Transition", Range(0,1)) = 1
	}

SubShader {
	Tags {"Queue"="Geometry+12" "IgnoreProjector"="True" "RenderType"="Trasparent"}
	Lighting Off
	ZWrite Off

	Pass {
	    Blend SrcAlpha OneMinusSrcAlpha//[_DstBlend1]
        Fog {mode Off}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float4 _TimeEditor;
            uniform fixed4 _DetailColor1;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
			uniform float4 _Speed;

			uniform fixed4 _FogColor;
			uniform fixed _FogHeight;
			uniform fixed _Transition;

            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD1;
                float4 posWorld : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.uv0 = v.texcoord1;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 Timer = _Time + _TimeEditor;
                float2 uvcoord = (i.uv0+Timer.g*float2(_Speed.r,_Speed.g));
                fixed4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(uvcoord, _MainTex));
                fixed3 finalColor = (_DetailColor1.rgb*_MainTex_var.rgb);
                       finalColor = lerp(finalColor,_FogColor.rgb,pow((1.0 - max(0,dot(normalize(i.posWorld.xyz),float3(-0,1,0)))),10-_FogHeight));
                fixed4 finalRGBA = fixed4(finalColor, _MainTex_var.a*_Transition);

                return finalRGBA;
            }
            ENDCG
	     }
    }
    FallBack "Mobile/Diffuse"
}
