using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
public class CBallCurvMotor : CMotor
{
    //写死
    private const float DEV_LENGTH = 10;
    private float _DevHeight = 2;
    private float _DevSpeed = 10;
    private List<Tweener> _TwList = new List<Tweener>();

    //设置速度和高度, 高度变化
    public void SetParam(float h, float s)
    {
        _DevHeight = h;
        _DevSpeed = s;
    }

    /*
     * target:目标
     * hud： 发出点
     * ro_ori: 初始角度
     * ro_end:结束角度
     * bullet：发射子物体
    */
    public void BallCurvFly(Vector3 target, GameObject fromObj, GameObject bulletObj, float angle)
    {
        if (fromObj == null || bulletObj == null) return;

        Transform from = fromObj.transform;
        Transform bullet = bulletObj.transform;

        Vector3 fromPos = from.position;

        var length = Util.DistanceH(target, fromPos);
        var cost = length / _DevSpeed; ;
        var costHSplit = 0.5f;
        var height = 0f;
        //大于1米的时候 分割抛物线
        if ((target.y - fromPos.y) > 1f)
        {
            height = length / DEV_LENGTH * _DevHeight / 2 + (target.y - fromPos.y);
            costHSplit = 1 - (height - (target.y - fromPos.y)) / height;
        }
        else
        {
            height = length / DEV_LENGTH * _DevHeight;
        }

        bullet.position = from.position;
        var oriy = bullet.position.y;
        var destPos = target;
        destPos.y = CUnityUtil.GetMapHeight(destPos);
        
        if (destPos.y <= CUnityUtil.InvalidHeight)
            destPos.y = oriy;

//         var midPos = from.position + (destPos - from.position) / 2;
//         midPos.y = oriy + height;

        //水平
        _TwList.Add(bullet.DOMoveX(destPos.x, cost).SetEase(Ease.Linear));
        _TwList.Add(bullet.DOMoveZ(destPos.z, cost).SetEase(Ease.Linear));
        

        //高度
        var tw = bullet.DOMoveY(oriy + height, cost * costHSplit);
        tw.SetEase(Ease.OutQuad);
        _TwList.Add(tw);

        var twLook = bullet.DORotateQuaternion(bullet.rotation * Quaternion.Euler(angle, 0, 0), cost * costHSplit);
        _TwList.Add(twLook);
        twLook.OnComplete(() =>
        {
            _TwList.Add(bullet.DOLookAt(destPos, cost * (1 - costHSplit)));
        });

        tw.OnComplete(() =>
        {
            var tw2 = bullet.DOMoveY(destPos.y, cost * (1 - costHSplit));
            tw2.SetEase(Ease.InQuad);
            _TwList.Add(tw2);
            _IsArrived = true; 
        });
    }

    protected override bool OnMove(float dt)
    {
        return false;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _TwList.Count; ++i)
        {
            _TwList[i].Pause();
            _TwList[i].Kill();
        }
        _TwList.Clear();
    }   
}