using System;
using LuaInterface;
using System.Runtime.InteropServices;


public struct LuaMethod:IEquatable<LuaMethod>
{

    public string name;
    public LuaCSFunction func;

    public LuaMethod(string str, LuaCSFunction f)
    {
        name = str;
        func = f;
    }

    public bool Equals(LuaMethod other)
    {
        //throw new NotImplementedException();
        if (!(this.func.Equals(other.func)))
            return false;
        if (!(this.name.Equals(other.name)))
            return false;
        else
            return true;
    }
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (!(obj is LuaMethod))
            return false;
        return Equals((LuaMethod)obj);

    }
    public override int GetHashCode()
    {
        return func.GetHashCode() ^ name.GetHashCode();
    }
};

public struct LuaField:IEquatable<LuaField>
{
    public string name;
    public LuaCSFunction getter;
    public LuaCSFunction setter;

    public LuaField(string str, LuaCSFunction g, LuaCSFunction s)
    {
        name = str;
        getter = g;
        setter = s;        
    }

    public bool Equals(LuaField other)
    {
        //throw new NotImplementedException();
        if (!(this.name.Equals(other.name)))
            return false;
        if (!(this.getter.Equals(other.getter)))
            return false;
        if (!(this.setter.Equals(other.setter)))
            return false;
        else
            return true;
    }
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (!(obj is LuaField))
            return false;
        return Equals((LuaField)obj);

    }
    public override int GetHashCode()
    {
        return  name.GetHashCode() ^ getter.GetHashCode() ^ setter.GetHashCode();
    }
};

/*public struct LuaEnum
{
    public string name;
    public System.Enum val;

    public LuaEnum(string str, System.Enum v)
    {
        name = str;
        val = v;
    }
}*/

public class NoToLuaAttribute : System.Attribute
{
    public NoToLuaAttribute()
    {

    }
}

public class OnlyGCAttribute : System.Attribute
{
    public OnlyGCAttribute()
    {

    }
}

public class UseDefinedAttribute : System.Attribute
{
    public UseDefinedAttribute()
    {

    }
}


public interface ILuaWrap 
{
    void Register();
}

public class LuaStringBuffer
{
    //从lua端读取协议数据
    public LuaStringBuffer(IntPtr source, int len)
    {
        buffer = new byte[len];
        Marshal.Copy(source, buffer, 0, len);
    }

    //c#端创建协议数据
    public LuaStringBuffer(byte[] buf)
    {
        this.buffer = buf;
    }

    public byte[] buffer = null;
}

public class LuaRef
{
    internal IntPtr L;
    public int reference;

    public LuaRef(IntPtr L, int reference)
    {
        this.L = L;
        this.reference = reference;
    }
}