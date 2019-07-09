#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor.Component
{
	public class KGFacebook : KGComponentBase
	{
		public KGFacebook()
		{
			isAvaliable = KGPackage.isGeneric;
		}

		public override void OnEnable()
		{
			//FileUtil.MoveFileOrDirectory(
			//    "Assets/Kakaogame/IDP/Facebook.nz", "Assets/Kakaogame/IDP/Facebook.cs");

			KGSharedData.instance.useFacebook = true;
			KGBuildAPI.ApplyPlugins ();
			KGBuildAPI.RegenerateAndroidManifest ();
		}
		public override void OnDisable()
		{
			//FileUtil.MoveFileOrDirectory(
			//    "Assets/Kakaogame/IDP/Facebook.cs", "Assets/Kakaogame/IDP/Facebook.nz");

			KGSharedData.instance.useFacebook = false;
			KGBuildAPI.ApplyPlugins ();
		}

		public override void OnInstallNativeLibrary()
		{
			KGSharedData.instance.useFacebookNative = true;
		}
		public override void OnUninstallNativeLibrary()
		{
			KGSharedData.instance.useFacebookNative = false;
		}

		public override void OnIOSPostprocess()
		{
#if UNITY_IOS
			KGIosSupport.AddConfig("FacebookAppID", KGSharedData.Facebook.appKey);
			KGIosSupport.AddConfig("FacebookDisplayName", KGSharedData.Facebook.appTitle);
			KGIosSupport.AddConfig("FacebookReadPermissions", KGSharedData.Facebook.permissions);

			KGIosSupport.AddURLScheme("FacebookLogin", "fb" + KGSharedData.Facebook.appKey);

			KGIosSupport.AddQueryScheme(new List<object>()
			{
				"fbapi",
				"fbapi20130214",
				"fbapi20130410",
				"fbapi20130702",
				"fbapi20131010",
				"fbapi20131219",
				"fbapi20140410",
				"fbapi20140116",
				"fbapi20150313",
				"fbapi20150629",
				"fbauth",
				"fbauth2",
				"fb-messenger-api20140430",
				"fb-messenger-platform-20150128",
				"fb-messenger-platform-20150218",
				"fb-messenger-platform-20150305"
			});
#endif
		}

		[SerializeField]
		private bool fold = true;
		public override void OnInspector()
		{
			string appKey = EditorGUILayout.TextField("App Key", KGSharedData.instance.facebook.appKey);
			string appTitle = EditorGUILayout.TextField("App Title", KGSharedData.instance.facebook.appTitle);

			if (KGSharedData.instance != null && KGSharedData.instance.useFacebook == true)
			{
				if (appKey != KGSharedData.instance.facebook.appKey)
				{
					KGSharedData.instance.facebook.appKey = appKey;
				}

				if (appTitle != KGSharedData.instance.facebook.appTitle)
				{
					KGSharedData.instance.facebook.appTitle = appTitle;
				}
			}

			var lastEnabled = GUI.enabled;
			GUI.enabled = false;
			EditorGUILayout.TextField("URL Scheme", "fb" + KGSharedData.instance.facebook.appKey);
			GUI.enabled = lastEnabled;

			KGPermissionList.ShowPermissionList("ReadPermissions", KGSharedData.instance.facebook.permissions, ref fold);

			//            Type fbType = null;
			//            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			//            foreach (var asm in assemblies)
			//            {
			//                fbType = asm.GetType("Facebook.Unity.FacebookSettings");
			//                if (fbType != null) break;
			//            }
			//
			//#if KG_ENABLE_FB_UNITY
			GUILayout.BeginHorizontal();
			//            GUILayout.FlexibleSpace();
			if (KGSharedData.instance.useFacebook == true && GUILayout.Button("FB SDK Settings"))
			{
				EditorWindow.focusedWindow.Close();
				EditorApplication.ExecuteMenuItem("Facebook/Edit Settings");
			}

			GUILayout.EndHorizontal();
			//
			//            // FB 설정과 지니 설정을 동기화
			//            if (GUI.changed && fbType != null)
			//            {
			//                fbType.GetProperty("AppLabels").SetValue(
			//                    null, new List<string>() { KGSharedData.instance.facebook.appTitle }, new object[] { });
			//                fbType.GetProperty("AppIds").SetValue(
			//                    null, new List<string>() { KGSharedData.instance.facebook.appKey }, new object[] { });
			//                fbType.InvokeMember(
			//                    "SettingsChanged",
			//                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
			//                    Type.DefaultBinder, null, new object[] { });
			//            }
			//#endif
			//
			//            GUILayout.BeginHorizontal();
			//            EditorGUILayout.LabelField("" /* NativeLibrary */);
			//            GUILayout.FlexibleSpace();
			//
			//            if (fbType == null)
			//                ; // EditorGUILayout.LabelField("Not Found");
			//            else
			//            {
			//                var fbVerType = fbType.Assembly.GetType("Facebook.Unity.FacebookSdkVersion");
			//                EditorGUILayout.LabelField("FB Unity SDK : v." + fbVerType.GetProperty("Build").GetValue(null, new object[] { }).ToString());
			//            }
			//            
			//            GUILayout.EndHorizontal();
		}
	}
}
#endif
