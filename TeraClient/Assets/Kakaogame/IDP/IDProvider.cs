#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kakaogame.SDK.IDP
{
    /// <summary>
    /// 내장된 IDP에서 공통적으로 제공되어야 하는 인터페이스를 정의
    /// </summary>
    interface IDProvider
    {
        string accessToken { get; }
        string userId { get; }
        bool isAuthorized { get; }

        void Initialize(Action<bool> callback);
        void Login(Action<bool> callback);
        void Logout(Action<bool> callback);
        void Unregister(Action<bool> callback);
        void Refresh(Action<bool> callback);
    }
}
#endif
