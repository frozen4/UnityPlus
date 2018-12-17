Shader "Nature/Terrain/Standard" {
	Properties {
		// set by terrain engine
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
		[HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
		[HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
		[HideInInspector] [Gamma] _Metallic0 ("Metallic 0", Range(0.0, 1.0)) = 0.0	
		[HideInInspector] [Gamma] _Metallic1 ("Metallic 1", Range(0.0, 1.0)) = 0.0	
		[HideInInspector] [Gamma] _Metallic2 ("Metallic 2", Range(0.0, 1.0)) = 0.0	
		[HideInInspector] [Gamma] _Metallic3 ("Metallic 3", Range(0.0, 1.0)) = 0.0
		[HideInInspector] _Smoothness0 ("Smoothness 0", Range(0.0, 1.0)) = 1.0	
		[HideInInspector] _Smoothness1 ("Smoothness 1", Range(0.0, 1.0)) = 1.0	
		[HideInInspector] _Smoothness2 ("Smoothness 2", Range(0.0, 1.0)) = 1.0	
		[HideInInspector] _Smoothness3 ("Smoothness 3", Range(0.0, 1.0)) = 1.0

		// used in fallback on old cards & base map
		[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader {
		Tags {
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}

		Pass
		{
			Tags { "LIGHTMODE" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile TERRAIN_NORMAL_ON TERRAIN_NORMAL_OFF
			#pragma multi_compile ENVMAP_LIGHTING_ON ENVMAP_LIGHTING_OFF
			#pragma multi_compile RAIN_SURFACE_OFF RAIN_SURFACE_ON
			#pragma multi_compile SNOW_SURFACE_OFF SNOW_SURFACE_ON

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "TeraUtil.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;

			half _Metallic0;
			half _Metallic1;
			half _Metallic2;
			half _Metallic3;

			half _Smoothness0;
			half _Smoothness1;
			half _Smoothness2;
			half _Smoothness3;

			sampler2D _Control;
			float4 _Control_ST;

			sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
			float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

			sampler2D _Normal0, _Normal1, _Normal2, _Normal3;

			uniform float4 _SunColor;
			uniform float4 _SunDir;
			uniform float4 _SunAmbientColor;
			uniform samplerCUBE _EnvMap;
			uniform fixed _EnvLodBias;

			struct appdata_terrain
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f_terrain
			{
				float4 vertex : SV_POSITION;
				float4 pack0 : TEXCOORD0; // _Splat0 _Splat1
				float4 pack1 : TEXCOORD1; // _Splat2 _Splat3
				float4 tc_Control : TEXCOORD2;// blend lm
#if defined(LIGHTMAP_OFF)
				fixed3 sh : COLOR0;
#endif
				UNITY_FOG_COORDS(3)
#if defined(TERRAIN_NORMAL_ON)
				half4 tangentToWorld[3]	: TEXCOORD4;
#else
				half3 normalWorld : TEXCOORD7;
				float3 viewDir : TEXCOORD8;
#endif
#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON)
				fixed faceUp : TEXCOORD9;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
				SHADOW_COORDS(10)
#endif
			};

			v2f_terrain vert(appdata_terrain v)
			{
				v2f_terrain o;
				UNITY_INITIALIZE_OUTPUT(v2f_terrain, o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.tc_Control.xy = TRANSFORM_TEX(v.texcoord, _Control);
#if defined(LIGHTMAP_ON)
				o.tc_Control.zw = v.texcoord1 * unity_LightmapST.xy + unity_LightmapST.zw;;
#endif
				o.pack0.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
				o.pack0.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
				o.pack1.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
				o.pack1.zw = TRANSFORM_TEX(v.texcoord, _Splat3);

				float4 tangent;
				tangent.xyz = cross(v.normal, float3(0, 0, 1));
				tangent.w = -1;
				float3 posWorld = mul(unity_ObjectToWorld, v.vertex);
				half3 normalWorld = UnityObjectToWorldNormal(v.normal);
#if defined(TERRAIN_NORMAL_ON)
				float4 tangentWorld = float4(UnityObjectToWorldDir(tangent.xyz), tangent.w);
				float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
				o.tangentToWorld[0].xyz = tangentToWorld[0];
				o.tangentToWorld[1].xyz = tangentToWorld[1];
				o.tangentToWorld[2].xyz = tangentToWorld[2];

				float3 viewDir = posWorld.xyz - _WorldSpaceCameraPos;
				o.tangentToWorld[0].w = viewDir.x;
				o.tangentToWorld[1].w = viewDir.y;
				o.tangentToWorld[2].w = viewDir.z;
#else
				o.normalWorld = normalWorld;
				o.viewDir = posWorld.xyz - _WorldSpaceCameraPos;
#endif
// SH/ambient and vertex lights
#ifdef LIGHTMAP_OFF
#if UNITY_SHOULD_SAMPLE_SH
				o.sh = 0;
				o.sh = ShadeSHPerVertex(normalWorld, o.sh);
#endif
#endif // LIGHTMAP_OFF
#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON)
				FaceUpFactor(o.faceUp, normalWorld);
#endif
				UNITY_TRANSFER_FOG(o, o.vertex);
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
				TERA_SHADOWUV(o)
#endif
				return o;
			}

			fixed4 frag(v2f_terrain i) : SV_Target
			{
				fixed4 final_color = fixed4(0, 0, 0, 1);

#if defined(TERRAIN_NORMAL_ON)
				half3 viewDir = -normalize(half3(i.tangentToWorld[0].w, i.tangentToWorld[1].w, i.tangentToWorld[2].w));
#else
				half3 viewDir = -normalize(i.viewDir);
#endif
				half3 lightDir = normalize(_SunDir.xyz);
				
				half4 splat_control = tex2D(_Control, i.tc_Control.xy);

				fixed4 diffuse = splat_control.r * tex2D(_Splat0, i.pack0.xy);
				diffuse += splat_control.g * tex2D(_Splat1, i.pack0.zw);
				diffuse += splat_control.b * tex2D(_Splat2, i.pack1.xy);
				diffuse += splat_control.a * tex2D(_Splat3, i.pack1.zw);

				half3 light_color = _SunColor.rgb * _SunColor.a;
				half3  debug_light_color = light_color;
				half3 p1 = half3(1, 1, 1);
				half3 p2 = half3(0, 0, 0);
				float atten = 1;
				half3 lmcolor = decodelightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.tc_Control.zw));
#if defined(LIGHTMAP_ON)
                //light_color *= decodelightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.tc_Control.zw));
				light_color *= lmcolor;
				fixed3 sh = _SunAmbientColor.rgb * _SunAmbientColor.a;
#else
				fixed3 sh = i.sh;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
				atten = SHADOW_ATTENUATION(i);
				light_color *= atten;
				sh *= atten;
#endif
#if defined(TERRAIN_NORMAL_ON)
				half3 normal;
				half4 nrm = 0.0f;
				nrm += splat_control.r * tex2D(_Normal0, i.pack0.xy);
				nrm += splat_control.g * tex2D(_Normal1, i.pack0.zw);
				nrm += splat_control.b * tex2D(_Normal2, i.pack1.xy);
				nrm += splat_control.a * tex2D(_Normal3, i.pack1.zw);
				normal = UnpackNormal(nrm);
				TangentToWorld(i.tangentToWorld, normal);
#else
				half3 normal = i.normalWorld;
#endif
				fixed nl = max(0, dot(normal, lightDir));
				//p1 = nl * light_color + sh ;
				half3 sh_final = (sh*0.5) + (sh*0.5*lmcolor);
				p1 = nl * light_color + sh_final;
#if defined(ENVMAP_LIGHTING_ON)
				float3 reflVect = normalize(reflect(-viewDir, normal));
				float3 reflColor = texCUBElod(_EnvMap, half4(reflVect, _EnvLodBias));
				half Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
				light_color *= reflColor * Metallic;
#endif
				
#ifdef SNOW_SURFACE_ON
				MixDiffuseWithSnow(diffuse.rgb, i.pack0.xy, normal, i.faceUp);
#endif
#if defined(TERRAIN_NORMAL_ON) || defined(RAIN_SURFACE_ON)
#if defined(RAIN_SURFACE_ON)
				half3 RippleNormal = GetRaindropRippleNormal(i.pack0.xy);
				normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
				float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif
				float3 halfway_vec = normalize(lightDir + viewDir);
				half nh = saturate(dot(normal, halfway_vec));
				half light_spec = pow(nh, 1 * 128);

				//half Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
				p2 = light_color * light_spec * diffuse.a;
#if defined(RAIN_SURFACE_ON)
				p2 *= factor;
#endif
#if defined(SNOW_SURFACE_ON)
				p2 *= saturate(1 - _SnowDensity * 1.6);
#endif
#endif
				final_color.rgb = diffuse.rgb * p1 + p2;
				UNITY_APPLY_FOG(i.fogCoord, final_color);
				return final_color;
			}
			ENDCG
		}
	}

	Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Standard-AddPass"
	Dependency "BaseMapShader" = "Hidden/TerrainEngine/Splatmap/Standard-Base"

	Fallback "Nature/Terrain/Diffuse"
}
