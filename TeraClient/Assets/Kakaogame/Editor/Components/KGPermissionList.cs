#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor.Component
{
    using Kakaogame.SDK.Editor;

    public class KGPermissionList
    {
        public static void ShowPermissionList(string name, List<string> list, ref bool fold)
        {
            fold = EditorGUILayout.Foldout(fold, name);
            if (fold)
            {
                var permissions = list;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("Add"))
                {
                    permissions.Add("PERMISSION_NAME");
                }
                EditorGUILayout.EndHorizontal();

                for (var i = 0; i < permissions.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    permissions[i] = EditorGUILayout.TextField(permissions[i]);
                    if (GUILayout.Button("X"))
                    {
                        permissions.RemoveRange(i, 1);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        public static void ShowPermissionList(
            string name, 
            Type hint,
            List<string> list, ref bool fold)
        {
            fold = EditorGUILayout.Foldout(fold, name);
            if (fold)
            {
                var permissions = list;
                var none = Enum.Parse(hint, "NONE");

                EditorGUILayout.BeginHorizontal();
                var selected = EditorGUILayout.EnumPopup((Enum)none);
                if (selected.ToString() != none.ToString())
                    permissions.Add(selected.ToString());
                if (GUILayout.Button("Add"))
                    permissions.Add("PERMISSION_NAME");
                EditorGUILayout.EndHorizontal();

                for (var i = 0; i < permissions.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    permissions[i] = EditorGUILayout.TextField(permissions[i]);
                    if (GUILayout.Button("X"))
                    {
                        permissions.RemoveRange(i, 1);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}
#endif
