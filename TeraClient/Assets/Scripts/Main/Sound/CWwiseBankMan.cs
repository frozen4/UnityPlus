using System;
using System.Collections.Generic;
using System.Text;
using Common;
using System.Runtime.InteropServices;
using UnityEngine;

#if false

public class CWwiseBankMan : Singleton<CWwiseBankMan>
{
    private const long AK_BANK_PLATFORM_DATA_ALIGNMENT = (long)AkSoundEngine.AK_BANK_PLATFORM_DATA_ALIGNMENT;
    private const long AK_BANK_PLATFORM_DATA_ALIGNMENT_MASK = AK_BANK_PLATFORM_DATA_ALIGNMENT - 1;
    private const int MAX_BNK_CACHED = 1;

    class CBankEntry
    {
        public uint InMemoryBankSize = 0;
        public GCHandle PinnedArray;
        public IntPtr InMemoryBankPtr = IntPtr.Zero;
        public uint BankID = AkSoundEngine.AK_INVALID_BANK_ID;
        public GameObject GameObject;

        public bool IsValid()
        {
            return BankID != AkSoundEngine.AK_INVALID_BANK_ID;
        }
    }

    private readonly Dictionary<string, CBankEntry> _DicBankLoad = new Dictionary<string, CBankEntry>();
    private readonly List<string> _ListBankCached = new List<string>();

    public bool LoadBank(string inBankFileName, bool localized = false)
    {
        if (_DicBankLoad.ContainsKey(inBankFileName.ToLower()))
            return true;

        /*
        string bankPath;
        
        if (!localized)
            bankPath = HobaText.Format("{0}/{1}/{2}/{3}", EntryPoint.Instance.ResPath, AkInitializer.GetBasePath(), AkBasePathGetter.GetPlatformName(), in_bankFileName);
        else
            bankPath = HobaText.Format("{0}/{1}/{2}/{3}/{4}", EntryPoint.Instance.ResPath, AkInitializer.GetBasePath(), AkBasePathGetter.GetPlatformName(), AkInitializer.GetCurrentLanguage(), in_bankFileName);

        SBankEntry entry = new SBankEntry();
        if (DoLoadBankFromImage(bankPath, entry))
        {
            string name = localized ? HobaText.Format("{0}/{1}", AkInitializer.GetCurrentLanguage(), in_bankFileName.ToLower()) : in_bankFileName.ToLower();
            entry.gameObject = new GameObject(name);
            entry.gameObject.transform.parent = WwiseSoundMan.Instance.BanksLoaded.transform;
            _DicBankLoad.Add(in_bankFileName.ToLower(), entry);
            return true;
        }

        HobaDebuger.LogWarningFormat("LoadBank Failed: {0}", bankPath);
        return false;
        */

        
        CBankEntry entry = new CBankEntry();
        string name = localized ? HobaText.Format("{0}/{1}", AkInitializer.GetCurrentLanguage(), inBankFileName) : inBankFileName;
        AKRESULT ret = AkSoundEngine.LoadBank(name, AkSoundEngine.AK_DEFAULT_POOL_ID, out entry.BankID);
        if (ret == AKRESULT.AK_Success)
        {
            entry.GameObject = new GameObject(name);
            entry.GameObject.transform.parent = WwiseSoundMan.Instance.BanksLoaded.transform;
            _DicBankLoad.Add(inBankFileName.ToLower(), entry);
            return true;
        }

        HobaDebuger.LogWarningFormat("LoadBank Failed: {0}", name);
        return false;
    }

    public void UnloadBank(string inBankFilename)
    {
        CBankEntry entry;
        if (!_DicBankLoad.TryGetValue(inBankFilename.ToLower(), out entry))
            return;

        if (entry.InMemoryBankPtr != IntPtr.Zero)
        {
            AKRESULT result = AkSoundEngine.UnloadBank(entry.BankID, entry.InMemoryBankPtr);
            if (result == AKRESULT.AK_Success)
            {
                entry.PinnedArray.Free();
            }
            else
            {
                HobaDebuger.LogWarningFormat("UnloadBank Failed: {0} {1}", inBankFilename, result);
            }
            entry.InMemoryBankPtr = IntPtr.Zero;
        }
        else
        {
            AkSoundEngine.UnloadBank(entry.BankID, IntPtr.Zero, null, null);
        }

        if (entry.GameObject != null)
            GameObject.Destroy(entry.GameObject);
        _DicBankLoad.Remove(inBankFilename.ToLower());
    }

