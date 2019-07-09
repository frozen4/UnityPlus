using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Newtonsoft.Json;

public class SingularSDK : MonoBehaviour, SingularDeferredDeepLinkHandler
{
    // public properties
    // Get Key & Secret from https://app.singular.net/#/sdk
    public string SingularAPIKey = "kakaogames_fdcfa9e7";
	public string SingularAPISecret = "5b400b84b030e3362a5d8d7fb3f047a9";
	public bool InitializeOnAwake = true;

	public bool autoIAPComplete = false;
	public static bool batchEvents = false;
	public static bool endSessionOnGoingToBackground = false;
	public static bool restartSessionOnReturningToForeground = false;

	public static bool enableDeferredDeepLinks = true;
	public static bool enableLogging = true;
	public static string facebookAppId;
	public static string openUri;

	public long ddlTimeoutSec = 0; // default - 0 - use default timeout (60s)
	public long sessionTimeoutSec = 0; // default - 0 - use default timeout (60s)

	// private properties
	private static bool Initialized = false;

	#if UNITY_ANDROID
	static AndroidJavaClass singular;
	static AndroidJavaClass jclass;
	static AndroidJavaObject activity;
	static AndroidJavaClass jniSingularUnityBridge;
	#endif

	static bool status = false;

	// singleton instance kept here
	private static SingularSDK instance = null;

	public static SingularDeferredDeepLinkHandler registeredDDLHandler = null;
	static System.Int32 cachedDDLMessageTime;
	static string cachedDDLMessage;


	// The Singular SDK is initialized here
	void Awake () {
        //Debug.Log(string.Format("SingularSDK Awake, InitializeOnAwake={0}", InitializeOnAwake));

		if (instance)
			return;
		
		// Initialize singleton
		instance = this;

        // Keep this script running when another scene loads
        DontDestroyOnLoad (gameObject);

		if (InitializeOnAwake) {
			//Debug.Log ("Awake : calling Singular Init");
			InitializeSingularSDK ();
            SetDeferredDeepLinkHandler(this);
        }
	}

	// Only call this if you have disabled InitializeOnAwake
	public static void InitializeSingularSDK() {
		if (Initialized)
			return;

		if (!instance) {
            DeviceLogger.Instance.WriteLog("SingularSDK InitializeSingularSDK, no instance available - cannot initialize");
            Debug.LogError ("SingularSDK InitializeSingularSDK, no instance available - cannot initialize");
            return;
		}

        DeviceLogger.Instance.WriteLogFormat("SingularSDK InitializeSingularSDK, APIKey={0}", instance.SingularAPIKey);
        //Debug.Log (string.Format("SingularSDK InitializeSingularSDK, APIKey={0}", instance.SingularAPIKey));

		#if UNITY_IOS
		StartSingularSession(instance.SingularAPIKey, instance.SingularAPISecret);
		SetAllowAutoIAPComplete_(instance.autoIAPComplete);
		#elif UNITY_ANDROID
		initSDK(instance.SingularAPIKey, instance.SingularAPISecret, facebookAppId, 
			openUri, enableDeferredDeepLinks, instance.ddlTimeoutSec, instance.sessionTimeoutSec, enableLogging);
		#endif

		Initialized = true;
	}
		
	public void Update () { }

	#if UNITY_ANDROID
	private static void initSDK (string APIkey, string secret, string facebookAppId, 
						 string openUri, bool useDeepLinks, long ddlTimeoutSec, long sessionTimeoutSec, bool enableLogging) {
		if (singular == null) {
			singular = new AndroidJavaClass ("com.singular.sdk.Singular");
			jclass = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");

			activity = jclass.GetStatic<AndroidJavaObject> ("currentActivity");

			jniSingularUnityBridge = new AndroidJavaClass ("com.singular.unitybridge.SingularUnityBridge");
			jniSingularUnityBridge.CallStatic ("init", APIkey, secret, facebookAppId, 
								openUri, useDeepLinks, ddlTimeoutSec, sessionTimeoutSec, enableLogging);
		}
	}
	#endif

	private enum NSType {
		STRING = 0,
		INT,
		LONG,
		FLOAT,
		DOUBLE,
		NULL,
		ARRAY,
		DICTIONARY,
	}

