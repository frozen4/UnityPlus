using LuaInterface;
using System;
using Common;
using UnityEngine;

public enum Enum_SkillCollisionShapeType
{
	RECT = 0,
	FAN,
	CYCLE,
	NUM
};

public class CSkillCollisionShape
{
    float[] _Float3_1 = new float[3];

    private IntPtr _raw;
    private Enum_SkillCollisionShapeType _type;

    public CSkillCollisionShape(Enum_SkillCollisionShapeType type, IntPtr ptr)
    {
        _type = type;
        _raw = ptr;
    }

    Enum_SkillCollisionShapeType Type { get { return _type;  } }

    public void Release()
    {
        if(_raw != IntPtr.Zero)
        {
            Common.SCounters.Instance.Increase( EnumCountType.DestroyShape );
            LuaDLL.SC_DestroyShape(_raw);
            _raw = IntPtr.Zero;
        }
    }

    public bool IsCollideWith(Vector3 pos, float radius)
    {
        _Float3_1[0] = pos.x;
        _Float3_1[1] = pos.y;
        _Float3_1[2] = pos.z;

        return LuaDLL.SC_IsCollideWithShape(_raw, _Float3_1, radius);
    }
}

public class CSkillCollision : Singleton<CSkillCollision> 
{
    float[] _Float3_1 = new float[3];
    float[] _Float3_2 = new float[3];

    public CSkillCollisionShape CreateShape(Enum_SkillCollisionShapeType type, float radius, float length, float angle, Vector3 pos, Vector3 dir)
    {
        _Float3_1[0] = pos.x;
        _Float3_1[1] = pos.y;
        _Float3_1[2] = pos.z;

        _Float3_2[0] = dir.x;
        _Float3_2[1] = dir.y;
        _Float3_2[2] = dir.z;

        Common.SCounters.Instance.Increase( EnumCountType.CreateShape );
        IntPtr pointer = LuaDLL.SC_CreateShape((int)type, radius, length, angle, _Float3_1, _Float3_2);

        if (pointer == IntPtr.Zero)
            return null;

        return new CSkillCollisionShape(type, pointer);
    }

}