    private bool DoLoadBankFromImage(string in_bankPath, CBankEntry bankEntry)
    {
        uint in_uInMemoryBankSize = 0;
        GCHandle ms_pinnedArray = new GCHandle();
        IntPtr ms_pInMemoryBankPtr = IntPtr.Zero;
        uint ms_bankID = AkSoundEngine.AK_INVALID_BANK_ID;

        byte[] bytes = Util.ReadFile(in_bankPath);
        if (bytes == null)
        {
            Common.HobaDebuger.LogErrorFormat("WwiseUnity: AkMemBankLoader: bank loading failed: {0}", in_bankPath);
            return false;
        }

        try
        {
            ms_pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            ms_pInMemoryBankPtr = ms_pinnedArray.AddrOfPinnedObject();
            in_uInMemoryBankSize = (uint)bytes.Length;

            // Array inside the WWW object is not aligned. Allocate a new array for which we can guarantee the alignment.
            if ((ms_pInMemoryBankPtr.ToInt64() & AK_BANK_PLATFORM_DATA_ALIGNMENT_MASK) != 0)
            {
                byte[] alignedBytes = new byte[bytes.Length + AK_BANK_PLATFORM_DATA_ALIGNMENT];
                GCHandle new_pinnedArray = GCHandle.Alloc(alignedBytes, GCHandleType.Pinned);
                IntPtr new_pInMemoryBankPtr = new_pinnedArray.AddrOfPinnedObject();
                int alignedOffset = 0;

                // New array is not aligned, so we will need to use an offset inside it to align our data.
                if ((new_pInMemoryBankPtr.ToInt64() & AK_BANK_PLATFORM_DATA_ALIGNMENT_MASK) != 0)
                {
                    Int64 alignedPtr = (new_pInMemoryBankPtr.ToInt64() + AK_BANK_PLATFORM_DATA_ALIGNMENT_MASK) & ~AK_BANK_PLATFORM_DATA_ALIGNMENT_MASK;
                    alignedOffset = (int)(alignedPtr - new_pInMemoryBankPtr.ToInt64());
                    new_pInMemoryBankPtr = new IntPtr(alignedPtr);
                }

                // Copy the bank's bytes in our new array, at the correct aligned offset.
                Array.Copy(bytes, 0, alignedBytes, alignedOffset, bytes.Length);

                ms_pInMemoryBankPtr = new_pInMemoryBankPtr;
                ms_pinnedArray.Free();
                ms_pinnedArray = new_pinnedArray;
            }
        }
        catch(Exception)
        {
            if (ms_pInMemoryBankPtr != IntPtr.Zero)
                ms_pinnedArray.Free();

            return false;
        }

        bankEntry.InMemoryBankSize = in_uInMemoryBankSize;
        bankEntry.PinnedArray = ms_pinnedArray;
        bankEntry.InMemoryBankPtr = ms_pInMemoryBankPtr;
        
        AKRESULT result = AkSoundEngine.LoadBank(ms_pInMemoryBankPtr, in_uInMemoryBankSize, out ms_bankID);
        if (result != AKRESULT.AK_Success)
        {
            HobaDebuger.LogErrorFormat("DoLoadBankFromImage failed with result: {0} {1}", result, in_bankPath);
            return false;
        }
        
        bankEntry.BankID = ms_bankID;
        return true;
    }

    public void CacheUnload(string in_bankFileName)
    {
        string bankName = in_bankFileName.ToLower();
        CBankEntry entry;
        if (!_DicBankLoad.TryGetValue(bankName, out entry))
            return;

        if (!_ListBankCached.Contains(bankName))
            _ListBankCached.Add(bankName);

        while (_ListBankCached.Count > MAX_BNK_CACHED)
        {
            UnloadBank(_ListBankCached[0]);
            _ListBankCached.RemoveAt(0);
        }  
    }

    public void ClearAllBanks()
    {
        foreach (var kv in _DicBankLoad)
        {
            string bankName = kv.Key;
            CBankEntry entry = kv.Value;
            if (entry.InMemoryBankPtr != IntPtr.Zero)
            {
                AKRESULT result = AkSoundEngine.UnloadBank(entry.BankID, entry.InMemoryBankPtr);
                if (result == AKRESULT.AK_Success)
                {
                    entry.PinnedArray.Free();
                }
                else
                {
                    HobaDebuger.LogWarningFormat("UnloadBank Failed: {0} {1}", bankName, result);
                }
                entry.InMemoryBankPtr = IntPtr.Zero;
            }
            else
            {
                AkSoundEngine.UnloadBank(entry.BankID, IntPtr.Zero, null, null);
            }

             GameObject.Destroy(entry.GameObject);
        }
        _DicBankLoad.Clear();
    }

}

#endif