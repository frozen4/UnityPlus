// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Environment/FX_Water_Distortadd_masked"
{
    Properties
    {
        _DissolveColor("Dissolve Color", Color) = (1.0, 0.0, 0.0, 0.5)
        _MainColor("Main Color", Color) = (1.0, 1.0, 1.0, 0.5)
        _MainTex("Main Texture", 2D) = "white" {}
        _DissolveTex("Dissolve Texture (RGBA)", 2D) = "white" {}
        _Transparent("Transparent", Range(0.0, 1.0)) = 0.5
        _DissolveWidth("Dissolve Width", Range(0.001, 1.0)) = 0.1
        _MMultiplier ("lighteness", Float) = 1.0
		_DiffuseStren("Diffuse Strenth", Float) = 1.0

		_Mask_Tex("_Mask_Tex", 2D) = "white" {}
		_Mask_UVc_Layer2("_Mask_UVc_Layer2", Vector) = (1, 1, 0, 0)
		_Mask_UVc_Layer3("_Mask_UVc_Layer3", Vector) = (1, 1, 0, 0)
		_Mask_UVc_Layer4("_Mask_UVc_Layer4", Vector) = (1, 1, 0, 0)
		_Mask_UVc_Layer12_Speed("_Mask_UVc_Layer12_Speed", Vector) = (0, 0, 0, 0)
		_Mask_UVc_Layer34_Speed("_Mask_UVc_Layer34_Speed", Vector) = (0, 0, 0, 0)
		_AlphaScale("Alpha Scale", Range(1, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "LightMode" = "Always"
        }

        // Back pass
        pass
        {
            ColorMask RGBA

            Lighting Off
            Cull Off
            ZWrite Off
            ZTest On

            Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			uniform float4 _TimeEditor;

            float4 _DissolveColor;
            float4 _MainColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;
            float _Transparent;
            float _DissolveWidth;
            float _MMultiplier;
			float _DiffuseStren;

			uniform sampler2D _Mask_Tex;

			uniform float4 _Mask_UVc_Layer2;
			uniform float4 _Mask_UVc_Layer3;
			uniform float4 _Mask_UVc_Layer4;

			uniform float4 _Mask_UVc_Layer12_Speed;
			uniform float4 _Mask_UVc_Layer34_Speed;

			uniform float _AlphaScale;

			struct VertexInput {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
			};

            struct V2F
            {
                float4 pos : POSITION;
                float4 uv0 : TEXCOORD0;
				float4 vertexColor : COLOR;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
            };

			float2 transform_tex_uv(float2 uv, float4 ST)
			{
				return (uv.xy * ST.xy + ST.zw);
			}

            V2F vert(VertexInput v)
            {
                V2F o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.vertexColor = v.color;

				float4 node_2470 = _Time + _TimeEditor;
				float2 node_1302 = (v.uv0 + node_2470.g*_Mask_UVc_Layer12_Speed.xy);
				float2 node_5558 = (v.uv1 + node_2470.g*_Mask_UVc_Layer12_Speed.zw);
				float2 node_8129 = (v.uv2 + node_2470.g*_Mask_UVc_Layer34_Speed.xy);
				float2 node_6908 = (v.uv3 + node_2470.g*_Mask_UVc_Layer34_Speed.zw);

				o.uv0.xy = TRANSFORM_TEX(v.uv0, _MainTex);
				o.uv0.zw = TRANSFORM_TEX(v.uv0, _DissolveTex);
				o.uv1 = transform_tex_uv(node_5558, _Mask_UVc_Layer2);
				o.uv2 = transform_tex_uv(node_8129, _Mask_UVc_Layer3);
				o.uv3 = transform_tex_uv(node_6908, _Mask_UVc_Layer4);

                return o;
            }

            half4 frag(V2F i) : COLOR
            {
                float4 baseColor = tex2D(_MainTex, i.uv0.xy);
                float mask = tex2D(_DissolveTex, i.uv0.zw).a;
                float w = mask - _Transparent;
                float e = _DissolveWidth * 0.5;
                clip(w - 0.004);

                half4 resultColor = baseColor ;
                resultColor.rgb *= _MainColor.rgb * _MainColor.a * 2.0;
                if (w < _DissolveWidth)
                {
                    if (w < e)
                    {
                        float f = w / e;
                        resultColor.rgb += _DissolveColor.rgb * f * 2.0;
                        resultColor.a = f;
                    }
                    else
                    {
                        float f = (_DissolveWidth - w) / e;
                        resultColor.rgb += _DissolveColor.rgb * f * 2.0;
                    }
                }

				float4 _Mask_UVc2_var = tex2D(_Mask_Tex, i.uv1);
				float4 _Mask_UVc3_var = tex2D(_Mask_Tex, i.uv2);
				float4 _Mask_UVc4_var = tex2D(_Mask_Tex, i.uv3);

                resultColor *= i.vertexColor*_DiffuseStren;
				resultColor.a *= _AlphaScale*_Mask_UVc2_var.r*_Mask_UVc3_var.g*_Mask_UVc4_var.b;
				return resultColor;
            }

            ENDCG
        }

        // Front pass
        pass
        {
            ColorMask RGBA

            Lighting Off
            Cull Back
            ZWrite Off
            ZTest On

            Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			uniform float4 _TimeEditor;

            float4 _DissolveColor;
            float4 _MainColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;
            float _Transparent;
            float _DissolveWidth;
            float _MMultiplier;
			float _DiffuseStren;

			uniform sampler2D _Mask_Tex;

			uniform float4 _Mask_UVc_Layer2;
			uniform float4 _Mask_UVc_Layer3;
			uniform float4 _Mask_UVc_Layer4;

			uniform float4 _Mask_UVc_Layer12_Speed;
			uniform float4 _Mask_UVc_Layer34_Speed;

			uniform float _AlphaScale;

			struct VertexInput {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
			};

            struct V2F
            {
				float4 pos : POSITION;
				float4 uv0 : TEXCOORD0;
				float4 vertexColor : COLOR;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
            };

			float2 transform_tex_uv(float2 uv, float4 ST)
			{
				return (uv.xy * ST.xy + ST.zw);
			}

            V2F vert(VertexInput v)
            {
				V2F o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.vertexColor = v.color;

				float4 node_2470 = _Time + _TimeEditor;
				//float2 node_1302 = (v.uv0 + node_2470.g*_Mask_UVc_Layer12_Speed.xy);
				float2 node_5558 = (v.uv1 + node_2470.g*_Mask_UVc_Layer12_Speed.zw);
				float2 node_8129 = (v.uv2 + node_2470.g*_Mask_UVc_Layer34_Speed.xy);
				float2 node_6908 = (v.uv3 + node_2470.g*_Mask_UVc_Layer34_Speed.zw);

				o.uv0.xy = TRANSFORM_TEX(v.uv0, _MainTex);
				o.uv0.zw = TRANSFORM_TEX(v.uv0, _DissolveTex);
				o.uv1 = transform_tex_uv(node_5558, _Mask_UVc_Layer2);
				o.uv2 = transform_tex_uv(node_8129, _Mask_UVc_Layer3);
				o.uv3 = transform_tex_uv(node_6908, _Mask_UVc_Layer4);

                return o;
            }

            half4 frag(V2F i) : COLOR
            {
                float4 baseColor = tex2D(_MainTex, i.uv0.xy);
                float mask = tex2D(_DissolveTex, i.uv0.zw).a;
                float w = mask - _Transparent;
                float e = _DissolveWidth * 0.5;
                clip(w - 0.004);

                half4 resultColor = baseColor;
                resultColor.rgb *= _MainColor.rgb * _MainColor.a * 2.0*_MMultiplier;
                if (w < _DissolveWidth)
                {
                    if (w < e)
                    {
                        float f = w / e;
                        resultColor.rgb += _DissolveColor.rgb * f * 2.0;
                        resultColor.a = f;
                    }
                    else
                    {
                        float f = (_DissolveWidth - w) / e;
                        resultColor.rgb += _DissolveColor.rgb * f * 2.0;
                    }
                }
				float4 _Mask_UVc2_var = tex2D(_Mask_Tex, i.uv1);
				float4 _Mask_UVc3_var = tex2D(_Mask_Tex, i.uv2);
				float4 _Mask_UVc4_var = tex2D(_Mask_Tex, i.uv3);

				resultColor *= i.vertexColor*_DiffuseStren;
				resultColor.a *= _AlphaScale*_Mask_UVc2_var.r*_Mask_UVc3_var.g*_Mask_UVc4_var.b;
				return resultColor;
            }

            ENDCG
        }
    }
}
