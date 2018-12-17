
#include "HLSLSupport.cginc"
#include "TeraUtil.cginc"

float4 _MainTex_ST;
uniform fixed _Night;
uniform fixed _lmfb;
uniform float4 _SunColor;
uniform float4 _SunDir;

uniform samplerCUBE _EnvMap;

#define LINEARCOLOR(color) pow(color, 1.0 / 2.2);

struct appdata_mdd200
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
#if defined(LIGHTMAP_ON)
	float2 lmuv : TEXCOORD1;
#endif
};

struct v2f_mdd200
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 lmuv : TEXCOORD1;
	UNITY_FOG_COORDS(2)
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	half3 normalWorld : TEXCOORD3;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	SHADOW_COORDS(4)
#endif
};

// with vertex normal
struct appdata_mdd
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
#if defined(LIGHTMAP_ON)
	float2 lmuv : TEXCOORD1;
#endif
	fixed3 vcolor : COLOR0;
};

struct v2f_mdd
{
	float2 uv : TEXCOORD0;
	float2 lmuv : TEXCOORD1;
	float4 vertex : SV_POSITION;
	fixed3 vcolor : COLOR0;
#if defined(LIGHTMAP_OFF)
	fixed3 sh : COLOR1;
#endif
	UNITY_FOG_COORDS(3)
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	half3 normalWorld : TEXCOORD4;
#endif
#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON) || defined(TRANSPARENT_SCENEOBJECT)
	fixed faceUp : TEXCOORD5;
	float3 viewDir : TEXCOORD6;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	SHADOW_COORDS(7)
#endif
};

v2f_mdd vert_mobile_diffuse_dir(appdata_mdd v)
{
	v2f_mdd o = (v2f_mdd)0;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	o.vcolor = v.vcolor;

	float3 posWorld = mul(unity_ObjectToWorld, v.vertex);
	half3 normalWorld = UnityObjectToWorldNormal(v.normal);

#if defined(LIGHTMAP_ON)
	o.lmuv.xy = v.lmuv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
	
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	o.normalWorld = normalWorld;
#endif

	// SH/ambient and vertex lights
	
#ifndef LIGHTMAP_ON
	half3 sh = (half3)(0);
#if UNITY_SHOULD_SAMPLE_SH
#ifdef VERTEXLIGHT_ON
	// Approximated illumination from non-important point lights
	sh = Shade4PointLights (
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, posWorld, normalWorld);
#endif
	o.sh = ShadeSHPerVertex(normalWorld, sh);
#endif
#endif // LIGHTMAP_ON

#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON) || defined(TRANSPARENT_SCENEOBJECT)
	FaceUpFactor(o.faceUp, normalWorld);
	o.viewDir = posWorld.xyz - _WorldSpaceCameraPos;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	TERA_SHADOWUV(o)
#endif
	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}

#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)

fixed3 ShadeWithDynamicLight_mdd(v2f_mdd i,
	fixed3 diffuse,
	float3 light_color,
	half atten,
	fixed3 sh)
{
	float3 lightDir = normalize(_SunDir.xyz);
	half3 normal = i.normalWorld;
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif
#ifdef RAIN_SURFACE_ON
	half3 RippleNormal = GetRaindropRippleNormal(i.uv);
	normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
	float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif

	fixed nl = max(0, dot(normal, lightDir));
	      sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),nl);

	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;

#ifdef SNOW_SURFACE_ON
	MixDiffuseWithSnow(diffuse, i.uv, normal, i.faceUp);
#endif

#ifdef RAIN_SURFACE_ON
	float3 halfway_vec = normalize(lightDir - normalize(i.viewDir));
	half nh = saturate(dot(normal, halfway_vec));

	half light_spec = pow(nh, 1 * 128);

	float3 reflVect = normalize(reflect(-i.viewDir, normal));
	float3 reflColor = texCUBE(_EnvMap, reflVect);
	light_color *= reflColor;

	half3 p1 = max(half3(0, 0, 0), light_color * nl + sh);
	half3 p2 = max(half3(0, 0, 0), light_color * light_spec * factor * atten);
