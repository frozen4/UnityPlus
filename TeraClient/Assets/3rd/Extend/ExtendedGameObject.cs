using UnityEngine;
using System;
using System.Collections.Generic;

public static class ExtendedGameObject
{
	public static GameObject Clone(this GameObject prefab)
	{
		if(null == prefab)
		{
			return null;
		}

		var cloned					= CUnityUtil.Instantiate(prefab) as GameObject;
		var prefabTransform			= prefab.transform;
		cloned.transform.parent		= prefabTransform.parent;
		// in NGUI, the localScale will be adjusted automatically according to the scale of camera. so restore it.
		cloned.transform.localScale	= prefabTransform.localScale;
		return cloned;
	}

	public static GameObject GetChildByName(this GameObject go, string name)
	{
		if(null != go)
		{
			foreach(Transform child in go.transform)
			{
				if(child.name == name)
				{
					return child.gameObject;
				}
				
				var grandson	= child.gameObject.GetChildByName(name);
				if(null != grandson)
				{
					return grandson;
				}
			}
		}
		
		return null;
	}
}
