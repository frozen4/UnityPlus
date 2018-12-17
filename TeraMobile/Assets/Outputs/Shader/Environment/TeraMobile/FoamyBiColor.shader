Shader "Tera/FoamyBiColor"
{
	Properties
	{
		[Normal]_NormalTexture("Normal", 2D) = "white" {}
		//_NormalTiling("Normal Tiling", Float) = 1
		_DeepWaterColor("Deep Water Color", Color) = (0.07843138,0.3921569,0.7843137,1)
		_ShallowWaterColor("Shallow Water Color", Color) = (0.4411765,0.9537525,1,1)
		_Shininess("Shininess", Float) = 1
		_Gloss("Gloss", Float) = 1
		_DepthTransparency("Depth Transparency", Float) = 1.5
		_ShoreFade("Shore Fade", Float) = 0.3
		_ShallowFade("Shallow-Deep-Fade", Float) = 3
		_ShallowDeepBlend("Shallow-Deep-Blend", Float) = 3.6
		_WaveSpeed("Wave Speed", Float) = 1
		_ReflectionTex("Reflection Tex", 2D) = "white" {}
		_Distortion("Distortion", Range(0, 2)) = 0.3
		_Displacement("Displacement Tex", 2D) = "white" {}
		_DisplacementFactor("DisplacementFactor", Float) = 1
	}
		SubShader
	{
		Tags {
			"IgnoreProjector" = "True"
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Name "FORWARD"
			Tags {
				"LightMode" = "ForwardBase"
			}

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_fog

			#pragma multi_compile WATER_SOFTEDGE_ON WATER_SOFTEDGE_OFF
			#pragma multi_compile WATER_REFLECTION_ON WATER_REFLECTION_OFF

			#include "UnityCG.cginc"

			uniform sampler2D_float _CameraDepthTexture;
			//uniform sampler2D _CameraDepthNormalsTexture;
			uniform float4 _LightColor0;

			uniform sampler2D _NormalTexture;
			uniform float4 _NormalTexture_ST;

			uniform float4 _DeepWaterColor;
			uniform float4 _ShallowWaterColor;

			uniform float _DepthTransparency;
			uniform float _ShallowDeepBlend;

			uniform float _ShoreFade;
			uniform float _ShallowFade;
			uniform float _ShoreTransparency;

			uniform float _Reflectionintensity;
			uniform sampler2D _ReflectionTex;
			uniform float4 _ReflectionTex_ST;

			uniform float _WaveSpeed;
			uniform float _Shininess;
			uniform float _Gloss;

			uniform float4 _TimeEditor;
			//uniform float _NormalTiling;

			uniform sampler2D _Displacement;
			uniform float4 _Displacement_ST;
			uniform float _DisplacementFactor;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
				float3 lightDir : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float4 projPos : TEXCOORD3;
				UNITY_FOG_COORDS(4)
				float4 screenPos : TEXCOORD5;
				float4 posWorld : TEXCOORD6;
				float3 normalDir : TEXCOORD7;
				float3 tangentDir : TEXCOORD8;
				float3 bitangentDir : TEXCOORD9;
			};

			v2f vert(appdata v)
			{
				v2f o;

				float h1 = tex2Dlod(_Displacement, float4(v.uv.xy + _Time.xx * 0.1, 0, 0)).r;
				v.vertex.y += h1 * _DisplacementFactor;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _NormalTexture);
				o.uv.zw = TRANSFORM_TEX(v.uv, _NormalTexture);
				float4 _timer1 = _Time;
				o.uv.xy += float2(_WaveSpeed * _timer1.x /4.6, _WaveSpeed * _timer1.x/4.6);
				o.uv.zw += float2(-_WaveSpeed * _timer1.x / 3.3, _WaveSpeed * _timer1.x/ 3.3);

				TANGENT_SPACE_ROTATION;
				o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));

				UNITY_TRANSFER_FOG(o,o.vertex);

				o.projPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				o.screenPos = o.vertex;

				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				i.screenPos = float4(i.screenPos.xy / i.screenPos.w, 0, 0);
				i.screenPos.y *= _ProjectionParams.x;

				float3 LightDir = normalize(i.lightDir);
				float3 ViewDir = normalize(i.viewDir);

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

				LightDir = lightDirection;
				ViewDir = viewDirection;

				float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);

				// use depth & noraml texture
				float3 n;
				float sceneZ;
				//DecodeDepthNormal(tex2Dproj(_CameraDepthNormalsTexture, UNITY_PROJ_COORD(i.projPos)), sceneZ, n);
				//sceneZ *= _ProjectionParams.z;
				sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;

				//float3 normal1 = UnpackNormal(tex2D(_NormalTexture, i.uv.xy));
				float3 normal1 = (tex2D(_NormalTexture, i.uv.xy));
				//float3 normal2 = UnpackNormal(tex2D(_NormalTexture, i.uv.zw));
				float3 normal2 = (tex2D(_NormalTexture, i.uv.zw));

				float3 normal = normalize(lerp(float3(0, 0, 1), normal1 - normal2, 0.5));

				normal = normalize(mul(normal, tangentTransform));

				float _LightWrapping = 1.5;
				//normal = normal1;
				float nl = max(0, dot(normal, LightDir));
				float3 w = float3(_LightWrapping, _LightWrapping, _LightWrapping)*0.5;
				float3 NdotLWrap = nl * (1.0 - w);
				float3 forwardLight = max(float3(0.0, 0.0, 0.0), NdotLWrap + w);
				float3 halfway_vec = normalize(LightDir + ViewDir);
				fixed light_spec = pow(max(dot(halfway_vec, normal), 0), _Shininess * 1) * _Gloss;
				fixed3 light_spec_color = _LightColor0.rgb * light_spec;

				// diff spec
				float3 _power = pow(saturate(max(_DeepWaterColor.rgb, (_ShallowWaterColor.rgb*(saturate((sceneZ - partZ) / _ShallowDeepBlend)*-1.0 + 1.0)))), _ShallowFade);
				_power *= _LightColor0.rgb * forwardLight;// +UNITY_LIGHTMODEL_AMBIENT.rgb;
				float s = 1 - saturate(i.projPos.z / 100);
				float3 specularColor = float3(0.5, 0.5, 0.5);
				float specPow = exp2(_Gloss * 10.0 + 1.0);
				float3 specular = _LightColor0.xyz * pow(max(0, dot(halfway_vec, normal)), specPow) * 1;

				// ref
				float2 _remap = ((i.screenPos.xy + (float2(normal.r, normal.g)*0.1))*0.5 + 0.5);
				float4 _ReflectionTex_var = tex2D(_ReflectionTex, _remap);
				//_power = lerp(_ReflectionTex_var, _power, 0.5);

				float3 finalColor = _power + specular + _ReflectionTex_var;
				finalColor = _power + specular;// pow(max(0, dot(halfway_vec, normal)), specPow) * _LightColor0.rgb * specularColor;
				fixed4 finalRGBA = fixed4(finalColor, (saturate((sceneZ - partZ) / _ShoreTransparency)*pow(saturate((sceneZ - partZ) / _DepthTransparency), _ShoreFade)));
				UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
				return finalRGBA;
			}
			ENDCG
		}
	}
}
