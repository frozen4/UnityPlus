using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LuaInterface;

public class DoTweenComponent : MonoBehaviour
{
    void OnDisable()
    {
        DoKill();
        DoKillAlpha();
    }

    public void DoMove(Vector3 toPos, float interval, Ease easeType,float fDelay, LuaFunction callback)
    {
        GameObject obj = this.gameObject;
        Tweener tweener = obj.transform.DOMove(toPos, interval);
        if (tweener != null)
        {
            tweener.OnComplete( 
                delegate { 
                    if (callback != null)
                    {
                        callback.Call();
                        callback.Release();
                    }  
                });
            tweener.SetDelay(fDelay);
            tweener.SetEase(easeType);
        }
    }

    public void DoLocalMove(Vector3 toPos, float interval, Ease easeType, LuaFunction callback)
    {
        GameObject obj = this.gameObject;
        Tweener tweener = obj.transform.DOLocalMove(toPos, interval);
        if (tweener != null)
        {
            tweener.OnComplete(
                delegate
                {
                    //Debug.Log("DoLocalMove OnComplete " + name + " " + toPos);

                    if (callback != null)
                    {
                        callback.Call();
                        callback.Release();
                    } 
                });
            tweener.SetEase(easeType);
        }
    }

    public void DoScale(Vector3 endscale, float interval, Ease easeType, LuaFunction callback)
    {
        GameObject obj = this.gameObject;
        Tweener tweener = obj.transform.DOScale(endscale, interval);
        if (tweener != null)
        {
            tweener.OnComplete(
                delegate
                {
                    if (callback != null)
                    {
                        callback.Call();
                        callback.Release();
                    } 
                });
            tweener.SetEase(easeType);
        }
    }

    public void DoAlpha(float endValue, float interval, LuaFunction callback)
    {
        DoKillAlpha();

        GameObject obj = this.gameObject;

        Image image = obj.GetComponent<Image>();
        Text text = obj.GetComponent<Text>();
        Material material = obj.GetComponent<Material>();

        Tweener tweener = null;
        if (image != null)
        {
            var c = image.color;
            c.a = 1;
            image.color = c;
            tweener = image.DOFade(endValue, interval);

        }
        if (text != null)
        {
            var c = text.color;
            c.a = 1;
            text.color = c;
            tweener = text.DOFade(endValue, interval);
                    }
        if (material != null)
        {
            var c = material.color;
            c.a = 1;
            material.color = c;
            tweener = material.DOFade(endValue, interval);
        }

        if (tweener != null)
        {
            tweener.Restart(true);
            tweener.OnComplete(
               delegate
               {
                   if (callback != null)
                   {
                       callback.Call();
                       callback.Release();
                   } 
               });
            tweener.SetEase(Ease.Linear);
        }
        else
        {
            if (callback != null)
            {
                callback.Call();
                callback.Release();
            }
        }
    }

    public void DoScaleFrom(Vector3 fromScale, float interval, Ease easeType, LuaFunction callback)
    {
        GameObject obj = this.gameObject;
        Tweener tweener = obj.transform.DOScale(fromScale, interval);
        if (tweener != null)
        {
            tweener.From();
            tweener.SetEase(easeType);
            tweener.OnComplete(
                delegate
                {
                    if (callback != null)
                    {
                        callback.Call();
                        callback.Release();
                    } 
                });
        }
    }

    public void DoLoopRotate(Vector3 endValue, float interval)
    {
        GameObject obj = this.gameObject;
        Tweener tweener = obj.transform.DORotate(endValue, interval);
        tweener.SetEase(Ease.Linear);
        tweener.SetLoops(-1, LoopType.Incremental);
    }

    public void DoLocalRotateQuaternion(Quaternion toPos, float interval, Ease easeType, LuaFunction callback)
    {
        GameObject obj = this.gameObject;
        Tweener tweener = obj.transform.DOLocalRotateQuaternion(toPos, interval);
        if (tweener != null)
        {
            tweener.OnComplete(
                delegate
                {
                    if (callback != null)
                    {
                        callback.Call();
                        callback.Release();
                    } 
                });
            tweener.SetEase(easeType);
        }
    }

    public void DoSlider(float toPos, float interval, Ease easeType, LuaFunction callback)
    {
        GameObject obj = this.gameObject;
        UnityEngine.UI.Slider sld = GetComponent<UnityEngine.UI.Slider>();
        Tweener tweener = sld.DOSlider(toPos, interval);
        if (tweener != null)
        {
            tweener.OnComplete(
                delegate
                {
                    if (callback != null)
                    {
                        callback.Call();
                        callback.Release();
                    }
                });
            tweener.SetEase(easeType);
        }
    }

    public void DoKillSlider()
    {
        GameObject obj = this.gameObject;
        UnityEngine.UI.Slider sld = GetComponent<UnityEngine.UI.Slider>();
        sld.DOKill();
    }

    void DoKill()
    {
        GameObject obj = this.gameObject;
        obj.transform.DOKill();
    }

    void DoKillAlpha()
    {
        GameObject obj = this.gameObject;

        Image image = obj.GetComponent<Image>();
        Text text = obj.GetComponent<Text>();
        Material material = obj.GetComponent<Material>();

        if (image != null)
            image.DOKill();
        if (text != null)
            text.DOKill();
        if (material != null)
            material.DOKill();    
    }

    public void KillAll()
    {
        DoKill();
        DoKillAlpha();
    }



}

public static class MyDOTween
{
    //statics
    /// <summary>Tweens a Slider's <code>value</code> to the given value.
    public static Tweener DOSlider(this UnityEngine.UI.Slider target, float endValue, float duration)
    {
        return DOTween.To(() => target.value, delegate(float x)
        {
            target.value = x;
        }, endValue, duration).SetTarget(target);
    }
}
