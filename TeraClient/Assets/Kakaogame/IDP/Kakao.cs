#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Kakaogame.SDK.IDP
{
    public class Kakao : IDProvider
    {
        public string accessToken
        {
            get;set;
        }
        public string userId
        {
            get;set;
        }
        public bool isAuthorized
        {
            get;set;
        }

        class KakaoLoginUI : MonoBehaviour
        {
            public Action<bool> callback { get; set; }
            public Kakao caller { get; set; }

            private string accessToken { get; set; }
            private string userId { get; set; }

            void Start()
            {
                accessToken = "";
                userId = "";
            }

            void OnGUI()
            {
                var width = Screen.width - 100;
                var height = 400;
                var rect = new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height);
                var innerRect = new Rect(Screen.width / 2 - width / 2 + 10, Screen.height / 2 - height / 2 + 10, width - 20, height - 20);

                GUI.Box(rect, "");

                GUILayout.BeginArea(innerRect);

                GUILayout.Label("KakaoLogin");

                // USER ID
                GUILayout.BeginHorizontal();
                GUILayout.Label("UserId");
                userId = GUILayout.TextField(userId, GUILayout.Width(300));
                GUILayout.EndHorizontal();
                // ACCESS TOKEN
                GUILayout.BeginHorizontal();
                GUILayout.Label("AccessToken");
                accessToken = GUILayout.TextField(accessToken, GUILayout.Width(300));
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel"))
                {
                    caller.isAuthorized = false;

                    callback(false);
                    Destroy(gameObject);
                }
                if (GUILayout.Button("OK"))
                {
                    caller.accessToken = accessToken;
                    caller.isAuthorized = true;
                    caller.userId = userId;

                    callback(true);
                    Destroy(gameObject);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }
        }

        public void Initialize(Action<bool> callback)
        {
            callback(true);
        }
        public void Login(Action<bool> callback)
        {
            var obj = new GameObject("KakaoLoginUI");
            var comp = obj.AddComponent<KakaoLoginUI>();

            comp.callback = callback;
            comp.caller = this;
        }
        public void Logout(Action<bool> callback)
        {
            callback(true);
        }
        public void Unregister(Action<bool> callback)
        {
            callback(true);
        }

        public void Refresh(Action<bool> callback)
        {
            callback(true);
        }
    }
}
#endif
