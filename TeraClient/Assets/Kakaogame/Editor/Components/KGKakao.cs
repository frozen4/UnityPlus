#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor.Component
{
	public class KGKakao : KGComponentBase
	{
		public KGKakao()
		{
			isFixedComponent = true;
			isAvaliable = true;
		}

		public override void OnEnable()
		{
		}
		public override void OnDisable()
		{
		}

		public override void OnInstallNativeLibrary()
		{
			//KGAndroidSupport.Modify(
			//    true,
			//    KGAndroidSupport.Projects.SDKKakao2, true);
			//KGSharedData.instance.useForKakao2Native = true;
		}
		public override void OnUninstallNativeLibrary()
		{
			//KGAndroidSupport.Modify(
			//    false,
			//    KGAndroidSupport.Projects.SDKKakao2, true);
			//KGSharedData.instance.useForKakao2Native = false;
		}

		public override void OnInspector()
		{
		}

		public override void OnIOSPostprocess()
		{
#if UNITY_IOS
			KGIosSupport.AddConfig("KAKAO_APP_KEY", KGSharedData.Configuration.appSecret);
			KGIosSupport.AddURLScheme("KakaoLogin", "kakao" + KGSharedData.Configuration.appSecret);

			KGIosSupport.AddQueryScheme(new List<object>()
				{
					"kakao" + KGSharedData.Configuration.appSecret,
					"kakaogame" + KGSharedData.Configuration.appId,
					"kakaokompassauth",
					"storykompassauth",
					"kakaolink",
					"kakaotalk",
					"kakaotalk-4.5.0",
					"kakaotalk-2.9.5",
					"kakaotalk-3.0.0",
					"kakao3rdauth",
					"kakaostory-2.9.0"
				});

			if (KGSharedData.instance.sdkDevelopConfiguration.useIOSTestAppAutoSetting == true)
			{
				KGIosSupport.AddURLScheme("KakaoLogin_Test02", "kakaob764eb8572f851999e677ffd54191ac3");
				KGIosSupport.AddURLScheme("KakaoLogin_Test03", "kakao5dcb8e668d050a4aa79b4d4a06ce66b1");
				KGIosSupport.AddURLScheme("KakaoLogin_Test04", "kakaofed762af19ef3fa6a37941ae4b1635b9");
				KGIosSupport.AddURLScheme("KakaoLogin_Test05", "kakao4b5a624351c569689a4cdf50236aa75f");
				KGIosSupport.AddURLScheme("KakaoLogin_Test06", "kakaobc0f88982e8e5fb0d88fb810dca19f1a");

				KGIosSupport.AddURLScheme("KakaoLogin_TermsTest01", "kakao7e5a851c86bd7ede96f7d4af65aa2cd2");
				KGIosSupport.AddURLScheme("KakaoLogin_TermsTest02", "kakaoae5154a3491b4913270dc8458ab88a2f");
				KGIosSupport.AddURLScheme("KakaoLogin_TermsTest03", "kakao53c12b1a71e50e7eda224d57cf5d7b7e");
				KGIosSupport.AddURLScheme("KakaoLogin_TermsTest04", "kakaofe8d0d2d5fcc63c5af989464df897af0");
				KGIosSupport.AddURLScheme("KakaoLogin_TermsTest05", "kakao08328ccd9c1755c81d155e4c17cd664f");
				KGIosSupport.AddURLScheme("KakaoLogin_TermsTest06", "kakao565908150403d0b3923203809226f868");

				KGIosSupport.AddURLScheme("KakaoLogin_AgeAuthTest01", "kakao31ce2b71e5a3282e9c1075ba872e9cdf");
				KGIosSupport.AddURLScheme("KakaoLogin_AgeAuthTest02", "kakao717883c006726d4e5f17eb4763aa0505");

				KGIosSupport.AddQueryScheme(new List<object>()
					{
						"kakaob764eb8572f851999e677ffd54191ac3",
						"kakao5dcb8e668d050a4aa79b4d4a06ce66b1",
						"kakaofed762af19ef3fa6a37941ae4b1635b9",
						"kakao4b5a624351c569689a4cdf50236aa75f",
						"kakaobc0f88982e8e5fb0d88fb810dca19f1a",

						"kakao7e5a851c86bd7ede96f7d4af65aa2cd2",
						"kakaoae5154a3491b4913270dc8458ab88a2f",
						"kakao53c12b1a71e50e7eda224d57cf5d7b7e",
						"kakaofe8d0d2d5fcc63c5af989464df897af0",
						"kakao08328ccd9c1755c81d155e4c17cd664f",
						"kakao565908150403d0b3923203809226f868",

						"kakao31ce2b71e5a3282e9c1075ba872e9cdf",
						"kakao717883c006726d4e5f17eb4763aa0505",

						"alphatalk-4.5.0",
						"alphastory-2.9.0",
						"alphakompassauth",
						"alphalink",
						"alphatalk"
					});
			}
#endif
		}
	}
}
#endif
