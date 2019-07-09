#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace Kakaogame.SDK.Internal
{
    /// <summary>
    /// iOS 네이티브 SDK와 통신을 수행한다.
    /// </summary>
    public class IosSyncRequest
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private extern static string KGInterfaceBrokerRequest(string request);
#endif

        

        public static string Request(string input)
        {
#if UNITY_IOS
            return KGInterfaceBrokerRequest(input);
#else
            return "";
#endif
        }
    }
}
#endif