	#if UNITY_IOS	
	[DllImport ("__Internal")]
	private static extern bool StartSingularSession_(string key, string secret);

	[DllImport ("__Internal")]
	private static extern bool StartSingularSessionWithLaunchOptions_(string key, string secret);

	[DllImport ("__Internal")]
	private static extern bool StartSingularSessionWithLaunchURL_(string key, string secret, string url);

	[DllImport ("__Internal")]
	private static extern void SendEvent_(string name);

	[DllImport ("__Internal")]
	private static extern void SendEventWithArgs(string name);

	[DllImport ("__Internal")]
	private static extern void EndSingularSession_();

	[DllImport ("__Internal")]
	private static extern void RestartSingularSession_(string key, string secret);

	[DllImport ("__Internal")]
	private static extern void SetAllowAutoIAPComplete_(bool allowed);

	[DllImport ("__Internal")]
	private static extern void SetBatchesEvents_(bool allowed);

	[DllImport ("__Internal")]
	private static extern void SetBatchInterval_(int interval);

	[DllImport ("__Internal")]
	private static extern void SendAllBatches_();

	[DllImport ("__Internal")]
	private static extern void SetAge_(int age);

	[DllImport ("__Internal")]
	private static extern void SetGender_(string gender);

	[DllImport ("__Internal")]
	private static extern string GetAPID_();

	[DllImport ("__Internal")]
	private static extern string GetIDFA_();

	// Revenue functions
	[DllImport ("__Internal")]
	private static extern void Revenue_(string currency, double amount);

	[DllImport ("__Internal")]
	private static extern void RevenueWithAllParams_(string currency, double amount,  string productSKU,  string productName,  string productCategory, int productQuantity, double productPrice);


	// Auxiliary functions;
	[DllImport ("__Internal")]
	private static extern void Init_NSDictionary();

	[DllImport ("__Internal")]
	private static extern void Init_NSMasterArray();

	[DllImport ("__Internal")]
	private static extern void Push_NSDictionary(string key, string value, int type);

	[DllImport ("__Internal")]
	private static extern void Free_NSDictionary();

	[DllImport ("__Internal")]
	private static extern void Free_NSMasterArray();

	[DllImport ("__Internal")]
	private static extern int New_NSDictionary();

	[DllImport ("__Internal")]
	private static extern int New_NSArray();

	[DllImport ("__Internal")]
	private static extern void Push_Container_NSDictionary(string key, int containerIndex);

	[DllImport ("__Internal")]
	private static extern void Push_To_Child_Dictionary(string key, string value, int type, int dictionaryIndex);

	[DllImport ("__Internal")]
	private static extern void Push_To_Child_Array(string value,int type, int arrayIndex);

	[DllImport ("__Internal")]
	private static extern void Push_Container_To_Child_Dictionary(string key, int dictionaryIndex, int containerIndex);

	[DllImport ("__Internal")]
	private static extern void Push_Container_To_Child_Array(int arrayIndex, int containerIndex);

	[DllImport ("__Internal")]
	private static extern void RegisterDeviceTokenForUninstall_(string APNSToken);

	[DllImport ("__Internal")]
	private static extern void RegisterDeferredDeepLinkHandler_();

	[DllImport ("__Internal")]
	private static extern int SetDeferredDeepLinkTimeout_(int duration);

	[DllImport ("__Internal")]
	private static extern void SetCustomUserId_(string customUserId);

	[DllImport ("__Internal")]
	private static extern void UnsetCustomUserId_();

	[DllImport ("__Internal")]
	private static extern void TrackingOptIn_();

	[DllImport ("__Internal")]
	private static extern void TrackingUnder13_();

	[DllImport ("__Internal")]
	private static extern void StopAllTracking_();

	[DllImport ("__Internal")]
	private static extern void ResumeAllTracking_();

	[DllImport ("__Internal")]
	private static extern bool IsAllTrackingStopped_();

