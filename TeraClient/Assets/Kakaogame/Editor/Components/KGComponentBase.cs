#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor.Component
{
    using Kakaogame.SDK;
    using Kakaogame.Base;

    /// <summary>
    /// 이 클래스를 상속하여 컴포넌트 추가/제거 로직을 구현한다.
    /// </summary>
    public class KGComponentBase
    {
        public bool isFixedComponent { get; protected set; }
        public bool isAvaliable { get; protected set; }

        /// <summary>
        /// 컴포넌트 추가가 이루어졌을 때
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// 컴포넌트 제거가 이루어졌을 때
        /// </summary>
        public virtual void OnDisable() { }

        public virtual void OnInstallNativeLibrary() { }
        public virtual void OnUninstallNativeLibrary() { }

        /// <summary>
        /// 인스펙터를 그릴 때
        /// </summary>
        public virtual void OnInspector() { }

        /// <summary>
        /// iOS 포스트빌드 처리
        /// 이 메소드를 상속하여 iOS 빌드시 후처리를 수행합니다.
        /// </summary>
        public virtual void OnIOSPostprocess() { }

        public void DrawInspector()
        {
            var prop = typeof(KGSharedData).GetProperty("use" + name);
            if(prop == null)
            {
                KGDebug.Error("KGComponent :: Property not found : " + name);
                EditorGUILayout.LabelField(name + " : ERROR");
                return;
            }

            var old =
                (bool)prop.GetValue(KGSharedData.instance, null) | 
                isFixedComponent; // FixedComponent일 경우 무조건 true 처리

            var changed = EditorGUILayout.Toggle(name, old) |
				isFixedComponent;
            prop.SetValue(
                KGSharedData.instance,
                changed, null);

            if (changed)
            {
                EditorGUI.indentLevel++;
                OnInspector();
                EditorGUI.indentLevel--;
            }

            if (old != changed)
            {
                if (changed)
                    OnEnable();
                else
                    OnDisable();
            }
        }

        public string name
        {
            get
            { 
                return GetType().Name.Substring(2);
            }
        }
    }
}
#endif
