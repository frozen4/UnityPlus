#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Kakaogame.SDK.Editor
{
    //[InitializeOnLoad]
    public class KGStartWindow : EditorWindow
    {
        public static KGStartWindow window { get; set; }

        private static readonly string firstrunPath = "Assets/Kakaogame/firstrun";
        private static readonly string sdkGuideUrl = "https://tech-wiki.kakaogames.com:8443/pages/viewpage.action?pageId=6469842";
        private static readonly string apiReferenceUrl = "https://dist-test-sdk.s3.amazonaws.com/KakaoGameSDK/Unity/Doc/index.html";

        static KGStartWindow()
        {
            if (File.Exists(firstrunPath) == false)
            {
                KGMenu.WelcomeScreen();

                File.WriteAllText(firstrunPath, DateTime.Now.ToLongDateString());
            }
        }
        
        void OnEnable()
        {
            window = this;

            minSize = new Vector2(440, 500);
            maxSize = new Vector2(440, 500);

            wantsMouseMove = true;

            KGResources.Initialize();

            titleContent.text = "KG SDK";
            titleContent.image = KGResources.logo;
        }

        public void OnGUI()
        {
            GUI.backgroundColor = Color.white;
            var style = new GUIStyle();
            style.normal.background = Texture2D.whiteTexture;
            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height), style);

            var imgHeader = new GUIStyle();
            imgHeader.normal.background = Texture2D.whiteTexture;
            imgHeader.normal.textColor = Color.white;

            //GUI.Box(new Rect(20,-20, 176, 100), "", imgHeader);
            var headLabelStyle = new GUIStyle(GUI.skin.label);
            headLabelStyle.fontSize = 30;
            headLabelStyle.fontStyle = FontStyle.Bold;
            headLabelStyle.normal.textColor = new Color32(122, 122, 122, 255);
            headLabelStyle.fixedHeight = 50;
            GUI.Label(new Rect(12, 12, 300,200), "KakaoGame SDK", headLabelStyle);

            headLabelStyle.normal.textColor = new Color32(255, 220, 50, 255);
            GUI.Label(new Rect(10, 10, 300, 200), "KakaoGame SDK", headLabelStyle);

            GUILayout.Space(100);

            if (ButtonWithIcon(KGResources.setting, "Configure SDK", "AppID, LogLevel등 SDK 환경설정을 수행합니다."))
            {
                KGMenu.Config_SDK();
            }
            if (ButtonWithIcon(KGResources.guide, "Guide", "연동 가이드를 표시합니다. (인터넷)"))
            {
                Application.OpenURL(sdkGuideUrl);
            }
            if (ButtonWithIcon(KGResources.example, "API Reference", "API 레퍼런스를 표시합니다. (인터넷)"))
            {
                Application.OpenURL(apiReferenceUrl);
                //new KGExampleWindow().Show();
            }

            GUI.Label(new Rect(400, 480, 40, 20), KGPackage.version.ToString());

            GUILayout.EndArea();
        }

        bool ButtonWithIcon(Texture texture, string heading, string body, int space = 0)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(54);
            GUILayout.Box(texture, GUIStyle.none, GUILayout.MaxWidth(48));
            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.Space(1);
            GUILayout.Label(heading, EditorStyles.boldLabel);
            GUILayout.Label(body);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            var rect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            bool returnValue = false;
#if UNITY_2017_3_OR_NEWER
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
#else
			if (Event.current.type == EventType.mouseDown && rect.Contains(Event.current.mousePosition))
#endif
            {
                returnValue = true;
            }

            GUILayout.Space(space);

            return returnValue;
        }
    }

}
#endif
