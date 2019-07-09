using UnityEngine;
using System.Collections.Generic;

namespace EZCameraShake
{
    [AddComponentMenu("EZ Camera Shake/Camera Shaker")]
    public class CameraShaker : MonoBehaviour
    {
        /// <summary>
        /// The single instance of the CameraShake in the current scene. Do not use if you have multiple instances.
        /// </summary>
        public static CameraShaker Instance;
        static Dictionary<string, CameraShaker> instanceList = new Dictionary<string, CameraShaker>();

        /// <summary>
        /// The default position influcence of all shakes created by this shaker.
        /// </summary>
        public Vector3 DefaultPosInfluence = new Vector3(0.15f, 0.15f, 0.15f);
        /// <summary>
        /// The default rotation influcence of all shakes created by this shaker.
        /// </summary>
        public Vector3 DefaultRotInfluence = new Vector3(1, 1, 1);
        
        Vector3 posAddShake, rotAddShake;

        //List<CameraShakeInstance> cameraShakeInstances = new List<CameraShakeInstance>();
        //Dictionary< string, CameraShakeInstance> cameraShakeInstances = new Dictionary<string, CameraShakeInstance>();
        public struct ShakeNode
        {
            public string key;
            public CameraShakeInstance sk;
        }
        List<ShakeNode> cameraShakeInstances = new List<ShakeNode>();
        Hoba.ObjectPool.ObjectPool<CameraShakeInstance> cameraShakeInstancePool = new Hoba.ObjectPool.ObjectPool<CameraShakeInstance>(10, 50, () => { return new CameraShakeInstance(); });

        public Vector3 StartPositon = new Vector3();
		public Vector3 EularAngles = new Vector3();
//        void Awake()
//        {
//            Instance = this;
//            instanceList.Add(gameObject.name, this);
//        }

		void OnEnable()
		{
            Instance = this;
            instanceList.Add(gameObject.name, this);

			StartPositon = transform.localPosition;
            EularAngles = transform.localEulerAngles;
		}

        void OnDisable()
        {
            if (instanceList.ContainsKey(gameObject.name))
                instanceList.Remove(gameObject.name);
        }

        void Update()
        {
			posAddShake = StartPositon;
			rotAddShake = EularAngles;

            for (int i = 0; i < cameraShakeInstances.Count; i++)
            {
                if (i >= cameraShakeInstances.Count)
                    break;

                CameraShakeInstance c = cameraShakeInstances[i].sk;

                if (c.CurrentState == CameraShakeState.Inactive && c.DeleteOnInactive)
                {
                    cameraShakeInstances.RemoveAt(i);
                    i--;
                }
                else if (c.CurrentState != CameraShakeState.Inactive)
                {
                    posAddShake += CameraUtilities.MultiplyVectors(c.UpdateShake(), c.PositionInfluence);
                    rotAddShake += CameraUtilities.MultiplyVectors(c.UpdateShake(), c.RotationInfluence);
                }
            }

            transform.localPosition = posAddShake;
            transform.localEulerAngles = rotAddShake;
        }
        
        public void  ClearShakes()
        {
            cameraShakeInstances.Clear();            
        }

        public bool RemoveShakeNodeByKey(string key)
        {            
            for (int i = 0; i < cameraShakeInstances.Count; i++)
            {
                 if(cameraShakeInstances[i].key == key)
                {
                    cameraShakeInstances.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Gets the CameraShaker with the given name, if it exists.
        /// </summary>
        /// <param name="name">The name of the camera shaker instance.</param>
        /// <returns></returns>
        public static CameraShaker GetInstance(string name)
        {
            CameraShaker c;

            if (instanceList.TryGetValue(name, out c))
                return c;

            Debug.LogError("CameraShake " + name + " not found!");

            return null;
        }

        /// <summary>
        /// Shake the camera once, fading in and out  over a specified durations.
        /// </summary>
        /// <param name="magnitude">The intensity of the shake.</param>
        /// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
        /// <param name="fadeInTime">How long to fade in the shake, in seconds.</param>
        /// <param name="fadeOutTime">How long to fade out the shake, in seconds.</param>
        /// <returns>A CameraShakeInstance that can be used to alter the shake's properties.</returns>
        public CameraShakeInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime, float keepTime, string key)
        {
            //ClearShakes();
            CameraShakeInstance shake = cameraShakeInstancePool.GetObject();
            shake.Set(magnitude, roughness, fadeInTime, fadeOutTime, keepTime);
            shake.PositionInfluence = DefaultPosInfluence;
            shake.RotationInfluence = DefaultRotInfluence;
            AddShakeItem(key, shake);
            shake.SetTimer();

            return shake;
        }

        public void AddShakeItem(string key,  CameraShakeInstance shake )
        {
            ShakeNode node;
            node.key = key;
            node.sk = shake;
            cameraShakeInstances.Add(node);
        }

        /// <summary>
        /// Gets a copy of the list of current camera shake instances.
        /// </summary>
        public List<ShakeNode> ShakeInstances
        {
            get
            {                
                  return new List<ShakeNode>(cameraShakeInstances);
            }
        }
    }
}