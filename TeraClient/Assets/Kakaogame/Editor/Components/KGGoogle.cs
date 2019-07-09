#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor.Component
{
	public class KGGoogle : KGComponentBase
	{
		public KGGoogle()
		{
			isAvaliable = KGPackage.isGeneric;
		}

		public override void OnEnable()
		{
			//KGAndroidSupport.Modify(true, KGAndroidSupport.Projects.IDPGoogle);

			KGSharedData.instance.useGoogle = true;
			KGBuildAPI.ApplyPlugins ();
		}
		public override void OnDisable()
		{
			//KGAndroidSupport.Modify(false, KGAndroidSupport.Projects.IDPGoogle);

			KGSharedData.instance.useGoogle = false;
			KGBuildAPI.ApplyPlugins ();
		}

		public override void OnInstallNativeLibrary()
		{
		}
		public override void OnUninstallNativeLibrary()
		{
		}

		[SerializeField]
		private bool fold = true;
		public override void OnInspector()
		{
			using(KGGUIHelper.DisableForSealed())
			{
				KGSharedData.instance.google.clientID = EditorGUILayout.TextField("iOS Client ID", KGSharedData.instance.google.clientID);

				var clientId = KGSharedData.instance.google.clientID;
				if (string.IsNullOrEmpty(clientId))
					clientId = "CLIENT_ID";

				bool old = GUI.enabled;
				GUI.enabled = false;
				KGSharedData.instance.google.urlScheme = EditorGUILayout.TextField("iOS URL Scheme", "com.googleusercontent.apps." + clientId.Split('.')[0]);
				GUI.enabled = old;

				KGSharedData.instance.google.webappClientId = EditorGUILayout.TextField("Webapp Client ID", KGSharedData.instance.google.webappClientId);

				var webappClientId = KGSharedData.instance.google.webappClientId;
				if (string.IsNullOrEmpty(webappClientId))
					webappClientId = "WEB_APP_CLIENT_ID";

				old = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.TextField("App ID", webappClientId.Split('-')[0]);
				GUI.enabled = old;
			}

			KGPermissionList.ShowPermissionList("Permissions", KGSharedData.instance.google.permissions, ref fold);

			EditorGUILayout.Space();
		}

		public override void OnIOSPostprocess()
		{
#if UNITY_IOS
			var tokens = KGSharedData.Google.clientID.Split(new char[] {'.' }, 2);
			if(tokens.Length == 2)
				KGIosSupport.AddURLScheme("GoogleLogin2", "com.googleusercontent.apps." + tokens[0]);
#if UNITY_5_6_OR_NEWER
			KGIosSupport.AddURLScheme("GoogleLogin", UnityEditor.PlayerSettings.applicationIdentifier);
#else
			KGIosSupport.AddURLScheme("GoogleLogin", UnityEditor.PlayerSettings.bundleIdentifier);
#endif
			KGIosSupport.AddConfig("GOOGLE_CLIENT_ID", KGSharedData.Google.clientID);   
			KGIosSupport.AddConfig("GOOGLE_SCOPES", KGSharedData.Google.permissions);
#endif
		}
	}
}
#endif
