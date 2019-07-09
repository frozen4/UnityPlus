//
//  IAPManager.cs
//  Unity-iPhone
//
//  Created by 周凯 on 2018/6/11.
//

#if PLATFORM_KAKAO
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Text;
using Common;

public class IAPManager : Singleton<IAPManager>, IStoreListener {
#if UNITY_IOS || UNITY_IPHONE
	[DllImport ("__Internal")]  
	private static extern void _save(string key, string str);
	[DllImport ("__Internal")]  
	private static extern bool _hasInfo(string key);
	[DllImport ("__Internal")]  
	private static extern string _load(string key);
	[DllImport ("__Internal")]  
	private static extern void _delete(string key);
#endif
    public class PURCHASE_INFO
    {
        public int iBillingType;
        public string strOrderId = string.Empty;
        public string strProductId = string.Empty;
        public string strTransactionId = string.Empty;
        public string strReceipt = string.Empty;
    }

    public delegate void LT_PURCHASE_NOTIFICATION_DELEGATE(bool bSuccess, PURCHASE_INFO pProduct);
	private static IStoreController _storeController = null;
	private static IExtensionProvider _extensionProvider = null;
    private static int _roleId = 0;

	private const string IAP_RECEIPT_SERVICE = "IAP_RECEIPT_SERVICE";
    private string VERIFY_URL = string.Empty;
    //private const string VERIFY_URL = "http://10.35.51.22:11000/api/iap/verify";
    private static LT_PURCHASE_NOTIFICATION_DELEGATE _fnPurchaseCallback = null;
    private List<string> listPid = null;

    public void InitPurchaseVerifyUrl(string verifyUrl)
    {
        VERIFY_URL = string.Format("{0}/api/iap/verify", verifyUrl);
        Debug.Log(string.Format("init url = {0}", VERIFY_URL));
    }

    public void SetProductIds(List<string> pids)
    {
        listPid = pids;
    }

    public void InitializeIAP(int roleID, LT_PURCHASE_NOTIFICATION_DELEGATE fnPurchaseCallback)
	{
        //RoleID
        _roleId = roleID;

        if (IsInitialized ())
			return;

        //first check cache ReProcessing
        ProcessPurchaseCache();

        Debug.Log ("InitializeIAP");
        var module = StandardPurchasingModule.Instance();
		ConfigurationBuilder builder = ConfigurationBuilder.Instance (module);

		//Add productID List
		SetProductList (builder);
		UnityPurchasing.Initialize (this, builder);

        _fnPurchaseCallback = fnPurchaseCallback;
    }

    private void SetProductList(ConfigurationBuilder builder)
	{
        int count = listPid.Count;
#if UNITY_IOS || UNITY_IPHONE
        /*
        builder.AddProduct("tc_ios_diamond_blue640", ProductType.Consumable, new IDs {{"tc_ios_diamond_blue640", AppleAppStore.Name}});
        builder.AddProduct("tc_ios_diamond_blue1920", ProductType.Consumable, new IDs {{"tc_ios_diamond_blue1920", AppleAppStore.Name}});
        builder.AddProduct("tc_ios_diamond_blue3200", ProductType.Consumable, new IDs { { "tc_ios_diamond_blue3200", AppleAppStore.Name } });
        builder.AddProduct("tc_ios_diamond_blue6400", ProductType.Consumable, new IDs { { "tc_ios_diamond_blue6400", AppleAppStore.Name } });
        */
        for (int i = 0; i < count; ++i)
        {
            var pid = listPid[i];
            //Debug.Log(string.Format("iOS ProductId = {0}", pid));
            builder.AddProduct(pid, ProductType.Consumable, new IDs { { pid, AppleAppStore.Name } });
        }
#elif UNITY_ANDROID
        /*
        builder.AddProduct("tc_aos_diamond_blue640", ProductType.Consumable, new IDs {{"tc_aos_diamond_blue640", GooglePlay.Name}});
        builder.AddProduct("tc_aos_diamond_blue1920", ProductType.Consumable, new IDs {{"tc_aos_diamond_blue1920", GooglePlay.Name}});
        builder.AddProduct("tc_aos_diamond_blue3200", ProductType.Consumable, new IDs { { "tc_aos_diamond_blue3200", GooglePlay.Name } });
        builder.AddProduct("tc_aos_diamond_blue6400", ProductType.Consumable, new IDs { { "tc_aos_diamond_blue6400", GooglePlay.Name } });
        */
        for (int i = 0; i < count; ++i)
        {
            var pid = listPid[i];
            //Debug.Log(string.Format("Android ProductId = {0}", pid));
            builder.AddProduct(pid, ProductType.Consumable, new IDs { { pid, GooglePlay.Name } });
        }
#endif
    }


