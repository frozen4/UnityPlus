using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Common;

public partial class EntryPoint : MonoBehaviour
{
    static EntryPoint _Instance;

    public static EntryPoint Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = GameObject.FindObjectOfType(typeof(EntryPoint)) as EntryPoint;
            if (_Instance == null)
                _Instance = new GameObject("EntryPoint").AddComponent<EntryPoint>();
            return _Instance;
        }
    }

    public LogLevel _WriteLogLevel;
    public string _ResPath = string.Empty;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
	
	 void Start()
    {
	}
}