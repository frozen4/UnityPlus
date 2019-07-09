using UnityEngine;
using System.Collections;

public class AQUAS_Visible : MonoBehaviour {

    public AQUAS_Reflection refelection;
    public bool Visible = true; 

	void OnBecameInvisible ()
	{
        //this.gameObject.SetActive(false);
        if (null != refelection)
            refelection.enabled = false;

        Visible = false;
	}
    void OnBecameVisible()
    {
        //this.gameObject.SetActive(true);
        if (null != refelection)
            refelection.enabled = true;

        Visible = true;
    }
}