#ifdef SNOW_SURFACE_ON
	p2 *= saturate(1 - _SnowDensity * 1.6);
#endif
	return (diffuse * p1 + p2);
#else
	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
	half3 p3 = max(half3(0, 0, 0),(light_color*((nl*0.5+0.5) + 1 + _lmfb)));
	      p1 = lerp(p1,p3,saturate(_Night*step(0.01,_lmfb)));
	return max(fixed3(0.01,0.01,0.01),diffuse * p1);
#endif
}

#if defined(TRANSPARENT_SCENEOBJECT)
fixed3 ShadeWithDynamicLight_mdd_t(v2f_mdd i,
	fixed3 diffuse,
	float3 light_color,
	half atten,
	fixed3 sh)
{
	float3 lightDir = normalize(_SunDir.xyz);
	half3 normal = i.normalWorld;
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif
#ifdef RAIN_SURFACE_ON
	half3 RippleNormal = GetRaindropRippleNormal(i.uv);
	normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
	float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif

	fixed nl = max(0, dot(normal, lightDir));
	      sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),nl);

	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;
#ifdef SNOW_SURFACE_ON
	MixDiffuseWithSnow(diffuse, i.uv, normal, i.faceUp);
#endif
	
//	float3 halfway_vec = normalize(lightDir - normalize(i.viewDir));
//	half nh = max(0.4, abs(dot(normal, halfway_vec)));
//
//	half light_spec = pow(nh, 1 * 128);
//
//	float3 reflVect = normalize(reflect(-i.viewDir, normal));
//	float3 reflColor = texCUBE(_EnvMap, reflVect);
//	light_color *= reflColor;

	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
//	half3 p2 = max(half3(0, 0, 0), light_color * light_spec * atten);
#ifdef RAIN_SURFACE_ON
	p2 *= factor;
#endif
#ifdef SNOW_SURFACE_ON
	p2 *= saturate(1 - _SnowDensity * 1.6);
#endif
	return max(fixed3(0.01,0.01,0.01),diffuse * p1);
}
#endif
#endif

float4 _BumpMap_ST;

// with bump map
struct appdata_mdbd
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	float4 tangent : TANGENT;
#endif
	float2 uv : TEXCOORD0;
#if defined(LIGHTMAP_ON)
	float2 lmuv : TEXCOORD1;
#endif
};

struct v2f_mdbd
{
	float4 uv : TEXCOORD0;
	float2 lmuv : TEXCOORD1;
	float4 vertex : SV_POSITION;
	float3 normal : TEXCOORD8;
#if defined(LIGHTMAP_OFF)
	fixed3 sh : COLOR0;
#endif
	UNITY_FOG_COORDS(2)
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	half4 tangentToWorld[3]	: TEXCOORD3;
#endif
#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON)
	fixed faceUp : TEXCOORD6;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	SHADOW_COORDS(7)
#endif
};

v2f_mdbd vert_mobile_diffuse_bump_dir(appdata_mdbd v)
{
	v2f_mdbd o = (v2f_mdbd)0;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	o.uv.zw = TRANSFORM_TEX(v.uv, _BumpMap);

	float3 posWorld = mul(unity_ObjectToWorld, v.vertex);
	half3 normalWorld = UnityObjectToWorldNormal(v.normal);

#if defined(LIGHTMAP_ON)
	o.lmuv.xy = v.lmuv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
	o.tangentToWorld[0].xyz = tangentToWorld[0];
	o.tangentToWorld[1].xyz = tangentToWorld[1];
	o.tangentToWorld[2].xyz = tangentToWorld[2];

	float3 viewDir = posWorld.xyz - _WorldSpaceCameraPos;
	o.tangentToWorld[0].w = viewDir.x;
	o.tangentToWorld[1].w = viewDir.y;
	o.tangentToWorld[2].w = viewDir.z;
#endif

	// SH/ambient and vertex lights
#ifndef LIGHTMAP_ON
	half3 sh = (half3)(0);
#if UNITY_SHOULD_SAMPLE_SH
#ifdef VERTEXLIGHT_ON
	// Approximated illumination from non-important point lights
	sh = Shade4PointLights (
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, posWorld, normalWorld);
#endif
	o.sh = ShadeSHPerVertex(normalWorld, sh);
#endif
#endif // LIGHTMAP_ON

#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON)
	FaceUpFactor(o.faceUp, normalWorld);
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	TERA_SHADOWUV(o)
#endif
    o.normal = normalWorld;
	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}

