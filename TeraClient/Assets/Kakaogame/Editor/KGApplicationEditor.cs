#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor
{
    [CustomEditor(typeof(KGApplication))]
    public class KGApplicationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var sharedData = KGSharedData.instance;

            EditorGUILayout.LabelField("AppId : " + sharedData.configuration.appId);
            EditorGUILayout.LabelField("AppSecret : " + sharedData.configuration.appSecret);

            if (Application.isPlaying)
                return;

            if (GUILayout.Button("Configuration"))
            {
                KGMenu.Config_SDK();
            }
        }
    }
}
#endif
