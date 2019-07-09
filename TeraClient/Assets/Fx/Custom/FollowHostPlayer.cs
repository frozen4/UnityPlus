using UnityEngine;

public class FollowHostPlayer : MonoBehaviour
{
    public Transform HostPlayer;

	void LateUpdate () 
    {
        if (null == HostPlayer)
            HostPlayer = Main.HostPalyer;

        if (null != HostPlayer)
            transform.localPosition = HostPlayer.transform.localPosition;
	}
}
