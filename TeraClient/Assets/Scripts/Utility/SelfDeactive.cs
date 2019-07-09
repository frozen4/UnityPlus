using UnityEngine;
using System.Collections;

public class SelfDeactive : MonoBehaviour
{

    public float _LifeTime;

    private float _BornTime;
    
    void OnEnable()
    {
        _BornTime = Time.time;
    }

	void Update () 
    {
        if (Time.time - _BornTime > _LifeTime)
            gameObject.SetActive(false);
	}
}
