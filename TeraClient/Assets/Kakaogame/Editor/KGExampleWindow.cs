#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace Kakaogame.SDK.Editor
{
    public class KGExampleWindow : UnityEditor.EditorWindow
    {
        private Texture iconLogo;
        private Texture iconExample;

        private Dictionary<string, List<string>> api = new Dictionary<string, List<string>>();
        private Dictionary<string, bool> fold = new Dictionary<string, bool>();

        private string selectedKlass = null;

        private object webView;
        private Type webViewType;

        public KGExampleWindow()
        {
            maxSize = new Vector2(450, 600);

            titleContent.text = "API Example";
            titleContent.image = KGResources.logo;

            api = new Dictionary<string, List<string>>()
            {
                {"Leaderboard", new List<string>()
                {
                    "LoadMyRanking",
                    "LoadMyLastSeasonRanking",
                    "LoadRankings",
                    "LoadLastSeasonRankings",
                    "ReportScore"
                }},
                {"AppOption", new List<string>()
                {
                    "getGameServerAddress",
                    "getCDNAddress",
                    "getValue"
                }},
                {"Session", new List<string>()
                {
                    "LoadMyRanking",
                    "LoadRankings",
                    "LoadLastSeasonRankings"
                }},
                {"Mail", new List<string>()
                {
                    "LoadMails",
                    "LoadUnreadMailCount",
                    "MarkAsReadMails",
                    "DeleteMails"
                }},
                {"Log", new List<string>()
                {
                    "LoadMyRanking",
                    "LoadRankings",
                    "LoadLastSeasonRankings"
                }},
                {"Support", new List<string>()
                {
                    "LoadMyRanking",
                    "LoadRankings",
                    "LoadLastSeasonRankings"
                }},
                {"Coupon", new List<string>()
                {
                    "ShowCouponPopup"
                }},
                {"Player", new List<string>()
                {
                    "GetLocalPlayer",
                    "LoadPlayers",
                    "LoadFriendPlayers",
                    "LoadUnregisteredFriendProfiles"
                }}
            };

            foreach(var apiName in api)
            {
                fold[apiName.Key] = false;
            }
        }

        public void OnGUI()
        {
            GUI.backgroundColor = Color.white;
            var style = new GUIStyle();
            style.normal.background = Texture2D.whiteTexture;
            GUILayout.BeginArea(new Rect(0, 0, 450, position.height), style);

            var imgHeader = new GUIStyle();
            imgHeader.normal.background = (Texture2D)KGResources.logo;
            imgHeader.normal.textColor = Color.white;

            GUI.Box(new Rect(20, -20, 176, 100), "", imgHeader);

            GUI.DrawTexture(new Rect(394, 0, 50, 50), KGResources.code);
            GUI.Label(new Rect(340, 30, 100, 30), "API Example");

            GUILayout.Space(100);

            if (selectedKlass == null)
            {
                for (int i = 0; i < api.Count; i += 3)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(30);

                    try
                    {
                        var a1 = api.ElementAt(i);
                        ShowMethodList(a1.Key);

                        var a2 = api.ElementAt(i + 1);
                        ShowMethodList(a2.Key);

                        var a3 = api.ElementAt(i + 2);
                        ShowMethodList(a3.Key);
                    }
                    catch
                    {
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(20);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30);
                ShowMethodList(selectedKlass);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(20);
            }
            

            GUILayout.EndArea();
        }

        private void ShowMethodList(string klass)
        {
            if (fold.ContainsKey(klass) == false)
                fold[klass] = false;

            EditorGUILayout.BeginVertical();
            var changed = EditorGUILayout.Foldout(fold[klass], klass);
            
            if (changed)
            {
                var methods = api[klass];

                foreach (var method in methods)
                {
                    EditorGUILayout.LabelField("  - " + method);
                    var rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

#if UNITY_2017_3_OR_NEWER
					if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
#else
					if (Event.current.type == EventType.mouseDown && rect.Contains(Event.current.mousePosition))
#endif
                    {
                        ShowWebview("");
                    }
                }

                selectedKlass = klass;
            }
            if (fold[klass] && changed == false)
                selectedKlass = null;

            fold[klass] = changed;
            EditorGUILayout.EndVertical();
        }

        private void CreateWebView()
        {
            maxSize = new Vector2(1200, 600);
            position = new Rect(position.x, position.y, 1200, position.height);

            var thisWindowGuiView = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

            webViewType = GetTypeFromAllAssemblies("WebView");
            webView = ScriptableObject.CreateInstance(webViewType);

            Rect webViewRect = new Rect(450, 22, position.width - 450, position.height - 25);
            webViewType.GetMethod("InitWebView").Invoke(
                webView,
                new object[] { thisWindowGuiView, (int)webViewRect.x, (int)webViewRect.y, (int)webViewRect.width, (int)webViewRect.height, true });
        }
        private void ShowWebview(string url)
        {
            if (webView == null)
                CreateWebView();

            webViewType.GetMethod("LoadURL").Invoke(webView, new object[] { "file://C:\\Users\\hyun\\Documents\\GitHub\\KakaoGameSDK_Unity\\KakaoGameSDK\\Binaries\\Help\\index.html" });
        }
        public static Type GetTypeFromAllAssemblies(string typeName)
        {
            Assembly[] assemblies = new Assembly[] { typeof(EditorWindow).Assembly };
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
                        return type;
                }
            }
            return null;
        }
    }
}
#endif