sampler2D _BumpMap;
float _Shininess;
uniform fixed _Refint;

#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)

fixed3 ShadeWithDynamicLight_mdbd(v2f_mdbd i,
	fixed3 diffuse,
	float spec_factor,
	half3 light_color,
	half atten,
	fixed3 sh)
{
	
	half3 viewDir = -normalize(half3(i.tangentToWorld[0].w, i.tangentToWorld[1].w, i.tangentToWorld[2].w));
	half3 lightDir = normalize(_SunDir.xyz);

	float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
	TangentToWorld(i.tangentToWorld, normal);
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif
#ifdef RAIN_SURFACE_ON
	half3 RippleNormal = GetRaindropRippleNormal(i.uv);
	normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
	float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif

#ifdef SNOW_SURFACE_ON
	MixDiffuseWithSnow(diffuse, i.uv, normal, i.faceUp);
#endif

	float3 halfway_vec = normalize(lightDir + viewDir);
	
	half nl = saturate(dot(normal, lightDir));
	half nh = saturate(dot(normal, halfway_vec));
	     sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),nl);
	half light_spec = pow(nh, _Shininess * 128) * spec_factor;

	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;

#ifdef ENVMAP_LIGHTING
    half nv = saturate(1 - dot(normal, viewDir));
	float3 reflVect = normalize(reflect(-viewDir, normal));
	float3 reflColor = texCUBE(_EnvMap, reflVect);
	       reflColor = max(0.01,lerp(fixed3(0.25,0.25,0.25),reflColor,_Refint));
	light_color += reflColor * _Refint * nv * 2;
#endif

	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
	half3 p3 = max(half3(0, 0, 0),(light_color*((nl*0.5+0.5) + 1 + _lmfb)));
	      p1 = lerp(p1,p3,saturate(_Night*step(0.01,_lmfb)));
	half3 p2 = max(half3(0, 0, 0), (diffuse*nl + light_color) * 0.5 * light_spec * atten);
#ifdef RAIN_SURFACE_ON
	p2 *= factor;
#endif
#ifdef SNOW_SURFACE_ON
	p2 *= saturate(1 - _SnowDensity * 1.6);
#endif

	fixed3 final_color = diffuse * p1 + p2;
	return max(fixed3(0.01,0.01,0.01),final_color);
}

sampler2D _Sptex;
uniform fixed _reflint;
uniform fixed _reflsat;
uniform int _reftype;
uniform int _fnl;

fixed3 ShadeWithDynamicLight_mdbdr(v2f_mdbd i,
	fixed3 diffuse,
	float spec_factor,
	half3 light_color,
	half atten,
	fixed3 sh)
//	sampler2D spmap)
{
	
	half3 viewDir = -normalize(half3(i.tangentToWorld[0].w, i.tangentToWorld[1].w, i.tangentToWorld[2].w));
	half3 lightDir = normalize(_SunDir.xyz);

	float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
	TangentToWorld(i.tangentToWorld, normal);
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif
#ifdef RAIN_SURFACE_ON
	half3 RippleNormal = GetRaindropRippleNormal(i.uv);
	normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
	float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif

#ifdef SNOW_SURFACE_ON
	MixDiffuseWithSnow(diffuse, i.uv, normal, i.faceUp);
#endif

	float3 halfway_vec = normalize(lightDir + viewDir);
	
	half nl = saturate(dot(normal, lightDir));
	half nh = saturate(dot(normal, halfway_vec));
	     sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),nl);