    /* ===================================================
	 * 
	 * Client Function
	 * 
	 * =================================================== */
    bool IsInitialized()
	{
		return (_storeController != null && _extensionProvider != null);
	}

    public void BuyProductByID(int purchaseType, string orderId, string productId)
	{
		Debug.Log (string.Format("BuyProductByID: {0}", productId));
        if (!IsInitialized ())
		{
			Debug.Log ("BuyProductByID Failed! Not Initialized!");

            PURCHASE_INFO info = new PURCHASE_INFO();
            info.iBillingType = purchaseType;
            info.strOrderId = orderId;
            info.strProductId = productId;
            _fnPurchaseCallback(false, info);

            return;
		}

        Debug.Log("SDK::通用接口，直接开启订单");
        Product product = _storeController.products.WithID (productId);
        if (product == null)
            Debug.Log("product is null");
        if (product != null && product.availableToPurchase == false)
            Debug.Log("product is not availableToPurchase");

        if (product != null && product.availableToPurchase)
        {
            Debug.Log(string.Format("直接开启订单: {0}", product.definition.id));
            //开启支付
            _storeController.InitiatePurchase(product, orderId);
        }
	}

    public void RestorePurchase()
	{
		if (!IsInitialized ())
		{
			Debug.Log ("RestorePurchase Failed! Not Initialized!");
			return;
		}
		if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
		{
			Debug.Log ("RestorePurchase...");
			var appleStore = _extensionProvider.GetExtension<IAppleExtensions> ();
			appleStore.RestoreTransactions ((result) => 
				{
					Debug.Log(string.Format("RestorePurchase result = [{0}]", result));
				});
		}
		else
		{
			Debug.Log(string.Format("RestorePurchase Not available on Platform:{0}", Application.platform));
		}
	}



	/* ===================================================
	 * 
	 * IStoreListener
	 * 
	 * =================================================== */

	public void OnInitialized (IStoreController controller, IExtensionProvider extensions)
	{
		Debug.Log ("IAP::OnInitialized Succeed!");
		_storeController = controller;
		_extensionProvider = extensions;
    }

	public void OnInitializeFailed (InitializationFailureReason error)
	{
		Debug.Log (string.Format("IAP::OnInitializeFailed {0}", error));
    }

    public void OnPurchaseFailed (Product product, PurchaseFailureReason p)
	{
        Debug.Log(string.Format("IAP::OnPurchaseFailed PurchaseFailureReason:{0}", p));
        int purchaseType = 0;
        string udid = "";

#if UNITY_IOS || UNITY_IPHONE
        purchaseType = 1;   //Apple Store  EPlatformType.EPlatformType_AppStore
        udid = IOSUtil.iosGetOpenUDID();
#elif UNITY_ANDROID
        purchaseType = 2;   //Google Play  EPlatformType.EPlatformType_GooglePlay
        udid = AndroidUtil.GetOpenUDID();
#endif
        var transactionID = product.transactionID;
        var pid = product.definition.id;

        //Write cache
        FileBilling.Instance.ReadFile();
        FileBilling.CReceiptInfo entry = new FileBilling.CReceiptInfo();
        entry.RoleId = _roleId;
        entry.ProductId = pid;
        entry.BillingType = purchaseType;
        entry.IsSucceed = false;
        entry.TransactionId = transactionID;
        FileBilling.Instance.Update(purchaseType, entry);
        FileBilling.Instance.WriteFile();

        PostVerifyData(_roleId, _roleId, purchaseType, pid, transactionID, udid, "", "", false, false);

        // Callback
        PURCHASE_INFO info = new PURCHASE_INFO();
        info.iBillingType = purchaseType;
        info.strTransactionId = product.transactionID;
        _fnPurchaseCallback(false, info);
    }