	private static void CreateDictionary(int parent, NSType parentType, string key, Dictionary<string,object> source) {
		int dictionaryIndex = New_NSDictionary();

		Dictionary<string,object>.Enumerator enumerator = source.GetEnumerator();

		while(enumerator.MoveNext()) {
			//test if string,int,float,double,null;
			NSType type = NSType.STRING;
			if (enumerator.Current.Value == null) {
				type = NSType.NULL;
				Push_To_Child_Dictionary(enumerator.Current.Key,"",(int)type,dictionaryIndex);
			} else {
				System.Type valueType = enumerator.Current.Value.GetType();

				if(valueType == typeof(int)) {
					type = NSType.INT;
				} else if(valueType == typeof(long)) {
					type = NSType.LONG;
				} else if(valueType == typeof(float)) {
					type = NSType.FLOAT;
				} else if(valueType == typeof(double)) {
					type = NSType.DOUBLE;
				} else if(valueType == typeof(Dictionary<string,object>)) {
					type = NSType.DICTIONARY;
					CreateDictionary(dictionaryIndex,NSType.DICTIONARY,enumerator.Current.Key,(Dictionary<string,object>)enumerator.Current.Value);
				} else if(valueType == typeof(ArrayList)) {
					type = NSType.ARRAY;
					CreateArray(dictionaryIndex,NSType.DICTIONARY,enumerator.Current.Key,(ArrayList)enumerator.Current.Value);
				}

				if ((int)type < (int)NSType.ARRAY) {
					Push_To_Child_Dictionary(enumerator.Current.Key,enumerator.Current.Value.ToString(),(int)type,dictionaryIndex);
				}
			}
		}

		if(parent < 0) {
			Push_Container_NSDictionary(key,dictionaryIndex);
		} else {
			if(parentType == NSType.ARRAY) {
				Push_Container_To_Child_Array(parent,dictionaryIndex);
			} else {
				Push_Container_To_Child_Dictionary(key,parent,dictionaryIndex);
			}
		}
	}

	private static void CreateArray(int parent, NSType parentType, string key, ArrayList source) {
		int arrayIndex = New_NSArray();

		foreach(object o in source) {
			//test if string,int,float,double,null;
			NSType type = NSType.STRING;

			if(o == null){
				type = NSType.NULL;
				Push_To_Child_Array("",(int)type,arrayIndex);
			} else {
				System.Type valueType = o.GetType();

				if(valueType == typeof(int)) {
					type = NSType.INT;
				} else if (valueType == typeof(long)) {
					type = NSType.LONG;
				} else if(valueType == typeof(float)) {
					type = NSType.FLOAT;
				} else if(valueType == typeof(double)) {
					type = NSType.DOUBLE;
				} else if(valueType == typeof(Dictionary<string,object>)) {
					type = NSType.DICTIONARY;
					CreateDictionary(arrayIndex,NSType.ARRAY,"",(Dictionary<string,object>)o);
				} else if(valueType == typeof(ArrayList)) {
					type = NSType.ARRAY;
					CreateArray(arrayIndex,NSType.ARRAY,"",(ArrayList)o);
				}

				if((int)type < (int)NSType.ARRAY) {
					Push_To_Child_Array(o.ToString(),(int)type,arrayIndex);
				}
			}
		}

		if(parent < 0) {
			Push_Container_NSDictionary(key,arrayIndex);
		} else {
			if(parentType == NSType.ARRAY){
				Push_Container_To_Child_Array(parent,arrayIndex);
			}else{
				Push_Container_To_Child_Dictionary(key,parent,arrayIndex);
			}
		}
	}

	#endif

	public static bool StartSingularSession(string key, string secret) {
		if(!Application.isEditor) {
			#if UNITY_IOS
			RegisterDeferredDeepLinkHandler_();
			return StartSingularSession_(key,secret);
			#endif
		}

		return false;
	}