//	half nv = saturate(dot(normal, viewDir));
//	half nvr = saturate(dot(reflect(-lightDir,normal),viewDir));
	half4 Specularmap = tex2D(_Sptex,i.uv.xy);

	half light_spec = pow(nh, Specularmap.a * 128);
//	     light_spec += pow(nv, Specularmap.a * 128) * (1-light_spec);
	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;

	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
	half3 p3 = max(half3(0, 0, 0),(light_color*((nl*0.5+0.5) + 1 + _lmfb)));
	      p1 = lerp(p1,p3,saturate(_Night*step(0.01,_lmfb)));
	half3 p2 = max(half3(0, 0, 0), (diffuse + light_color) * 0.5 * light_spec * atten);
	      
#ifdef ENVMAP_LIGHTING
    half nv = clamp(1 - dot(normal, viewDir),0.04,1);
         nv = lerp(1,nv,_fnl);
//    half nv = clamp(dot(normal, viewDir),0.04,1);
	float3 reflVect = normalize(reflect(-viewDir, normalize(i.normal)));
	float3 refVect = normalize(reflect(-viewDir, normal));
	float refpow = max(0,exp(Specularmap.a*5-3)*0.14);
    float ref = lerp(3,1,refpow);
	float3 reflColor = texCUBElod(_EnvMap, half4(lerp(reflVect,refVect,_reftype),ref));
	       reflColor = max(0.01,lerp(fixed3(0.25,0.25,0.25),reflColor,_Refint));
	half refpower = ((reflColor.r*0.299)+(reflColor.g*0.587)+(reflColor.b*0.114));
	half3 p4 = pow(reflColor * Specularmap.a,1+_reflsat);
//	half3 p4 = pow(reflColor * Specularmap.a,lerp(1,1.5+_reflint,nv));
	      p4 *= light_color * refpower * (_reflint)*nv;
	      p4 *= 1-p2*0.75;
	p2 += p4;
#endif
#ifdef RAIN_SURFACE_ON
	p2 *= factor;
#endif
#ifdef SNOW_SURFACE_ON
	p2 *= saturate(1 - _SnowDensity * 1.6);
#endif

	fixed3 final_color = diffuse * p1 + p2;
	return clamp(final_color,fixed3(0.01,0.01,0.01),fixed3(1,1,1));
}

#if defined(TRANSPARENT_SCENEOBJECT)
fixed4 ShadeWithDynamicLight_mdbd_t(v2f_mdbd i,
	fixed4 diffuse,
	float3 light_color,
	half spec_factor,
	half atten,
	fixed3 sh)
{
	
	half3 viewDir = -normalize(half3(i.tangentToWorld[0].w, i.tangentToWorld[1].w, i.tangentToWorld[2].w));
	half3 lightDir = normalize(_SunDir.xyz);

	float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
	TangentToWorld(i.tangentToWorld, normal);
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif
#ifdef RAIN_SURFACE_ON
	half3 RippleNormal = GetRaindropRippleNormal(i.uv);
	normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
	float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif

#ifdef SNOW_SURFACE_ON
	MixDiffuseWithSnow(diffuse.rgb, i.uv, normal, i.faceUp);
#endif

	float3 halfway_vec = normalize(lightDir + viewDir);
	
	half nl = saturate(dot(normal, lightDir));
	half nh = saturate(dot(normal, halfway_vec));
	     sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),nl);
	half light_spec = pow(nh, _Shininess * 128) * spec_factor;
	half alpha = diffuse.a;

	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;
	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
	half3 p3 = max(half3(0, 0, 0),(light_color*((nl*0.5+0.5) + 1 + _lmfb)));
	      p1 = lerp(p1,p3,saturate(_Night*step(0.01,_lmfb)));
	half3 p2 = max(half3(0, 0, 0), aa_light_color * light_spec * atten);
	fixed3 finalcolor = (diffuse.rgb * p1 + p2);
