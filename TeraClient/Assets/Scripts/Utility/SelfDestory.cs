using UnityEngine;
using System.Collections;

public class SelfDestory : MonoBehaviour {

    public float _LifeTime;

	private float _expireTime;
	void Start () 
    {
		_expireTime = Time.time + _LifeTime;
	}
	
	// Update is called once per frame
	void Update () 
    {
		if (Time.time > _expireTime)
		{
			Destroy(gameObject);
		}
	}
}
