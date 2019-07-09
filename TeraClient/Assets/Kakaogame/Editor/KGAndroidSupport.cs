#if PLATFORM_KAKAO
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

using UnityEditor;
using UnityEngine;

namespace Kakaogame.SDK.Editor
{
	public class KGAndroidSupport
	{
		public class Projects
		{
			public static readonly string IDPFacebook = "KakaoGameSDK_IDP_Facebook.jar.nz";
			public static readonly string IDPGoogle = "KakaoGameSDK_IDP_Google.jar.nz";

			public static readonly string SDKFacebook = "../facebook.zip";
			public static readonly string SDKGoogle = "../google-play-services_lib.zip";

			public static readonly string Push = "KakaoGameSDK_Push.jar.nz";
		}

		private static readonly string srcPath = KGPackage.androidLibPath;
		private static readonly string dstPath = "Assets/Kakaogame/AndroidPlugins/KakaoGameSDK.libs/";
		private static readonly string dstProjPath = "Assets/Kakaogame/AndroidPlugins/";

		public KGAndroidSupport()
		{
		}

		public static void Enable(string key, bool isProj = false, bool ignorable = false)
		{
			//NZProgressBar.Show ();

			try
			{
				if (key.EndsWith(".zip"))
				{
					var _srcPath = srcPath + key;

					if (key.StartsWith("../"))
						key = key.Substring(2);
					var _dstPath = dstProjPath + key;

					ZipUtil.Unzip(_srcPath, _dstPath);
				}
				else
				{
					var _srcPath = srcPath + key;

					if (key.EndsWith(".nz"))
						key = key.Substring(0, key.Length - 3);

					var _dstPath = dstPath + key;

					if (File.Exists(_dstPath))
					{
						Debug.Log("File already exists - " + _dstPath);
						File.Delete(_dstPath);
					}
					FileUtil.CopyFileOrDirectory(
						_srcPath, _dstPath);
				}
			}
			catch (Exception e)
			{
				if (ignorable == false)
				{
					EditorUtility.DisplayDialog("Error", e.Message, "OK");
				}
			}

			//NZProgressBar.End ();
		}
		public static void Disable(string key, bool isProj = false, bool ignorable = false)
		{
			//NZProgressBar.Show ();

			try
			{
				if (key.EndsWith(".nz"))
					key = key.Substring(0, key.Length - 3);
				if (key.StartsWith("../"))
					key = key.Substring(2);

				FileUtil.DeleteFileOrDirectory(
					(isProj ? dstProjPath : dstPath) + key);
			}
			catch (Exception e)
			{
				if (ignorable == false)
				{
					EditorUtility.DisplayDialog("Error", e.Message, "OK");
				}
			}

			//NZProgressBar.End ();
		}

		public static void Modify(bool b, string key, bool isProj = false, bool ignorable = false)
		{
			if (b == true)
				Enable(key, isProj, ignorable);
			else
				Disable(key, isProj, ignorable);
		}
	}

}
#endif
