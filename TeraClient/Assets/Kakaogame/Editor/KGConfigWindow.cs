#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor
{
    public class KGConfigWindow : EditorWindow
    {
        private KGConfigSDK _editor;
        private KGConfigSDK editor
        {
            get
            {
                if (_editor == null)
                    _editor = new KGConfigSDK();

                return _editor;
            }
        }

        private KGConfigWindow()
        {
        }

        void OnGUI()
        {
            editor.OnInspectorGUI();
        }

        void Update()
        {
            editor.Update();
        }
    }
}
#endif