	public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs e)
	{
		//获取并解析你需要上传的数据。解析成string类型
		var wrapper = (Dictionary<string, object>) MiniJson.JsonDecode (e.purchasedProduct.receipt);
		if (null == wrapper) {
			return PurchaseProcessingResult.Complete;
		}
		var store = (string)wrapper ["Store"];
		//下面的payload 即为IOS的验证商品信息的数据。即我们需要上传的部分。
		var payload = (string)wrapper ["Payload"]; // For Apple this will be the base64 encoded ASN.1 receipt
		var pid = e.purchasedProduct.definition.id;
		var transactionID = e.purchasedProduct.transactionID;

        int purchaseType = 0;
        string receipt = "";
        string udid = OSUtility.GetOpenUDID();
        string signature = "";
#if UNITY_IOS || UNITY_IPHONE
        purchaseType = 1;   //Apple Store  EPlatformType.EPlatformType_AppStore
        receipt = payload;

		Debug.Log(string.Format("PID = {0} Payload = {1}", pid, receipt));
#elif UNITY_ANDROID
        purchaseType = 2;   //Google Play  EPlatformType.EPlatformType_GooglePlay

        var gpDetails = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
		receipt = (string)gpDetails["signature"];       // 服务器待验证 签名数据
        signature = (string)gpDetails["json"];          // 服务器待验证 订单信息
#endif

        //Write cache
        FileBilling.Instance.ReadFile();
        FileBilling.CReceiptInfo entry = new FileBilling.CReceiptInfo();
        entry.RoleId = _roleId;
        entry.ProductId = pid;
        entry.IsSucceed = true;
        entry.BillingType = purchaseType;
        entry.Receipt = payload;
        entry.TransactionId = transactionID;
        FileBilling.Instance.Update(purchaseType, entry);
        FileBilling.Instance.WriteFile();

        // Post receipt for verify
        bool bPostSucceed = PostVerifyData(_roleId, _roleId, purchaseType, pid, transactionID, udid, receipt, signature, false, true);

        PURCHASE_INFO info = new PURCHASE_INFO();
        info.iBillingType = purchaseType;
        info.strTransactionId = transactionID;
        info.strProductId = pid;
        info.strReceipt = receipt;
        _fnPurchaseCallback(true, info);

        return PurchaseProcessingResult.Complete;
    }

    private bool PostVerifyData(int roleId, int roleIdCache, int purchaseType, string pid, string transactionId, string udid, string receipt, string signature, bool isCache, bool isSucceed)
    {
        Debug.Log("PostVerifyData...");

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add("RoleId", roleId.ToString());
        dic.Add("IsSucceed", isSucceed ? "1" : "0");
        dic.Add("RoleIdCache", roleIdCache.ToString());
        dic.Add("PurchaseType", purchaseType.ToString());
        dic.Add("ProductId", pid);
        dic.Add("TransactionID", transactionId);
        dic.Add("DeviceID", udid);
        dic.Add("Receipt", receipt);
        dic.Add("SignatureData", signature);

        dic.Add("IsCache", isCache ? "1" : "0");
        string retPost = Post(VERIFY_URL, dic);

        if (retPost.Contains("OK"))
        {
            // Del Cache
            FileBilling.Instance.ReadFile();
            FileBilling.Instance.Remove(purchaseType);
            FileBilling.Instance.WriteFile();

            return true;
        }
        else
        {
            Debug.Log(string.Format("Verify Post Error = {0}", retPost));
            return false;
        }
    }

    public void ProcessPurchaseCache()
    {
        int purchaseType = 0;
#if UNITY_IOS || UNITY_IPHONE
        purchaseType = 1;   //Apple Store  EPlatformType.EPlatformType_AppStore
#elif UNITY_ANDROID
        purchaseType = 2;   //Google Play  EPlatformType.EPlatformType_GooglePlay
#endif
        FileBilling.Instance.ReadFile();
        var entry = FileBilling.Instance.Get(purchaseType);
        if (entry == null)
            return;

        Debug.Log("=================ProcessPurchaseCache==================");
        string udid = OSUtility.GetOpenUDID();
        string signature = "";
        string receipt = entry.Receipt;
        bool isSucceed = entry.IsSucceed;
        string pid = entry.ProductId;
        string transactionID = entry.TransactionId;
        int roleIdCache = entry.RoleId;
#if UNITY_IOS || UNITY_IPHONE

#elif UNITY_ANDROID
        if (isSucceed)
        {
            // 成功后解析需要的订单信息
            var gpDetails = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
            receipt = (string)gpDetails["signature"];       // 服务器待验证 签名数据
            signature = (string)gpDetails["json"];          // 服务器待验证 订单信息
        }
#endif

        // Post receipt for verify
        PostVerifyData(_roleId, roleIdCache, purchaseType, pid, transactionID, udid, receipt, signature, true, isSucceed);
    } 

    public string Post(string url, Dictionary<string, string> dic)
    {
        string result = "";

        try
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            #region 添加Post 参数
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
        }
        catch(WebException e)
        {
            result = string.Format("ERROR: WebException {0}", e.ToString());
        }

        return result;
    }
}
#endif