	public static bool StartSingularSessionWithLaunchOptions(string key, string secret, Dictionary<string,object> options) {
		if(!Application.isEditor){
			#if UNITY_IOS
			Init_NSDictionary();
			Init_NSMasterArray();

			Dictionary<string,object>.Enumerator enumerator = options.GetEnumerator();

			while (enumerator.MoveNext()) {
				NSType type = NSType.STRING;

				if(enumerator.Current.Value == null) {
					type = NSType.NULL;
					Push_NSDictionary(enumerator.Current.Key,"",(int)type);
				} else {
					System.Type valueType = enumerator.Current.Value.GetType();

					if(valueType == typeof(int)) {
						type = NSType.INT;
					}else if(valueType == typeof(long)) {
						type = NSType.LONG;
					}else if(valueType == typeof(float)) {
						type = NSType.FLOAT;
					}else if(valueType == typeof(double)) {
						type = NSType.DOUBLE;
					}else if(valueType == typeof(Dictionary<string,object>)) {	
						type = NSType.DICTIONARY;
						CreateDictionary(-1,NSType.DICTIONARY,enumerator.Current.Key,(Dictionary<string,object>)enumerator.Current.Value);
					} else if(valueType == typeof(ArrayList)){
						type = NSType.ARRAY;
						CreateArray(-1,NSType.DICTIONARY,enumerator.Current.Key,(ArrayList)enumerator.Current.Value);
					}

					if ((int)type < (int)NSType.ARRAY) {
						Push_NSDictionary(enumerator.Current.Key,enumerator.Current.Value.ToString(),(int)type);
					}
				}
			}

			StartSingularSessionWithLaunchOptions_(key,secret);

			Free_NSDictionary();
			Free_NSMasterArray();

			return true;
			#endif
		}
		return false;
	}

	public static bool StartSingularSessionWithLaunchURL(string key, string secret, string url) {
		if (!Application.isEditor) {
			#if UNITY_IOS
			return StartSingularSessionWithLaunchURL_(key,secret,url);
			#endif
		}
		return false;
	}


	public static void RestartSingularSession(string key, string secret) {
		if (!Application.isEditor) {
			#if UNITY_IOS
			#elif UNITY_ANDROID
			if (singular != null) {
				singular.CallStatic("onActivityResumed");
			}
			#endif
		}
	}

	public static void EndSingularSession() {
		if (!Application.isEditor) {
			#if UNITY_IOS
			#elif UNITY_ANDROID
			if (singular != null) {
				singular.CallStatic("onActivityPaused"); 
			}
			#endif
		}
	}

	public static void Event(string name) {
		if (!Initialized)
			return;
		
		if (!Application.isEditor) {
			#if UNITY_IOS
			SendEvent_(name);
			#elif UNITY_ANDROID	
			if (singular != null) {
				status = singular.CallStatic<bool> ("isInitialized");
				singular.CallStatic<bool>("event", name);
			}
			#endif
		}
	}

	/*
	dictionary is first parameter, because the compiler must be able to see a difference between
	SendEventWithArgs(Dictionary<string,object> args,string name) 
	and
	public static void SendEventsWithArgs(string name, params object[] args)
	the elements in the ArrayList and values in the Dictionary must have one of these types:
	  string, int, long, float, double, null, ArrayList, Dictionary<String,object>
	*/
	public static void Event(Dictionary<string,object> args, string name) {
		Debug.Log (string.Format("SingularSDK Event: args JSON={0}", JsonConvert.SerializeObject(args, Formatting.None)));

		if (!Initialized)
			return;
		
		if (!Application.isEditor) {
			#if UNITY_IOS
			Init_NSDictionary();
			Init_NSMasterArray();

			Dictionary<string,object>.Enumerator enumerator = args.GetEnumerator();

			while (enumerator.MoveNext()) {
				NSType type = NSType.STRING;
				
				if (enumerator.Current.Value == null) {
					type = NSType.NULL;
					Push_NSDictionary(enumerator.Current.Key,"",(int)type);
				} else {
					System.Type valueType = enumerator.Current.Value.GetType();

					if(valueType == typeof(int)) {
						type = NSType.INT;
					} else if(valueType == typeof(long)) {
						type = NSType.LONG;
					} else if(valueType == typeof(float)) {
						type = NSType.FLOAT;
					} else if(valueType == typeof(double)) {
						type = NSType.DOUBLE;
					} else if(valueType == typeof(Dictionary<string,object>)) {	
						type = NSType.DICTIONARY;
						CreateDictionary(-1,NSType.DICTIONARY,enumerator.Current.Key,(Dictionary<string,object>)enumerator.Current.Value);
					} else if(valueType == typeof(ArrayList)) {
						type = NSType.ARRAY;
						CreateArray(-1,NSType.DICTIONARY,enumerator.Current.Key,(ArrayList)enumerator.Current.Value);
					}
					if ((int)type < (int)NSType.ARRAY) {
						Push_NSDictionary(enumerator.Current.Key,enumerator.Current.Value.ToString(),(int)type);
					}
				}
			}

			SendEventWithArgs(name);
			Free_NSDictionary();
			Free_NSMasterArray();
			#elif UNITY_ANDROID
			AndroidJavaObject json = new AndroidJavaObject("org.json.JSONObject", JsonConvert.SerializeObject(args, Formatting.None));

			if (singular != null) {
				status =  singular.CallStatic<bool>("eventJSON", name, json);
			}
			#endif
		}
	}

