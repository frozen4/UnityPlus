// ReSharper disable once CheckNamespace
namespace Common
{
	//Singleton���Ϳ�
	//�̰߳�ȫ
	//ʹ�÷���:
	//class TestClass : Singleton<TestClass>
	//{
	//	......
	//{
	//
	//ע��:�޷���֤�ⲿʹ���߲�new,ʧȥ��Singleton�Ĳ�����new��У��
	//�ŵ�ֻ����д�����
    
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
	 *  ����ʵ�֣�
	 *  Activator.CreateInstance�滻��new���ŵ���ڣ����о�
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