//using JetBrains.Annotations;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(T)) as T;
            }
            if (!_instance)
            {
                //Debug.LogWarning("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
                _instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }
            return _instance;
        }
    }
}