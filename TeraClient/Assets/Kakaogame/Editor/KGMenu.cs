#if PLATFORM_KAKAO
using UnityEngine;
using UnityEditor;
using System;

namespace Kakaogame.SDK.Editor
{
    public static partial class KGMenu
    {
        [MenuItem("Kakao/WelcomeScreen")]
        public static void WelcomeScreen()
        {
            //var window = new KGStartWindow();
			var window = (KGStartWindow)ScriptableObject.CreateInstance(typeof(KGStartWindow));
			window.ShowUtility();
        }

        [MenuItem("Kakao/Configure SDK")]
        public static bool Config_SDK()
        {
            KGBuildAPI.Begin();

            var asm = typeof(UnityEditor.Editor).Assembly;
            var inspector = asm.GetType("UnityEditor.InspectorWindow");
            var window = EditorWindow.GetWindow<KGConfigWindow>(inspector);

            window.Show();

            return true;
        }

        [MenuItem("Kakao/Open Log Directory")]
        public static void LogDirectory()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                Debug.LogError("Not a windows editor env");
                return;
            }

            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            System.Diagnostics.Process.Start("explorer.exe", appdata + "\\..\\Local\\Unity\\Editor");
        }

        [MenuItem("Kakao/About")]
        public static void About()
        {
            EditorUtility.DisplayDialog(
                "About",
                "KakaoGame.SDK " + KGPackage.version + "\r\n\r\n" + 
                "icon resources from:\r\n" + 
                "https://icons8.com/",
                "Close");
        }
    }
}
#endif