	/* 
	allowed argumenst are: string, int, long, float, double, null, ArrayList, Dictionary<String,object>
	the elements in the ArrayList and values in the Dictionary must have one of these types:
	string, int, long, float, double, null, ArrayList, Dictionary<String,object>
    */

	public static void Event(string name, params object[] args) {
		if (!Initialized)
			return;
		
		if(!Application.isEditor) {
		#if UNITY_IOS || UNITY_ANDROID
			if(args.Length %2 != 0) {
				// Debug.LogWarning("The number of arguments is ann odd number. The arguments are key-value pairs so the number of arguments should be even.");
			} else {
				Dictionary<string,object> dict = new Dictionary<string,object>();

				for(int i = 0; i < args.Length; i+=2) {
					dict.Add(args[i].ToString(),args[i+1]);
				}

				Event(dict, name);
			}
		#endif
		}
	}

	public static void SetAge(int age) {
		if (!Initialized)
			return;
		
		if(Mathf.Clamp(age,0,100) != age) {
			Debug.Log("Age " + age + "is not between 0 and 100");
			return;
		}
		#if UNITY_IOS
		if (!Application.isEditor){
			SetAge_(age);
		}		
		#endif
	}

	public static void SetGender(string gender) {
		if (!Initialized)
			return;
		
		if(gender != "m" && gender != "f") {
			Debug.Log("gender " + gender + "is not m or f");
			return;
		}
		#if UNITY_IOS
		if (!Application.isEditor) {
			SetGender_(gender);
		}
		#endif
	}

	public static void SetAllowAutoIAPComplete(bool allowed) {
		#if UNITY_IOS
		if (!Application.isEditor) {
			SetAllowAutoIAPComplete_(allowed);
		}

		if (instance != null) {
			instance.autoIAPComplete = allowed;
		}
		#elif UNITY_ANDROID
		if (Application.isEditor) {
			Debug.Log("SetAllowAutoIAPComplete is not supported on Android");
		}
		#endif
	}

	void OnApplicationPause(bool paused) {
		if (!Initialized || !instance)
			return;

		#if UNITY_IOS || UNITY_ANDROID
		if (paused) { //Application goes to background.
			if(!Application.isEditor) {
				if (endSessionOnGoingToBackground) {
					EndSingularSession();
				}
			}
		} else { //Application did become active again.
			if (!Application.isEditor) {
				if (restartSessionOnReturningToForeground) {
					RestartSingularSession(instance.SingularAPIKey, instance.SingularAPISecret);
				}
			}
		}
		#endif
	}

	void OnApplicationQuit() {
		if (!Initialized)
			return;
		
		#if UNITY_IOS || UNITY_ANDROID
		EndSingularSession();
		#endif
	}

	public static void SetDeferredDeepLinkHandler(SingularDeferredDeepLinkHandler ddlHandler){
		if (!instance) {
			Debug.LogError ("SingularSDK SetDeferredDeepLinkHandler, no instance available - cannot set deferred deeplink handler!");
			return;
		}

		registeredDDLHandler = ddlHandler;
		System.Int32 now = (System.Int32)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;

		// call the ddl handler with the cached value if the timeout has not passed yet
		if (now - cachedDDLMessageTime < instance.ddlTimeoutSec && cachedDDLMessage != null) {
			registeredDDLHandler.OnDeferredDeepLink (cachedDDLMessage);
		}
	}

	// this is the internal handler - handling deeplinks for both iOS & Android
	public void DeepLinkHandler(string message) {
		Debug.Log (string.Format("SingularSDK DeepLinkHandler called! message='{0}'", message));

		if (message == "") {
			message = null;
		}
		if (registeredDDLHandler != null) {
			registeredDDLHandler.OnDeferredDeepLink(message);
		}else {
			cachedDDLMessage = message;
			cachedDDLMessageTime = (System.Int32)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
		};
    }

