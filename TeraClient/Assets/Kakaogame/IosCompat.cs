#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

namespace Kakaogame.SDK
{
    public class IosCompat
    {
		// iOS/IL2CPP 환경에서 코드 스트리핑에 의한 에러를 방지하기 위해서
        private static Int32Converter i32 = new Int32Converter();
        private static Int64Converter i64 = new Int64Converter();
        private static DecimalConverter dec = new DecimalConverter();
    }
}
#endif
