// ReSharper disable once CheckNamespace
namespace Common
{
	//Singleton泛型壳
	//线程安全
	//使用方法:
	//class TestClass : Singleton<TestClass>
	//{
	//	......
	//{
	//
	//注意:无法保证外部使用者不new,失去了Singleton的不允许new的校验
	//优点只是少写点代码
    
	public abstract class Singleton<T> where T : class, new()
	{
		public static T Instance
        {
            get 
            {
                if (_Instance == null)
                    _Instance = new T();
                return _Instance;
            }
        }

        public static void ClearInstance()
        {
            _Instance = null;
        }

        private static T _Instance = null;
	}

	/*
	 *  蓝港实现：
	 *  Activator.CreateInstance替换了new，优点何在？待研究
	 *  
	namespace T3GameClient
	{
		using System;

		public class Singleton<T> where T : class, new()
		{
			private static T _Instance;

			public static void CreateInstance()
			{
				if (Singleton<T>._Instance == null)
				{
					Singleton<T>._Instance = Activator.CreateInstance<T>();
				}
			}

			public static void DestroyInstance()
			{
				if (Singleton<T>._Instance != null)
				{
					Singleton<T>._Instance = null;
				}
			}

			public static T GetInstance()
			{
				if (Singleton<T>._Instance == null)
				{
					Singleton<T>._Instance = Activator.CreateInstance<T>();
				}
				return Singleton<T>._Instance;
			}

			public static T GetInstance(int i)
			{
				if ((i >= 0) && (i < 1))
				{
					return Singleton<T>._Instance;
				}
				return null;
			}

			public static T Instance
			{
				get
				{
					if (Singleton<T>._Instance == null)
					{
						Singleton<T>._Instance = Activator.CreateInstance<T>();
					}
					return Singleton<T>._Instance;
				}
			}
		}
	}

	*/
}