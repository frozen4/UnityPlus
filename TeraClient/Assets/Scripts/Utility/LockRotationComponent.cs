using UnityEngine;
public class LockRotationComponent : MonoBehaviour
{
    public Transform Root { get; set; }

    private void LateUpdate()
    {
        //if (transform.parent != null)
        //{
        //    Vector3 temp = transform.parent.eulerAngles;
        //    temp.z = 0;
        //    float rotation_y = Mathf.Repeat(transform.parent.localEulerAngles.y, 360f);
        //    if (_Root == null)
        //        temp.y = 180;
        //    else
        //        temp.y = _Root.eulerAngles.y;
        //    transform.rotation = Quaternion.Euler(temp);
        //}
        if (transform.parent != null && Root != null)
        {
            Vector3 temp = transform.parent.eulerAngles;
            temp.z = 0;
            temp.y = Root.eulerAngles.y;
            transform.rotation = Quaternion.Euler(temp);
        }
    }
}
