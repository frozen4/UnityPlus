Shader "TERA/Character/Heroic_Face" {
        Properties {
        _BaseRGBA ("Base(RGBA)", 2D) = "grey" {}
        _SkinColor ("Skin Color Custom", Color) = (0,0.667,0.667,1)
        _Skinems ("Skin Ambient Bounce", Color) = (0,0,0,1)
        _Skinedge ("Skin Edge Color", Color) = (1,1,1,1)
        _SkinDirpow ("Skin Bounce", Range(0, 0.5)) = 0.3
        _se ("Skin Emission", Range(0,2)) = 0.3
        _Eyecolor ("Eye color Custom", Color) = (0,0.667,0.667,1)
        _Normalmap ("Normalmap", 2D) = "bump" {}
        _Normalscale ("Normalscale", Range(1,5)) = 1
        _SpecularmapRGBA ("Specularmap(RGBA)", 2D) = "white" {}
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1.5
        _MatMask ("Mat Mask(RGB)(R_Skin)(G_Eye)(B_null)(A_Transmision)", 2D) = "red" {}
        _SkinAmbientBounce ("Skin Ambient Bounce", Range(0, 0.5)) = 0
        _Brdfmap ("Brdfmap", 2D) = "white" {}
        _brdfrange("Brdfmap Effective Range", Range(0,1)) = 1
        _skinpoint("Skin PointLight Intensity", Range(0,1)) = 1
        _ao ("tester", Range(0,1)) = 0
        _lf ("SkinHeadHighlight", Range(0,2)) = 1
        _headlight ("HeadLighting", Range(0,1)) = 0
        _OriginOffset ("Origin Position Offset", Vector) = (0, 0.125, 0)
        _TransmissionColor ("TransmissionColor ", Color) = (0.25,0,0,1)
//		_TransmissionRangeAdj ("TransmissionRangeAdj", Vector) = (0.75, -0.1, 1.5, 0.1)
//		_TransmissionPointPower("TransmissionPointPower", Range(0, 1)) = 0.25
		_TransmissionRange("TransmissionRange", Range(-1, 1)) = 0
		_normalblend("Blend With OriginNormal", Range(0,1)) = 0
		_ssspower("SSSPower", Range(0,1)) = 0
		_sssmerge("SSSMerge", Range(0,1)) = 0
        [Header(RimEffect)]
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0, 5)) = 0
//        [Header(tester)]
//        [MaterialToggle] _spchk("specular check",Range(0,1)) = 0
//        [MaterialToggle] sks ("Skinswitcher",float) = 0
//        [MaterialToggle] _spchk1("specular check",Range(0,1)) = 0
//        [MaterialToggle] _spchk2("Skinswitcher",float) = 0
//        [MaterialToggle] _spchk3("specular check",Range(0,1)) = 0
//        [MaterialToggle] _spchk4("Skinswitcher",float) = 0
    }
    SubShader {
        Tags {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest-50"
            "DisableBatching"="True"
        }
        LOD 400
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Zwrite On
            
            CGPROGRAM
            #pragma vertex vert_tera_pbr
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
//	        #include "3rd/UniqueShadows/Features/UniqueShadow/UniqueShadow_ShadowSample.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
			#include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "TeraPBRLighting.cginc"

            uniform float4 _LightColor0;
            uniform sampler2D _BaseRGBA; uniform float4 _BaseRGBA_ST;
            uniform fixed _RimPower;
            uniform fixed4 _RimColor;
            uniform fixed4 _SkinColor;
            uniform fixed4 _Eyecolor;
            uniform fixed4 _Skinems;
            uniform fixed _SkinDirpow;
            uniform fixed _Normalscale;
            uniform fixed _skinpoint;
            uniform float _brdfrange;
            uniform float _ao;
            uniform float _lf;
            uniform fixed _se;
            uniform half _headlight;
            uniform sampler2D _MatMask; uniform float4 _MatMask_ST;
            uniform sampler2D _Normalmap; uniform float4 _Normalmap_ST;
            uniform sampler2D _SpecularmapRGBA; uniform float4 _SpecularmapRGBA_ST;
            uniform sampler2D _Brdfmap; uniform float4 _Brdfmap_ST;
            uniform fixed _SpecularIntensity;
            uniform fixed _SkinAmbientBounce;

            uniform fixed4 _TransmissionColor;
			uniform fixed4 _TransmissionRangeAdj;
//			uniform fixed _TransmissionPointPower;
			uniform fixed _TransmissionRange;
			uniform fixed _normalblend;
			uniform fixed _ssspower;
			uniform fixed _sssmerge;
			uniform fixed _spchk;
			uniform fixed _spchk1;
			uniform fixed _spchk2;
			uniform fixed _spchk3;
			uniform fixed _spchk4;
			uniform fixed4 _Skinedge;
			uniform int sks;
            uniform float4 _SunDirchar;
//            uniform fixed4 _SunColorchar;

            half4 frag(V2f_TeraPBR i) : COLOR {
/////// Texture:
                float4 _BaseRGBA_var = tex2D(_BaseRGBA,TRANSFORM_TEX(i.uv0, _BaseRGBA));
                clip(_BaseRGBA_var.a - 0.5);
                float4 _SpecularmapRGBA_var = tex2D(_SpecularmapRGBA,TRANSFORM_TEX(i.uv0, _SpecularmapRGBA));
                float4 _MatMask_var = tex2D(_MatMask,TRANSFORM_TEX(i.uv0, _MatMask));
                fixed noskin = saturate(_MatMask_var.g*10)+_MatMask_var.r;
// Geometrydata:
                i.normalDir = normalize(i.normalDir);
                float3 objectOrigin = mul(unity_ObjectToWorld, float4(0,0,0,1));
		        float3 originNormal = normalize(i.posWorld.xyz  - objectOrigin.xyz + _OriginOffset.xyz);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 viewDir = normalize(float3(viewDirection.x,0,viewDirection.z));
                float3 _Normalmap_var = UnpackNormal(tex2Dlod(_Normalmap,float4(TRANSFORM_TEX(i.uv0, _Normalmap),0.0,_Normalscale)));
                float3 _Normalyeye = UnpackNormal(tex2D(_Normalmap,TRANSFORM_TEX(i.uv0, _Normalmap)));
                float3 normalLocal = (_Normalmap_var.rgb * _MatMask_var.r) +(_Normalyeye * (1-_MatMask_var.r));
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 blurrednormal = lerp(normalDirection,i.originNormal,_normalblend);//*i.vcolor.a);
                float3 viewReflectDirection = reflect(-viewDirection, normalDirection);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightDir = normalize(float3(_WorldSpaceLightPos0.x,0,_WorldSpaceLightPos0.z));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
                float3 ahalfDirection = normalize(viewDirection-lightDirection);
                float3 halfDir = normalize(viewDirection+lightDir);

                float3 charSunDirection = normalize(_SunDirchar1.xyz);
//                float3 halfsun = normalize(viewDirection+SunDirection);
                float3 halfsunchar = normalize(viewDirection+charSunDirection);

////// Lighting:
//                float attenuation = LIGHT_ATTENUATION(i);
                float attenuation = 1;
///// Lightness:
                float lightpower = ((lightColor.r*0.299)+(lightColor.g*0.587)+(lightColor.b*0.114));
                float emispower = saturate(lightpower);
                float NdotL = max(-_brdfrange, dot( normalDirection, lightDirection ));
                float Ndoti = max(-_brdfrange, dot( i.normalDir, lightDirection ));
                float NdotLD = max(0, dot( normalDirection, lightDir ));
                float Ndotv = max(0, dot( normalDirection, viewDirection ));
//                float NdotLD = max(0, dot( normalDirection, lightDir ));
                float NdotO = max(0, dot( blurrednormal, lightDirection));
//                float VdotO = max(0, dot( blurrednormal, viewDirection));
                float VdotO = max(0,lerp(0.5,pow(Ndotv,0.75),0.5));
//                float _NdotL = max(0, dot( normalDirection, -lightDirection ));
                
//                float NdotvD = max(0, dot( normalDirection, viewDir ));
//                float VdotH = max(0,dot(viewDirection,halfDirection));
                float Ndoth = max(0,dot(halfDirection,normalDirection));
//                float Ndotah = max(0,dot(ahalfDirection,normalDirection));
                float mrref = max(0,dot(reflect(-lightDirection,normalDirection),viewDirection));
                float Ndothd = dot(normalDirection,halfDir);
                float Vdoti = max(0, dot( i.normalDir, viewDirection));
                float NdotV = 1 - Ndotv;
                float Ndotl = 1 - NdotL;
                float NdotSunchar = max(0,dot(charSunDirection,normalDirection)*NdotV);
                float3 sh = AmbientColorGradient(normalDirection);
                       sh = lerp(sh,dot(sh,float3(0.3,0.59,0.11)),NdotL);// * (pow(NdotL,0.7)*0.5+0.5);
                       sh *= lerp((pow(NdotL,0.7)*0.5+0.5),1,lerp(_ao,1,noskin));
                float3 attenColor = lerp(sh,1,attenuation) * lightColor;
                float3 _Rim = Rim(NdotV,_RimPower,_RimColor);
///////// Gloss:
                float gloss = _SpecularmapRGBA_var.a;
                fixed pie = 3.1415926535;
                fixed pie2 = pie/2;
///// SkinColor:
                float3 _skincolor = ColorCstm(_BaseRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r);
                float3 _skinspcolor = ColorCstm(_SpecularmapRGBA_var.rgb,_SkinColor.rgb,_MatMask_var.r) + (_SpecularmapRGBA_var.rgb*saturate(_MatMask_var.g*10));

                float3 additionallightcolor = _SunColorchar.rgb * _SunColorchar.a * NdotSunchar;
////// Specular:
                float3 specmncrm = dot(_SpecularmapRGBA_var.rgb,float3(0.3,0.59,0.11));
                float spGGX = SpecularGGX(pie, gloss, _MatMask_var.r, max(Ndothd,Ndoth), NdotV, NdotL);//saturate(Ndothd+Ndoth)max(Ndothd,Ndoth)
                float3 specColor = Specularskin(gloss, max(Ndothd,Ndoth), _lf, spGGX, NdotL, attenColor, Ndotv, _headlight, Ndotl, _skinspcolor, specmncrm);
                float3 directSpecular = attenColor * specColor;
                float3 specular = directSpecular * _SpecularIntensity * i.pl;
                _TransmissionColor.rgb *= _ssspower;
/////// Diffuse:
                float T = Transmission(VdotO,_TransmissionRange);
                float scatter = Scattering(Ndotv, 10, 0.05,_MatMask_var.r);
                float3 scattering = scatter * _Skinems * 0.5;
                float3 sssadd = _MatMask_var.g * (T*_MatMask_var.r+NdotO) * _TransmissionColor.rgb*0.5;
                float3 NdotLs = LightingbyBRDFmap(_Brdfmap, pow(NdotL,1), sqrt(Ndotv), Ndotl, _brdfrange, 0.5, 1, _SkinDirpow);
//                       NdotLs = lerp(NdotLs,FakeSSSLightingforSkin1(NdotL, Ndotv, sh, FastSSSforskin(Ndotl,NdotV,_Skinedge.rgb), scatter,Ndotl),sks);
                       NdotLs *= _MatMask_var.r;// * (pow(NdotL,0.7)*0.5+0.5);
//                       NdotLs = pow(NdotL,3) * _MatMask_var.r;
                

                float3 trasission = saturate(pow((Ndotv+Vdoti)*0.5,6))*_MatMask_var.r*0.25*_TransmissionColor.rgb;
                float3 diffuse = CalculateDiffuseAddon(NdotLs,0,scattering,attenColor,sh,_skincolor,NdotSunchar) * lerp(i.pl,1,_skinpoint);
                       diffuse = lerp(diffuse,_skincolor*(1+_se*NdotL),NdotL*_MatMask_var.r);
                       diffuse += sssadd;
//                float3 diffuse1 = CalculateDiffuseAddonf(NdotLs,0,scattering*0,attenColor,sh,_skincolor,NdotSunchar,_MatMask_var.r) * lerp(i.pl,1,_skinpoint);
//                       diffuse = lerp(diffuse,diffuse1,sks); 
///// Eye Color:
                float3 _EyeColor = ColorCstm(_BaseRGBA_var.rgb,_Eyecolor.rgb,_MatMask_var.g);
                float3 eye = Eye(_EyeColor,_MatMask_var,emispower,NdotLD,Ndotv,sh);
/// Final Color:
                float3 finalColor = diffuse;
                       finalColor *= _MatMask_var.r;
                       finalColor += specular;
                       finalColor += eye + _Rim;
                       finalColor = max(0,finalColor);

//                       finalColor = lerp(finalColor,_skincolor,_spchk);
//                       finalColor = lerp(finalColor,CalculateDiffuseAddon(NdotLs,0,scattering,attenColor,sh,_skincolor,NdotSunchar) * lerp(i.pl,1,_skinpoint),_spchk1);
//                       finalColor = lerp(finalColor,_skincolor*(1+_se*NdotL),_spchk2);
//                       finalColor = lerp(finalColor,NdotLs + scattering,_spchk3);
//                       finalColor = lerp(finalColor,specColor,_spchk4);
//
//
//                       finalColor = pow(NdotvD,exp2( gloss * 10 + 1));
//                       finalColor = pow(Ndotv,4);
//                       finalColor = specular;
//                       finalColor = NdotLs;
//                       finalColor = NdotLs;// + (pow(NdotLq,0.7)*0.5+0.5);
//                       finalColor = NdotL;
//                       finalColor = Ndotl;
//                       finalColor *= NdotV;
//                       finalColor = FastSSSforskin(Ndotl,NdotV,_Skinedge.rgb);
//
//                       finalColor = FakeSSSLightingforSkin1(NdotL, Ndotv, sh, finalColor, scatter,Ndotl);
//                       finalColor *= _skincolor;

                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        
        UsePass "Hidden/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }

    SubShader {
        
        Tags {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest-50"
            "DisableBatching"="True"
             }
        LOD 200

        UsePass "Hidden/Character/CharacterPass/CHARACTERFORWARDBASEFACE"
        UsePass "Hidden/Character/CharacterPass/CHARACTERSHADOWCASTER"
    }
    //FallBack "Diffuse"
}
