#if PLATFORM_KAKAO
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor
{
    // 사용되지 않음
    public class KGConfigIOS : EditorWindow
    {
        private static readonly string IOSSdkPath = KGPackage.sdkPath + "iOSNative/";
        private static readonly string FrameworkPath = IOSSdkPath + "framework/";
        private static readonly string IDPPath = IOSSdkPath + "sample/IDP/";
        private static readonly string InstallPath = "Assets/Plugins/iOS/ZinnySDK";

        public KGConfigIOS()
        {
#pragma warning disable 0618
            title = "Zinny iOS SDK Configuration";
#pragma warning restore 0618

            maxSize = new Vector2(300, 100);
        }

        public void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;

            EditorGUILayout.LabelField("<b>iOS SDK Path</b> : " + IOSSdkPath, style);
            EditorGUILayout.LabelField("<b>Install Path</b> : " + InstallPath, style);
            
            if (Directory.Exists(InstallPath))
            {
                if (GUILayout.Button("Uninstall"))
                {
                    Debug.Log("Uninstall Zinny iOS SDK");
                    Debug.Log("Installed path : " + InstallPath);

                    FileUtil.DeleteFileOrDirectory(InstallPath);
                }
            }
            else if (Directory.Exists(FrameworkPath) &&
                Directory.Exists(IDPPath))
            {
                if (GUILayout.Button("Install"))
                {
                    Debug.Log("Install Zinny iOS SDK");
                    Debug.Log("Install path : " + InstallPath);

                    FileUtil.CopyFileOrDirectory(FrameworkPath, InstallPath);
                    FileUtil.CopyFileOrDirectory(IDPPath, InstallPath);
                }
            }
            else
            {
                EditorGUILayout.LabelField("iOS SDK Not Found");
                EditorGUILayout.LabelField("Please place iOS SDK under " + IOSSdkPath + "path.");
                if (GUILayout.Button("Open directory"))
                {
                    Application.OpenURL("file://" + Directory.GetCurrentDirectory() + "/" + IOSSdkPath);
                }

            }
        }
    }
}
#endif
