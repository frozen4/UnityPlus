#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor.Component
{
	public class KGPush : KGComponentBase
	{
		public KGPush()
		{
			isAvaliable = KGPackage.isGeneric;
		}

		public override void OnEnable()
		{
			KGSharedData.instance.usePush = true;
			KGBuildAPI.ApplyPlugins ();
		}
		public override void OnDisable()
		{
			KGSharedData.instance.usePush = false;
			KGBuildAPI.ApplyPlugins ();
		}

		public override void OnInstallNativeLibrary()
		{
		}
		public override void OnUninstallNativeLibrary()
		{
		}

		public override void OnInspector()
		{
			GUILayout.BeginHorizontal();

			GUILayout.EndHorizontal();
		}
	}
}
#endif