#ifdef ENVMAP_LIGHTING
    half nv = saturate(1 - dot(normal, viewDir));
	float3 reflVect = normalize(reflect(-viewDir, normal));
	float3 reflColor = texCUBE(_EnvMap, reflVect);
	alpha = pow(dot(reflColor.rgb,float3(0.3,0.59,0.11)),2) + light_spec + diffuse.a;
	reflColor *= _Refint;
	light_spec += reflColor;
	finalcolor *= reflColor;
#endif
#ifdef RAIN_SURFACE_ON
	p2 *= factor;
#endif
#ifdef SNOW_SURFACE_ON
	p2 *= saturate(1 - _SnowDensity * 1.6);
#endif
	return fixed4(max(fixed3(0.01,0.01,0.01),finalcolor),alpha);
}
#endif

fixed3 ShadeWithDynamicLight_mdbdsnow(v2f_mdbd i,
    half4 _coverdir,
    fixed3 snowcolor,
	fixed3 diffuse,
	float spec_factor,
	half3 light_color,
	half atten,
	fixed3 sh)
{
	
	half3 viewDir = -normalize(half3(i.tangentToWorld[0].w, i.tangentToWorld[1].w, i.tangentToWorld[2].w));
	half3 lightDir = normalize(_SunDir.xyz);
	half3 coverDir = normalize(_coverdir.xyz);

	float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
	TangentToWorld(i.tangentToWorld, normal);
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif
#ifdef RAIN_SURFACE_ON
	half3 RippleNormal = GetRaindropRippleNormal(i.uv);
	normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
	float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif

#ifdef SNOW_SURFACE_ON
	MixDiffuseWithSnow(diffuse, i.uv, normal, i.faceUp);
#endif

	float3 halfway_vec = normalize(lightDir + viewDir);
	
	half ns = saturate(dot(normal, coverDir));
	half nl = saturate(dot(normal, lightDir));
	half nh = saturate(dot(normal, halfway_vec));
	     sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),nl);
	half light_spec = pow(nh, _Shininess * 128) * spec_factor;

	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;

#ifdef ENVMAP_LIGHTING
	float3 reflVect = normalize(reflect(-viewDir, normal));
	float3 reflColor = texCUBE(_EnvMap, reflVect);
	light_color *= reflColor;
#endif

	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
	half3 p3 = max(half3(0, 0, 0),(light_color*((nl*0.5+0.5) + 1 + _lmfb)));
	      p1 = lerp(p1,p3,saturate(_Night*step(0.01,_lmfb)));
	half3 p2 = max(half3(0, 0, 0), (diffuse + light_color) * 0.5 * light_spec * nl * atten);
#ifdef RAIN_SURFACE_ON
	p2 *= factor;
#endif
#ifdef SNOW_SURFACE_ON
	p2 *= saturate(1 - _SnowDensity * 1.6);
#endif

	fixed3 final_color = lerp(diffuse,snowcolor,ns) * p1 + p2;
	return max(fixed3(0.01,0.01,0.01),final_color);
}
#endif

struct appdata_mdbdn
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	float4 tangent : TANGENT;
#endif
	float2 uv : TEXCOORD0;
#if defined(LIGHTMAP_ON)
	float2 lmuv : TEXCOORD1;
#endif
    fixed4 lt : COLOR0;
};

struct v2f_mdbdn
{
	float4 uv : TEXCOORD0;
	float2 lmuv : TEXCOORD1;
	float4 vertex : SV_POSITION;
	fixed4 lt : COLOR0;
#if defined(LIGHTMAP_OFF)
	fixed3 sh : COLOR1;
#endif
	UNITY_FOG_COORDS(2)
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	half4 tangentToWorld[3]	: TEXCOORD3;
#endif
#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON)
	fixed faceUp : TEXCOORD6;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	SHADOW_COORDS(7)
#endif
};

