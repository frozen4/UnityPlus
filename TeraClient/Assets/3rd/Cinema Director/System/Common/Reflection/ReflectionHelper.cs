#if BakeReflection
using System;
using System.Collections.Generic;
using System.Reflection;

//using MemberInfoMap = System.Collections.Generic.Dictionary<string, System.Reflection.MemberInfo[]>;
//using FieldInfoMap = System.Collections.Generic.Dictionary<string, System.Reflection.FieldInfo>;
using PropertyInfoMap = System.Collections.Generic.Dictionary<string, System.Reflection.PropertyInfo>;

namespace CinemaSuite.Common
{
    /// A helper class for reflection calls, that allows for calls on multiple platforms.
    public static class ReflectionHelper
    {
        //static Dictionary<Type, MemberInfoMap> _DicTypeMemberInfoMap = new Dictionary<Type, MemberInfoMap>();
        //static Dictionary<Type, FieldInfoMap> _DicTypeFieldInfoMap = new Dictionary<Type, FieldInfoMap>();
        static Dictionary<Type, PropertyInfoMap> _DicTypePropertyInfoMap = new Dictionary<Type, PropertyInfoMap>();

        static ReflectionHelper() { }

        public static Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();     //获得程序所有的Assembly
        }

        public static Type[] GetTypes(Assembly assembly)
        {
            return assembly.GetTypes();
        }

        public static bool IsSubclassOf(Type type, Type c)
        {
            return type.IsSubclassOf(c);
        }
        public static PropertyInfo GetProperty(Type type, string name)
        {
            bool bFind = false;
            PropertyInfoMap propertyInfoMap;
            if (!_DicTypePropertyInfoMap.TryGetValue(type, out propertyInfoMap))
            {
                propertyInfoMap = new PropertyInfoMap();
            }

            PropertyInfo propertyInfo;
            if (!propertyInfoMap.TryGetValue(name, out propertyInfo))
            {
                propertyInfo = type.GetProperty(name);
                propertyInfoMap.Add(name, propertyInfo);
            }
            else
            {
                bFind = true;
            }

            if (!bFind)
                _DicTypePropertyInfoMap[type] = propertyInfoMap;
            return propertyInfo;
        }

        public static T[] GetCustomAttributes<T>(Type type, bool inherited) where T : Attribute
        {
            return (T[]) type.GetCustomAttributes(typeof(T), inherited);
        }
    }
}
#endif