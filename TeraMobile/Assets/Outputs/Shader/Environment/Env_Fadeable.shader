Shader "Environment/Env_Fadeable" {

Properties {
	_MainColor ("Main Color" ,Color) = (1,1,1,1)
	_MainTex ("Textuer", 2D) = "black" {}
	_Transition ("Transition", range(0,1)) = 0
}

SubShader {
	Tags {"Queue"="Geometry+11" "IgnoreProjector"="True" "RenderType"="Transparent"}
	Lighting Off
	ZWrite Off
	ZTest LEqual

	Pass {
	    Name "Base"
	    Blend SrcAlpha OneMinusSrcAlpha
	    Fog {mode Off}

	        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			uniform fixed4 _MainColor;
			uniform sampler2D _MainTex;
			uniform fixed4 _MainTex_ST;
			uniform fixed _Transition;

            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord1 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.uv0 = v.texcoord1;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                fixed4 diffuse = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 finalColor = diffuse.rgb * _MainColor.rgb;
                fixed4 finalRGBA = fixed4(finalColor, (1-_Transition)*diffuse.a);
                return finalRGBA;
            }
            ENDCG
	     }
    }
    FallBack "Mobile/Diffuse"
}
