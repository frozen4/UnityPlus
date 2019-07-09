#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kakaogame.SDK.Editor
{
    public static class KGUtil
    {
        public static string ToLowerFirstChar(this string input)
        {
            string newString = input;
            if (!string.IsNullOrEmpty(newString) && Char.IsUpper(newString[0]))
                newString = Char.ToLower(newString[0]) + newString.Substring(1);
            return newString;
        }
        public static string SafeString(this string input)
        {
            if (input == null)
                return "";
            else
                return input;
        }
    }
}
#endif
