using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuideMaskTrsSet : MonoBehaviour {
	private Material mask;
	private float _MaxTime = 1; 
	private float _CurTime = 0; 
	private Vector4 _CurV4;
	private Vector4 _TarV4;
	public bool IsLerp = false;
	// Use this for initialization
	void Awake(){
		mask = this.GetComponent<Image> ().material;
	}
	
	public void SetMaskTrs(GameObject dest)
	{
		//Debug.Log ("destname="+dest.name);

		_CurTime = 0;
		_CurV4 = Vector4.zero;

		RectTransform rt = dest.GetComponent<RectTransform> ();

		RectTransform t_root = Main.UIRootCanvas as RectTransform;
		Rect rt_root = t_root.rect;

		//Vector4 v4 = t_root.InverseTransformPoint(dest.transform.position);
		float a=0, b=0, c=0, d=0;
        //float a1 = 0, c1 = 0;
                Rect rect = GNewUITools.GetRelativeRect(t_root, rt);
                a = rect.xMin / rt_root.width + 0.5f;
                c = 0.5f - rect.xMax / rt_root.width;
				b = rect.yMin / rt_root.height + 0.5f;
				d = 0.5f - rect.yMax / rt_root.height;
                //if (Util.Equals(rt.pivot.x, 0f))
                //{
                //    a = rt.anchoredPosition.x / rt_root.width;
                //    c = (rt_root.width - (rt.anchoredPosition.x + rt.sizeDelta.x)) / rt_root.width;
                //}
                //else if (Util.Equals(rt.pivot.x, 1f))
                //{
                //    a = (rt_root.width + rt.anchoredPosition.x - rt.sizeDelta.x) / rt_root.width;
                //    c = (rt_root.width - (rt_root.width + rt.anchoredPosition.x)) / rt_root.width;
                //}
                //else if (Util.Equals(rt.pivot.x, 0.5f))
                //{

                //    a = ((rt_root.width / 2) + (rt.anchoredPosition.x - rt.sizeDelta.x / 2)) / rt_root.width;
                //    c = (rt_root.width - ((rt_root.width / 2) + (rt.anchoredPosition.x + rt.sizeDelta.x / 2))) / rt_root.width;
                //}

		//if (Util.Equals(rt.pivot.y,0f)) {
		//	b = rt.anchoredPosition.y / rt_root.height;
		//	d = (rt_root.height - (rt.anchoredPosition.y + rt.sizeDelta.y)) / rt_root.height;
		//} else if (Util.Equals(rt.pivot.y,1f)){
		//	b = (rt_root.height + rt.anchoredPosition.y - rt.sizeDelta.y) / rt_root.height;
		//	d = (rt_root.height - (rt_root.height +rt.anchoredPosition.y)) / rt_root.height;
		//} else if (Util.Equals(rt.pivot.y,0.5f)) {
		//	b = ((rt_root.height / 2) + (rt.anchoredPosition.y - rt.sizeDelta.y / 2)) / rt_root.height;
		//	d = (rt_root.height - ((rt_root.height / 2) + (rt.anchoredPosition.y + rt.sizeDelta.y / 2))) / rt_root.height;
		//}

		_TarV4 = new Vector4 (a, b, c, d);
		if (!IsLerp) {
			mask.SetVector ("_Rect", _TarV4);
		}
        //Debug.Log(":: " + a1 + ", " + c1);
        //Debug.Log("::: " + a + ", " + c);

	}

	public void MaskTrsReset()
	{
		mask.SetVector ("_Rect", new Vector4 (0, 0, 1, 1));
		IsLerp = false;
	}

	void Update()
	{
		if (IsLerp) {
			_CurTime += Time.deltaTime;
			float fill_amount = _CurTime / _MaxTime;

			_CurV4 = Vector4.Lerp (Vector4.zero, _TarV4, fill_amount);
			mask.SetVector ("_Rect", _CurV4);

			if (_CurTime >= _MaxTime) {
				IsLerp = false;
				_CurV4 = Vector4.zero;
			}
		}

	}
}