v2f_mdbdn vert_mobile_diffuse_bump_dir_partnight(appdata_mdbdn v)
{
	v2f_mdbdn o = (v2f_mdbdn)0;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	o.uv.zw = TRANSFORM_TEX(v.uv, _BumpMap);

	float3 posWorld = mul(unity_ObjectToWorld, v.vertex);
	half3 normalWorld = UnityObjectToWorldNormal(v.normal);

#if defined(LIGHTMAP_ON)
	o.lmuv.xy = v.lmuv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
	o.tangentToWorld[0].xyz = tangentToWorld[0];
	o.tangentToWorld[1].xyz = tangentToWorld[1];
	o.tangentToWorld[2].xyz = tangentToWorld[2];

	float3 viewDir = posWorld.xyz - _WorldSpaceCameraPos;
	o.tangentToWorld[0].w = viewDir.x;
	o.tangentToWorld[1].w = viewDir.y;
	o.tangentToWorld[2].w = viewDir.z;
#endif

	// SH/ambient and vertex lights
#ifndef LIGHTMAP_ON
	half3 sh = (half3)(0);
#if UNITY_SHOULD_SAMPLE_SH
#ifdef VERTEXLIGHT_ON
	// Approximated illumination from non-important point lights
	sh = Shade4PointLights (
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, posWorld, normalWorld);
#endif
	o.sh = ShadeSHPerVertex(normalWorld, sh);
#endif
#endif // LIGHTMAP_ON

#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON)
	FaceUpFactor(o.faceUp, normalWorld);
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	TERA_SHADOWUV(o)
#endif
    o.lt = v.lt;
//    fixed lighted = o.lt.a;
	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}

#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)

fixed3 ShadeWithDynamicLight_mdbdn(v2f_mdbdn i,
	fixed3 diffuse,
	float spec_factor,
	half3 light_color,
	half atten,
	fixed3 sh,
	sampler2D night_mask)
{
//	fixed mask = tex2D(night_mask,i.uv2).r;
	fixed mask = i.lt.a;
	half3 viewDir = -normalize(half3(i.tangentToWorld[0].w, i.tangentToWorld[1].w, i.tangentToWorld[2].w));
	half3 lightDir = normalize(_SunDir.xyz);

	float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
	TangentToWorld(i.tangentToWorld, normal);
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif
#ifdef RAIN_SURFACE_ON
	half3 RippleNormal = GetRaindropRippleNormal(i.uv);
	normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
	float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif

#ifdef SNOW_SURFACE_ON
	MixDiffuseWithSnow(diffuse, i.uv, normal, i.faceUp);
#endif

	float3 halfway_vec = normalize(lightDir + viewDir);
	
	half nl = saturate(dot(normal, lightDir));
	half nh = saturate(dot(normal, halfway_vec));
	     sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),nl);
	half light_spec = pow(nh, _Shininess * 128) * spec_factor;

	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;

#ifdef ENVMAP_LIGHTING
    half nv = saturate(1 - dot(normal, viewDir));
	float3 reflVect = normalize(reflect(-viewDir, normal));
	float3 reflColor = texCUBE(_EnvMap, reflVect);
	light_color *= reflColor * _Refint * nv * 2;
#endif

	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
	half3 p3 = max(half3(0, 0, 0),(light_color*((nl*0.5+0.5) + 1 + _lmfb)));
	      p1 = lerp(p1,p3,saturate(_Night*step(0.01,_lmfb)*mask));
	half3 p2 = max(half3(0, 0, 0), (diffuse*nl + light_color) * 0.5 * light_spec * atten);
#ifdef RAIN_SURFACE_ON
	p2 *= factor;
#endif
#ifdef SNOW_SURFACE_ON
	p2 *= saturate(1 - _SnowDensity * 1.6);
#endif

	fixed3 final_color = diffuse * p1 + p2;
	return max(fixed3(0.01,0.01,0.01),final_color);
}
#endif

v2f_mdd200 vert_200(appdata_mdd200 v)
{
	v2f_mdd200 o = (v2f_mdd200)0;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);

