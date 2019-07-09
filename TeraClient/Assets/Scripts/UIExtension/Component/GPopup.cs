using UnityEngine;
using DG.Tweening;

public class GPopup : GBase
{
	//ease type效果预览 ： http://robertpenner.com/easing/easing_demo.html
	//经测试如果是作为弹窗，此处可选的效果还有 easeOutBounce easeOutBack
	[SerializeField]
	private Ease _easeType = Ease.OutElastic;

    private InteractableUIHolder _holder;

	protected override void OnEnable()
	{
        if (_holder == null)
        {
            _holder = GetComponentInParent<InteractableUIHolder>();
        }

        RectTrans.localScale = Vector3.one;

        //OnMyEffectStart();

        Tweener tw = RectTrans.DOScale(Vector3.zero, .2f);
        tw.SetEase(_easeType).From();
        //tw.OnComplete(OnMyEffectOver);
	}

	protected override void OnDisable()
	{
        RectTrans.localScale = Vector3.one;
        DOTween.Kill(gameObject);
	}

    //private void OnMyEffectOver()
    //{
        //if (_holder != null)
        //{
        //    for (int i = 0; i < _holder.NewLists.Length; i++)
        //    {
        //        _holder.NewLists[i].OnPopEffectOver();
        //    }

        //    for (int i = 0; i < _holder.NewTables.Length; i++)
        //    {
        //        _holder.NewTables[i].OnPopEffectOver();
        //    }

        //    for (int i = 0; i < _holder.NewTabLists.Length; i++)
        //    {
        //        _holder.NewTabLists[i].OnPopEffectOver();
        //    }
        //}
    //}

    //private void OnMyEffectStart()
    //{
        //if (_holder != null)
        //{
        //    for (int i = 0; i < _holder.NewLists.Length; i++)
        //    {
        //        _holder.NewLists[i].OnPopEffectStart();
        //    }

        //    for (int i = 0; i < _holder.NewTables.Length; i++)
        //    {
        //        _holder.NewTables[i].OnPopEffectStart();
        //    }

        //    for (int i = 0; i < _holder.NewTabLists.Length; i++)
        //    {
        //        _holder.NewTabLists[i].OnPopEffectStart();
        //    }
        //}
    //}
}
