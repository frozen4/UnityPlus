#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Kakaogame.SDK.Editor
{
    /// <summary>
    /// 에디터 스크립트에서 사용하는 텍스쳐 모음
    /// </summary>
    internal class KGResources
    {
        private static readonly string BasePath = "Kakaogame/";

        public static Texture2D qa { get; set; }
		public static Texture2D qaActive { get; set; }

        public static Texture2D real { get; set; }
        public static Texture2D realActive { get; set; }

		public static Texture2D googlePlay { get; set; }
		public static Texture2D googlePlayActive { get; set; }

		public static Texture2D oneStore { get; set; }
		public static Texture2D oneStoreActive { get; set; }

		public static Texture2D kakaogameShop { get; set; }
		public static Texture2D kakaogameShopActive { get; set; }
        
        public static Texture2D logo { get; set; }
        public static Texture2D code { get; set; }

        public static Texture2D setting { get; set; }
        public static Texture2D guide { get; set; }
        public static Texture2D example { get; set; }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
			if (qa != null && googlePlay != null)
                return;

            qa = Load("qa");
			qaActive = Load("qa_active");

            real = Load("real");
            realActive = Load("real_active");

			googlePlay = Load("googlePlay");
			googlePlayActive = Load("googlePlay_active");

			oneStore = Load("oneStore");
			oneStoreActive = Load("oneStore_active");

			kakaogameShop = Load("kakaogameShop");
			kakaogameShopActive = Load("kakaogameShop_active");

            logo = Load("logo");
            code = Load("code");

            setting = Load("setting");
            guide = Load("guide");
            example = Load("code"); 
        }

        private static Texture2D Load(string name)
        {
            return Resources.Load<Texture2D>(BasePath + name);
        }
    }
}
#endif