#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	o.normalWorld = UnityObjectToWorldNormal(v.normal);
#endif

#if defined(LIGHTMAP_ON)
	o.lmuv.xy = v.lmuv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif

#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	TERA_SHADOWUV(o)
#endif
	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}

#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)

fixed3 ShadeWithLevel200(v2f_mdd200 i,
	fixed3 diffuse,
	float3 light_color,
	fixed3 sh = fixed3(1, 1, 1))
{
	float3 lightDir = normalize(_SunDir.xyz);
	half3 normal = i.normalWorld;

#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	fixed atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif

	fixed nl = max(0, dot(normal, lightDir));
	      sh = lerp(sh, dot(sh,float3(0.3, 0.59, 0.11)), nl);

	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;
	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
	half3 p3 = max(half3(0, 0, 0),(light_color*((nl*0.5+0.5) + 1 + _lmfb)));
	      p1 = lerp(p1,p3,saturate(_Night*step(0.01,_lmfb)));
	return max(fixed3(0.01,0.01,0.01),diffuse * p1);
}

#endif


// with vertex animation
vector _Wind;
float _Windnoise;
float _Windfreq;
vector _Disturb;

struct appdata_mddva
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
#if defined(LIGHTMAP_ON)
	float2 lmuv : TEXCOORD1;
#endif
	fixed4 vcolor : COLOR;
};

struct v2f_mddva
{
	float2 uv : TEXCOORD0;
	float2 lmuv : TEXCOORD1;
	float4 vertex : SV_POSITION;
	fixed4 vcolor : COLOR;
#if defined(LIGHTMAP_OFF)
	fixed3 sh : COLOR1;
#endif
	UNITY_FOG_COORDS(3)
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	half3 normalWorld : TEXCOORD4;
#endif
#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON) || defined(TRANSPARENT_SCENEOBJECT)
	fixed faceUp : TEXCOORD5;
	float3 viewDir : TEXCOORD6;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	SHADOW_COORDS(7)
#endif
};

//modifered from TerrianEngine.cginc
inline float4 AnimateVertex2(float4 pos, float3 normal, float4 animParams,float4 wind,float4 _Disturb,float2 time)
{	
	float fDetailAmp = 0.1f;
	float fBranchAmp = 0.3f;

	float fObjPhase = dot(unity_ObjectToWorld[3].xyz, 1);
	float fBranchPhase = fObjPhase + animParams.x;
	
	float fVtxPhase = dot(pos.xyz, animParams.y + fBranchPhase);

	float2 vWavesIn = time  + float2(fVtxPhase, fBranchPhase );

	float4 vWaves = (frac( vWavesIn.xxyy * float4(1.975, 1.758, 0.375+_Disturb.z, 0.193+_Disturb.w) ) * 2.0 - 1.0);
	vWaves = abs( frac(vWaves+0.5) * 2 -1);
	vWaves = vWaves * vWaves * (3.0 - 2.0 * vWaves);
	float2 vWavesSum = vWaves.xz + vWaves.yw;

	float3 bend = animParams.y * fDetailAmp * normal.xyz;
//	bend.y = animParams.w * fBranchAmp;
	bend.y = 0;
	pos.xyz += ((vWavesSum.xyx * bend) + (wind.xyz * vWavesSum.y * animParams.w)) * wind.w; 

	pos.xyz += animParams.z * wind.xyz;
	
	return pos;
}

