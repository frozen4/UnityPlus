using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Common
{
	public class DataSupport
	{
		public static Dictionary<string, bool> PropertyWhiteList = new Dictionary<string, bool>();

		static DataSupport()
		{
			PropertyWhiteList.Add("System.Boolean", true);
			PropertyWhiteList.Add("System.Int32", true);
            PropertyWhiteList.Add("System.UInt32", true);
			PropertyWhiteList.Add("System.Single", true);
			PropertyWhiteList.Add("System.String", true);
			PropertyWhiteList.Add("System.Enum", true);
        }

		public static bool IsPropertyInWhiteList(Type propertyType)
		{
			if (PropertyWhiteList.ContainsKey(propertyType.FullName)) return true;
			if (propertyType.BaseType != null)
			{
				if (PropertyWhiteList.ContainsKey(propertyType.BaseType.FullName)) return true;
			}
			return false;
		}
	}

    public class DataException : Exception
	{
		public string DataMessage;

		public DataException(string message)
		{
			DataMessage = message;
		}
	}
}
