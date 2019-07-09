using UnityEngine;
//namespace Fx
//{
public class ReusableFx : MonoBehaviour
{
    public virtual void SetActive(bool active)
    {
        enabled = active;
    }

    public virtual void Tick(float delta) {}

    public virtual void LateTick(float delta) {}
}
//}