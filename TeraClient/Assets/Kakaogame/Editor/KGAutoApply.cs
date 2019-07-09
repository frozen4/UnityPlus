#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace Kakaogame.SDK.Editor
{
    [InitializeOnLoad]
    public class KGAutoApply
    {
        static KGAutoApply()
        {
			EditorApplication.playmodeStateChanged += () =>
			{
			};
			// Apply When Editor load
             
			EditorApplication.delayCall += () => {
				KGBuildAPI.Apply();
			};
        }
    }
}
#endif
