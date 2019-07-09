#if PLATFORM_KAKAO
using System;
using System.IO;

namespace Kakaogame.SDK.Editor
{
	/// <summary>
	/// 현재 패키지에 관한 정보들이 담긴 클래스
	/// </summary>
	public class KGPackage {
		/* base path */
		public static readonly string sdkPath = "Assets/Kakaogame/";

		/* endpoint path */
		public static readonly string editorScriptPath = sdkPath + "Editor/";
		public static readonly string templateScriptPath = sdkPath + "Templates/";
		public static readonly string androidBasePath = sdkPath + "AndroidNative/";
		public static readonly string androidLibPath = androidBasePath + "libs/";
		public static readonly string defaultManifestPath = sdkPath + "DefaultAndroidManifest.xml";

		public static readonly string emptySharedDataPath = "Assets/Resources/Kakaogame/EmptyKGSharedData.json";
		public static readonly string sharedDataPath = "Assets/Resources/Kakaogame/KGSharedData.json";
		public static readonly string projectPath = sdkPath + "KakaogameSDK.project";

		public static bool isGeneric { get; set; }
		public static bool isTestPackage { get; set; }
		public static Version version { get; set; }
		public static string name { get; set; }

		public static bool isSdkDevelop
		{
			get
			{
				if (File.Exists(sdkPath + "SdkDevelop.txt") == false)
					return false;

				string val = File.ReadAllText(sdkPath + "SdkDevelop.txt");
				if (val != null && string.Compare(val, "yes", true) == 0)
					return true;

				return false;
			}
		}

		public static bool isPublishingGame
		{
			get
			{
				if (File.Exists(sdkPath + "PublishingGame.txt") == false)
					return false;

				string val = File.ReadAllText(sdkPath + "PublishingGame.txt");
				if (val != null && string.Compare(val, "yes", true) == 0)
					return true;

				return false;
			}
		}

		public static bool isGameShopPaymentPackage
		{
			get
			{
				if (File.Exists(sdkPath + "GameShopPayment.txt") == false)
					return false;

				string val = File.ReadAllText(sdkPath + "GameShopPayment.txt");
				if (val != null && string.Compare(val, "yes", true) == 0)
					return true;

				return false;
			}
		}

		public static bool isSealed
		{
			get
			{
				return false;
			}
		}

		static KGPackage()
		{
			name = File.ReadAllText(sdkPath + "Package");

			isTestPackage = File.Exists(sdkPath + "TEST");
			isGeneric = true;

			version = new Version(File.ReadAllText(sdkPath + "Version.txt"));
		}
	};
}
#endif
