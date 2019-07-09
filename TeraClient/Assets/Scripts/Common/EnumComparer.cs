using System;
using System.Collections.Generic;

//namespace EnumComparer
//{
    public class EnumComparer<T> : IEqualityComparer<T> where T : struct
    {
        public bool Equals(T first, T second)
        {
            int a = Convert.ToInt32(first);
            int b = Convert.ToInt32(second);
            return a == b;
        }

        public int GetHashCode(T instance)
        {
            return Convert.ToInt32(instance);
        }
    }
//}