    public void OnDeferredDeepLink(string deepLink)
    {
        //TODO
        DeviceLogger.Instance.WriteLogFormat("[Singular]OnDeferredDeepLink: {0}", deepLink);
    }

    public static void RegisterDeviceTokenForUninstall(string APNSToken) {
		#if UNITY_IOS
		if (!Application.isEditor) {
			if (APNSToken.Length % 2 != 0)
			{
				Debug.Log("RegisterDeviceTokenForUninstall: token must be an even-length hex string!");
				return;
			}

			RegisterDeviceTokenForUninstall_(APNSToken);
		}
		#elif UNITY_ANDROID
		Debug.Log("RegisterDeviceTokenForUninstall is supported only for iOS");
		#endif
	}


	public static string GetAPID() {
		//only works for iOS. Will return null until Singular is initialized.
		#if UNITY_IOS
		if (!Application.isEditor) {
			return GetAPID_();
		}
		#endif
		return null;
	}

	public static string GetIDFA() {
		//only works for iOS. Will return null until Singular is initialized.
		#if UNITY_IOS
		if (!Application.isEditor) {
			return GetIDFA_();
		}
		#endif
		return null;
	}

	public static void Revenue(string currency, double amount) {
		#if UNITY_IOS
		Revenue_(currency, amount);
		#elif UNITY_ANDROID
		if (singular != null) {
			singular.CallStatic<bool>("revenue",currency,amount);
		}
		#endif
	}

	public static void Revenue(string currency, double amount, string productSKU, string productName, string productCategory, int productQuantity, double productPrice) {
		#if UNITY_IOS
		RevenueWithAllParams_(currency, amount, productSKU, productName, productCategory, productQuantity, productPrice);
		#elif UNITY_ANDROID
		if (singular != null) {
			singular.CallStatic<bool>("revenue",currency, amount, productSKU, productName, productCategory, productQuantity, productPrice);
		}
		#endif
	}

	public static void SetFCMDeviceToken(string fcmDeviceToken) {
		#if UNITY_IOS

		#elif UNITY_ANDROID
		if (singular != null) {
			singular.CallStatic("setFCMDeviceToken",fcmDeviceToken);
		}
		#endif
	}

	public static void SetGCMDeviceToken(string gcmDeviceToken) {
		#if UNITY_IOS

		#elif UNITY_ANDROID
		if (singular != null) {
			singular.CallStatic("setGCMDeviceToken",gcmDeviceToken);
		}
		#endif
	}

	public static void SetCustomUserId(string customUserId) {
		#if UNITY_IOS
		SetCustomUserId_(customUserId);
		#elif UNITY_ANDROID
		if (singular != null) {
		singular.CallStatic("setCustomUserId",customUserId);
		}
		#endif
	}

	public static void UnsetCustomUserId() {
		#if UNITY_IOS
		UnsetCustomUserId_();
		#elif UNITY_ANDROID
		if (singular != null) {
		singular.CallStatic("unsetCustomUserId");
		}
		#endif
	}

	public static void TrackingOptIn() {
		#if UNITY_IOS
		TrackingOptIn_();
		#elif UNITY_ANDROID
		if (singular != null) {
			singular.CallStatic("trackingOptIn");
		}
		#endif
	}

	public static void TrackingUnder13() {
		#if UNITY_IOS
		TrackingUnder13_();
		#elif UNITY_ANDROID
		if (singular != null) {
			singular.CallStatic("trackingUnder13");
		}
		#endif
	}

	public static void StopAllTracking() {
		#if UNITY_IOS
		StopAllTracking_();
		#elif UNITY_ANDROID
		if (singular != null) {
			singular.CallStatic("stopAllTracking");
		}
		#endif
	}

	public static void ResumeAllTracking() {
		#if UNITY_IOS
		ResumeAllTracking_();
		#elif UNITY_ANDROID
		if (singular != null) {
			singular.CallStatic("resumeAllTracking");
		}
		#endif
	}

	public static bool IsAllTrackingStopped() {
		#if UNITY_IOS
		return IsAllTrackingStopped_();
		#elif UNITY_ANDROID
		if (singular != null) {
			return singular.CallStatic<bool>("isAllTrackingStopped");
		}
		#endif

		return false;
	}
}
