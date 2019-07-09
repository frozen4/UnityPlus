using UnityEngine;
using GameLogic;

namespace EntityComponent
{
    public class CPhysicsEventTransfer : MonoBehaviour, IRecyclable
    {
        private ObjectBehaviour _ObjectComp;

        public void Link(ObjectBehaviour ob)
        {
            _ObjectComp = ob;
            ob.PhysicsHandler = this;
        }

        public void DieFly(Vector3 dir, float force, float mass = 1)
        {
            Collider cd = gameObject.GetComponent<Collider>();
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (cd != null && rb != null)
            {
                cd.isTrigger = false;
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.mass = mass;
                //rb.AddForce(force * dir);
                rb.AddExplosionForce(force, dir, 5f, 3.0f);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_ObjectComp != null && other.gameObject.tag == "Weapon")
            {
                Vector3 hit_pos = Vector3.zero;
                Transform hit_point = other.gameObject.transform.Find("JianCe");
                if (hit_point != null)
                    hit_pos = hit_point.position;

                _ObjectComp.OnPhysicsEventEnter(other.gameObject, hit_pos);
            }
        }

        public void OnRecycle()
        {
            
        }
    }
}