v2f_mddva vert_mobile_diffuse_dirva(appdata_mddva v)
{
	v2f_mddva o = (v2f_mddva)0;

	float4 wind;
		
	float bendingFact = v.vcolor.a;

	wind.xyz = mul((float3x3)unity_WorldToObject,_Wind.xyz);
	wind.w = _Wind.w * bendingFact;
	float4 windParams = float4(0,_Windnoise+_Disturb.x,bendingFact.xx);
	float windTime = _Time.y * float2(_Windfreq+_Disturb.y,1);
	float4 anmdpos = AnimateVertex2(v.vertex,v.normal,windParams,wind,_Disturb,windTime);

	o.vertex = UnityObjectToClipPos(anmdpos);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	o.vcolor = v.vcolor;

	float3 posWorld = mul(unity_ObjectToWorld, v.vertex);
	half3 normalWorld = UnityObjectToWorldNormal(v.normal);

#if defined(LIGHTMAP_ON)
	o.lmuv.xy = v.lmuv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
	
#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)
	o.normalWorld = normalWorld;
#endif

	// SH/ambient and vertex lights
	
#ifndef LIGHTMAP_ON
	half3 sh = (half3)(0);
#if UNITY_SHOULD_SAMPLE_SH
#ifdef VERTEXLIGHT_ON
	// Approximated illumination from non-important point lights
	sh = Shade4PointLights (
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, posWorld, normalWorld);
#endif
	o.sh = ShadeSHPerVertex(normalWorld, sh);
#endif
#endif // LIGHTMAP_ON

#if defined(RAIN_SURFACE_ON) || defined(SNOW_SURFACE_ON) || defined(TRANSPARENT_SCENEOBJECT)
	FaceUpFactor(o.faceUp, normalWorld);
	o.viewDir = posWorld.xyz - _WorldSpaceCameraPos;
#endif
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	TERA_SHADOWUV(o)
#endif
	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}

#if defined(MOBILE_DYNAMIC_DIRLIGHT_ON) || !defined(LIGHTMAP_ON)

fixed3 ShadeWithDynamicLight_mddva(v2f_mddva i,
	fixed3 diffuse,
	float3 light_color,
	half atten,
	fixed3 sh)
{
	float3 lightDir = normalize(_SunDir.xyz);
	half3 normal = i.normalWorld;
#if defined(SHADOWS_NATIVE) || defined(SHADOWS_SCREEN)
	atten = SHADOW_ATTENUATION(i);
	light_color *= atten;
	sh *= atten;
#endif
#ifdef RAIN_SURFACE_ON
	half3 RippleNormal = GetRaindropRippleNormal(i.uv);
	normal = BlendNormalWithRaindrop(normal, RippleNormal, 1 - i.faceUp);
	float factor = lerp(1, 1 - _RainParamters.z, _RainParamters.x);
#endif

	fixed nl = max(0, dot(normal, lightDir));
	      sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),nl);

	fixed3 aa_light_color = light_color * _SunColor.rgb * _SunColor.a;

#ifdef SNOW_SURFACE_ON
	MixDiffuseWithSnow(diffuse, i.uv, normal, i.faceUp);
#endif

#ifdef RAIN_SURFACE_ON
	float3 halfway_vec = normalize(lightDir - normalize(i.viewDir));
	half nh = saturate(dot(normal, halfway_vec));

	half light_spec = pow(nh, 1 * 128);

	float3 reflVect = normalize(reflect(-i.viewDir, normal));
	float3 reflColor = texCUBE(_EnvMap, reflVect);
	light_color *= reflColor;

	half3 p1 = max(half3(0, 0, 0), light_color * nl + sh);
	half3 p2 = max(half3(0, 0, 0), light_color * light_spec * factor * atten);
#ifdef SNOW_SURFACE_ON
	p2 *= saturate(1 - _SnowDensity * 1.6);
#endif
	return (diffuse * p1 + p2);
#else
	half3 p1 = max(half3(0, 0, 0), aa_light_color * nl + (sh*0.5) + (sh*0.5*light_color));
	half3 p3 = max(half3(0, 0, 0),(light_color*((nl*0.5+0.5) + 1 + _lmfb)));
	      p1 = lerp(p1,p3,saturate(_Night*step(0.01,_lmfb)));
	return max(fixed3(0.01,0.01,0.01),(diffuse * p1));
#endif
//    return ShadeWithDynamicLight_mdd(v2f_mddva i,diffuse,light_color,atten,sh)
}
#endif
