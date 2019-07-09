using UnityEngine;

public class HangPointHolder : MonoBehaviour
{
    public enum HangPointType
    {
        Hurt = 1,
        WeaponLeft = 2,
        WeaponRight = 3,
        WeaponBack1 = 4,
        WeaponBack2 = 5,
        WaistTrans = 6,
        Wing = 7,
        Carry = 8,
        Ride_alipriest_f = 9,
        Ride_casassassin_m = 10,
        Ride_humwarrior_m = 11,
        Ride_sprarcher_f = 12,
        DCamPos = 14,
        DCamPosLookAt = 15,
        DCamPos1 = 16,
        DCamPosLookAt1 = 17,
        Headwear = 18,
        LineOfSightDirection = 19,
        Talent = 20
    }

    public GameObject HangPoint_Hurt;
    public GameObject HangPoint_WeaponLeft;
    public GameObject HangPoint_WeaponRight;
    public GameObject HangPoint_WeaponBack1;
    public GameObject HangPoint_WeaponBack2;
    public GameObject HangPoint_WaistTrans;
    public GameObject HangPoint_Wings;
    public GameObject HangPoint_Carry;
    // 以下挂点是在坐骑身上，不同职业挂点不同 (现在逻辑中未使用，冗余)
    public GameObject HangPoint_Ride_alipriest_f;
    public GameObject HangPoint_Ride_casassassin_m;
    public GameObject HangPoint_Ride_humwarrior_m;
    public GameObject HangPoint_Ride_sprarcher_f;
    //public GameObject HangPoint_NPCDigCamera;
    public GameObject HangPoint_DCamPos;
    public GameObject HangPoint_DCamPosLookAt;
    public GameObject HangPoint_DCamPos1;
    public GameObject HangPoint_DCamPosLookAt1;
    public GameObject HangPoint_Headwear;
    public GameObject HangPoint_LineOfSightDirection;
    public GameObject HangPoint_Talent;

    public GameObject GetHangPoint(HangPointType id)
    {
        if (id == HangPointType.Hurt)
            return HangPoint_Hurt;
        else if (id == HangPointType.WeaponLeft)
            return HangPoint_WeaponLeft;
        if (id == HangPointType.WeaponRight)
            return HangPoint_WeaponRight;
        else if (id == HangPointType.WeaponBack1)
            return HangPoint_WeaponBack1;
        if (id == HangPointType.WeaponBack2)
            return HangPoint_WeaponBack2;
        if (id == HangPointType.WaistTrans)
            return HangPoint_WaistTrans;
        if (id == HangPointType.Wing)
            return HangPoint_Wings;
        if (id == HangPointType.Carry)
            return HangPoint_Carry;
        if (id == HangPointType.Ride_alipriest_f)
            return HangPoint_Ride_alipriest_f;
        if (id == HangPointType.Ride_casassassin_m)
            return HangPoint_Ride_casassassin_m;
        if (id == HangPointType.Ride_humwarrior_m)
            return HangPoint_Ride_humwarrior_m;
        if (id == HangPointType.Ride_sprarcher_f)
            return HangPoint_Ride_sprarcher_f;
        if (id == HangPointType.DCamPos)
            return HangPoint_DCamPos;
        if (id == HangPointType.DCamPosLookAt)
            return HangPoint_DCamPosLookAt;
        if (id == HangPointType.DCamPos1)
            return HangPoint_DCamPos1;
        if (id == HangPointType.DCamPosLookAt1)
            return HangPoint_DCamPosLookAt1;
        if (id == HangPointType.Headwear)
            return HangPoint_Headwear;
        if (id == HangPointType.LineOfSightDirection)
            return HangPoint_LineOfSightDirection;
        if (id == HangPointType.Talent)
            return HangPoint_Talent;
        return null;
    }
}
