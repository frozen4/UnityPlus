using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;

public class FileBilling : Singleton<FileBilling>
{
    public class CReceiptInfo
    {
        public int BillingType = 0;
        public int RoleId = 0;
        public bool IsSucceed = false;
        public string OrderId = string.Empty;
        public string ProductId = string.Empty;
        public string TransactionId = string.Empty;
        public string Receipt = string.Empty;
    }

    public Dictionary<int, CReceiptInfo> _DicEntries = new Dictionary<int, CReceiptInfo>();

    public void Update(int billingType, CReceiptInfo entry)
    {
        if (_DicEntries.ContainsKey(billingType))
            _DicEntries[billingType] = entry;
        else
            _DicEntries.Add(billingType, entry);
    }
    public void Remove(int billingType)
    {
        if (_DicEntries.ContainsKey(billingType))
            _DicEntries.Remove(billingType);
    }

    public CReceiptInfo Get(int billingType)
    {
        CReceiptInfo ret;
        if (_DicEntries.TryGetValue(billingType, out ret))
            return ret;
        return null;
    }

    public bool ReadFile()
    {
        return ReadFile(EntryPoint.Instance.UserBillingFile);
    }

    public bool WriteFile()
    {
        return WriteFile(EntryPoint.Instance.UserBillingFile);
    }

    public void DeleteFile()
    {
        try
        {
            File.Delete(EntryPoint.Instance.UserBillingFile);
        }
        catch(Exception)
        {

        }
    }

    public bool ReadFile(string filename)
    {
        _DicEntries.Clear();

        if (!File.Exists(filename))
            return false;

        try
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int num = br.ReadInt32();

                    for(int i = 0; i < num; ++i)
                    {
                        int billingType = br.ReadInt32();
                        int roldId = br.ReadInt32();
                        bool isSucceed = br.ReadBoolean();
                        string orderId = Util.ReadString(br);
                        string productId = Util.ReadString(br);
                        string transactionId = Util.ReadString(br);
                        string receipt = Util.ReadString(br);

                        _DicEntries.Add(billingType, new CReceiptInfo() {
                            BillingType = billingType,
                            RoleId = roldId,
                            IsSucceed = isSucceed,
                            OrderId = orderId,
                            ProductId = productId,
                            TransactionId = transactionId,
                            Receipt = receipt });
                    }
                }
            }
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public bool WriteFile(string filename)
    {
        try
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
            {
                {
                    BinaryWriter bw = new BinaryWriter(fs);

                    bw.Write(_DicEntries.Count);            //总个数
                    foreach (var kv in _DicEntries)
                    {
                        CReceiptInfo entry = kv.Value;

                        bw.Write(entry.BillingType);
                        bw.Write(entry.RoleId);
                        bw.Write(entry.IsSucceed);
                        Util.WriteString(bw, entry.OrderId);
                        Util.WriteString(bw, entry.ProductId);
                        Util.WriteString(bw, entry.TransactionId);
                        Util.WriteString(bw, entry.Receipt);
                    }

                }
            }
        }
        catch (IOException)
        {
            return false;
        }
        return true;
    }
}

