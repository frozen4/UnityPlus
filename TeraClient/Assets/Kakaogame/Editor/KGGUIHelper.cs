#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor
{
    public sealed class KGGUIHelper
    {
        /// <summary>
        /// </summary>
        /// <example>
        /// using (new Enabler()) {
        ///     // GUIGUIGUIGUIGUI
        /// }
        /// </example>
        public class Enabler : IDisposable
        {
            private bool target { get; set; }
            private bool old { get; set; }

            public Enabler(bool enable)
            {
                old = GUI.enabled;
                target = enable;

                GUI.enabled = target;
            }

            void IDisposable.Dispose()
            {
                GUI.enabled = old;
            }
        }
        public class Nothing : IDisposable
        {
            void IDisposable.Dispose()
            {
            }
        }

        public static Enabler Enable(bool enable)
        {
            return new Enabler(enable);
        }
        public static IDisposable DisableForSealed()
        {
            if (KGPackage.isSealed)
                return new Enabler(false);
            else
                return new Nothing();
        }
    }
}
#endif
