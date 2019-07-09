using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Net;
using LuaInterface;
using Hoba.ObjectPool;
using System.Net;
using UnityEngine;

public class CGameSession
{
    private static CGameSession _Instance;
    public static CGameSession Instance()
    {
        if (_Instance == null)
            _Instance = new CGameSession();
        return _Instance;
    }

    private NetClient _NetClientNode = null;
    private static readonly System.Object _CsSession = new System.Object();
    private volatile EVENT _GatewayEvent = EVENT.NONE;


    enum ConnectState
    {
        Connecting = 0,
        Connected = 1,
        Disconnected = 2,
    }

    private ConnectState _ConnectState = ConnectState.Disconnected;

    private readonly List<CS2CPrtcData> _UnprocessedPrtcList = new List<CS2CPrtcData>();
    private readonly List<CS2CPrtcData> _NewPrtcList = new List<CS2CPrtcData>();  //New received protocols
    private static readonly ObjectPool<CS2CPrtcData> _PrtcDataPool = new ObjectPool<CS2CPrtcData>(10, 100, () => { return new CS2CPrtcData(); });
    private float _PingInterval = 0;
    private long _LastSendPingTime = 0;        //发送过C2SPing协议
    private long _LastRecvAliveTime = 0;        //接收过任何协议
    private Action _Reconnect = null;
    private long _ProcessBeginTicks = 0;

    [NoToLua]
    public float PingInterval = 10;
    [NoToLua]
    public int MaxProcessProtocol = 60;     //每帧最大处理协议数
    [NoToLua]
    public int MaxProcessSpecialProtocol = 30;  //每帧协议超过多少时触发特殊处理

    public bool IsProcessingPaused = false;
    public bool IsSendingPaused = false;

    [NoToLua]
    public string IP { get; set; }
    [NoToLua]
    public int Port { get; set; }
    [NoToLua]
    public string UserName { get; set; }
    [NoToLua]
    public string Password { get; set; }
    [NoToLua]
    public bool IsLocalServerMode { get; set; }

    public struct C2SPrtcData
    {
        public int PrtcId;
        public byte[] PrtcData;

        public C2SPrtcData(int id, byte[] data)
        {
            PrtcId = id;
            PrtcData = data;
        }
    }
    private readonly List<C2SPrtcData> _ToSendPrtcList = new List<C2SPrtcData>();


    // for 网络延迟测试
    [NoToLua]
    public int LatencyMillisecond = 0;

    public bool IsValidIpAddress(string ipAddr)
    {
        IPAddress addr;
        return IPAddress.TryParse(ipAddr, out addr);
    }

    public void ConnectToServer(string addr, int port, string username, string password)
    {
        IsLocalServerMode = false;

        IP = addr;
        Port = port;
        UserName = username;
        Password = password;

        Connect();
    }

    public bool IsConnecting()
    {
        if (_NetClientNode == null) return false;
        return _ConnectState == ConnectState.Connecting;
    }

    public bool IsConnected()
    {
        if (_NetClientNode == null) return false;
        return _ConnectState == ConnectState.Connected;
    }

    [NoToLua]
    public void SendProtocol(int prtcId, byte[] data)
    {
        if (_ConnectState != ConnectState.Connected) return;

        if (IsLocalServerMode)
        {
            // TODO: 离线模式
        }
        else
        {
            if (IsSendingPaused)
            {
                _ToSendPrtcList.Add(new C2SPrtcData(prtcId, data));
            }
            else
            {
                byte[] result = MakeByteArrayWithLength(data, prtcId);
                _NetClientNode.SendSync(result);
                _PingInterval = 0.0f;           //发过协议的，时间间隔内可以不发ping
            }
        }
    }

    [NoToLua]
    public void Tick(float dt)
    {
        if (_NetClientNode == null)
            return;

        // connection check ...
        CheckConnection(10);

        // keep alive ...
        KeepAlive(PingInterval, dt);

        _NetClientNode.DoNetMessage();

        CheckGatewayEvent();

        if (!IsSendingPaused && _ToSendPrtcList.Count > 0)
        {
            foreach (var v in _ToSendPrtcList)
                SendProtocol(v.PrtcId, v.PrtcData);
            _ToSendPrtcList.Clear();
        }

        if (IsProcessingPaused) return;

        bool canProcess = true;
        int processedCount = 0;
        int count = FetchProtocols();
        //var beginTime = Time.time;

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (!canProcess || processedCount >= MaxProcessProtocol)
                    break;

                var p = PopupProtocol();
                ProcessProtocol(p, count > MaxProcessSpecialProtocol);
                ++processedCount;

                if (IsProcessingPaused)
                    canProcess = false;

                //if(Time.time - beginTime > 0.015f)
                //    canProcess = false;
            }
        }
    }

    public void Close()
    {
        if (_NetClientNode != null)
        {
            _NetClientNode.Release();
            _NetClientNode = null;
            _ConnectState = ConnectState.Disconnected;

            UnityEngine.Debug.LogWarning("GameSession Close");
        }

        ClearProtocols();

#if UNITY_EDITOR
        _PrtcDataPool.ShowDiagnostics("PrtcDataPool");
#endif
    }


    public void TestBytes(CS2CPrtcData p, bool isSpecial, bool isSimple)
    {
        if (p == null || p.Type != 10531) return;

        int count = 10000;
        long totalTicks = 0;
        if (p != null && p.Type > 0)
        {
            //UnityEngine.Profiling.Profiler.BeginSample("Start PB ");
            var time = DateTime.Now.Ticks;
            totalTicks = 0;

            for (int i = 0; i < count; i++)
            {
                time = DateTime.Now.Ticks;
                IntPtr L = LuaScriptMgr.Instance.GetL();
                if (L != IntPtr.Zero)
                {
                    int oldTop = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_getglobal(L, "ProcessProtocol");
                    if (!LuaDLL.lua_isnil(L, -1))
                    {
                        LuaDLL.lua_pushinteger(L, p.Type);
                        if (p.Buffer != null)
                            LuaDLL.lua_pushlstring(L, p.Buffer, p.Buffer.Length);
                        else
                            LuaDLL.lua_pushstring(L, String.Empty);
                        LuaDLL.lua_pushboolean(L, isSpecial);
                        LuaDLL.lua_pushboolean(L, isSimple);

                        if (LuaDLL.lua_pcall(L, 4, 0, 0) != 0)
                        {
                            HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                        }
                    }

                    LuaDLL.lua_settop(L, oldTop);
                }
                totalTicks += DateTime.Now.Ticks - time;

            }
            UnityEngine.Debug.LogError("PB总耗费时间为" + (totalTicks).ToString());
          
            using (FileStream sr = new FileStream(UnityEngine.Application.dataPath + "/tmpBuffer",
                FileMode.OpenOrCreate))
            {
                BinaryWriter bw = new BinaryWriter(sr);
                //type
                bw.Write(1);
                //entityid
                bw.Write(2);
                //CurrentPosition
                bw.Write(1.0f);
                bw.Write(2.0f);
                bw.Write(3.0f);
                //CurrentOrientation
                bw.Write(4.0f);
                bw.Write(5.0f);
                bw.Write(6.0f);
                //MoveType
                bw.Write(3);
                //MoveDirection
                bw.Write(7.0f);
                bw.Write(8.0f);
                bw.Write(9.0f);
                //MoveSpeed
                bw.Write(10.0f);
                //TimeInterval
                bw.Write(4);
                //DstPosition
                bw.Write(11.0f);
                bw.Write(12.0f);
                bw.Write(33.0f);
                //IsDestPosition
                bw.Write(false);
                bw.Flush();
            }

            //UnityEngine.Profiling.Profiler.BeginSample("Start BF ");
            totalTicks = 0;
            for (int i = 0; i < count; i++)
            {
                FileStream sr2 = new FileStream(UnityEngine.Application.dataPath + "/tmpBuffer", FileMode.Open);
                BinaryReader br = new BinaryReader(sr2);

                time = DateTime.Now.Ticks;

                var type = br.ReadInt32();
                var EntityId = br.ReadInt32();
                var vecX = br.ReadSingle();
                var vecY = br.ReadSingle();
                var vecZ = br.ReadSingle();
                var CurrentPosition = new Vector3(vecX, vecY, vecZ);
                vecX = br.ReadSingle();
                vecY = br.ReadSingle();
                vecZ = br.ReadSingle();
                var CurrentOrientation = new Vector3(vecX, vecY, vecZ);
                var MoveType = br.ReadInt32();
                vecX = br.ReadSingle();
                vecY = br.ReadSingle();
                vecZ = br.ReadSingle();
                var MoveDirection = new Vector3(vecX, vecY, vecZ);
                var MoveSpeed = br.ReadSingle();
                var TimeInterval = br.ReadInt32();
                vecX = br.ReadSingle();
                vecY = br.ReadSingle();
                vecZ = br.ReadSingle();
                var DstPosition = new Vector3(vecX, vecY, vecZ);
                var IsDestPosition = br.ReadBoolean();
                LuaScriptMgr.Instance.CallLuaOnWeatherEventFunc2(type, EntityId, CurrentPosition, CurrentOrientation
                    , MoveType, MoveDirection, MoveSpeed, TimeInterval, DstPosition, IsDestPosition);
                totalTicks += DateTime.Now.Ticks - time;
                br.Close();
            }
            UnityEngine.Debug.LogError("二进制总耗费时间为" + (totalTicks).ToString());
            //UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    private void BytesDecodeTest(CS2CPrtcData p)
    {
        if(p == null || p.Type != 10531) return;

        const int count = 10000;
        Single totalTime = 0;

        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            var startTime = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
              
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "ProcessMoveProtocol2");
                if (!LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_pushinteger(L, p.Type);
                    LuaDLL.lua_pushlstring(L, p.Buffer, p.Buffer.Length);
                    LuaDLL.lua_pushboolean(L, false);
                    LuaDLL.lua_pushboolean(L, false);

                    if (LuaDLL.lua_pcall(L, 4, 0, 0) != 0)
                    {
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                    }
                }
                LuaDLL.lua_settop(L, oldTop);

            }
            totalTime = (DateTime.Now - startTime).Ticks;
        }

        UnityEngine.Debug.LogErrorFormat("ProcessMovePBProtocol * {0} = {1} ticks", count, totalTime);

        byte[] inputBytes = new byte[100];
        System.IO.MemoryStream ms = new System.IO.MemoryStream(inputBytes);
        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);

        {
            //entityid
            bw.Write(2);
            //CurrentPosition
            bw.Write(1.0f);
            bw.Write(2.0f);
            bw.Write(3.0f);
            //CurrentOrientation
            bw.Write(4.0f);
            bw.Write(5.0f);
            bw.Write(6.0f);
            //MoveType
            bw.Write(3);
            //MoveDirection
            bw.Write(7.0f);
            bw.Write(8.0f);
            bw.Write(9.0f);
            //MoveSpeed
            bw.Write(10.0f);
            //TimeInterval
            bw.Write(4);
            //DstPosition
            bw.Write(11.0f);
            bw.Write(12.0f);
            bw.Write(33.0f);
            //IsDestPosition
            bw.Write(false);


            ms.Seek(0, System.IO.SeekOrigin.Begin);
        }


        System.IO.BinaryReader br = new System.IO.BinaryReader(ms);

        {
            var startTime = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                br.BaseStream.Position = 0;
                var EntityId = br.ReadInt32();
                var vecX = br.ReadSingle();
                var vecY = br.ReadSingle();
                var vecZ = br.ReadSingle();
                var CurrentPosition = new UnityEngine.Vector3(vecX, vecY, vecZ);
                vecX = br.ReadSingle();
                vecY = br.ReadSingle();
                vecZ = br.ReadSingle();
                var CurrentOrientation = new UnityEngine.Vector3(vecX, vecY, vecZ);
                var MoveType = br.ReadInt32();
                vecX = br.ReadSingle();
                vecY = br.ReadSingle();
                vecZ = br.ReadSingle();
                var MoveDirection = new UnityEngine.Vector3(vecX, vecY, vecZ);
                var MoveSpeed = br.ReadSingle();
                var TimeInterval = br.ReadInt32();
                vecX = br.ReadSingle();
                vecY = br.ReadSingle();
                vecZ = br.ReadSingle();
                var DstPosition = new UnityEngine.Vector3(vecX, vecY, vecZ);
                var IsDestPosition = br.ReadBoolean();

                if (L != IntPtr.Zero)
                {
                    int oldTop = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_getglobal(L, "ProcessMoveProtocol1");
                    if (!LuaDLL.lua_isnil(L, -1))
                    {
                        LuaScriptMgr.Push(L, EntityId);
                        LuaScriptMgr.Push(L, CurrentPosition);
                        LuaScriptMgr.Push(L, CurrentOrientation);
                        LuaScriptMgr.Push(L, MoveType);
                        LuaScriptMgr.Push(L, MoveDirection);
                        LuaScriptMgr.Push(L, MoveSpeed);
                        LuaScriptMgr.Push(L, TimeInterval);
                        LuaScriptMgr.Push(L, DstPosition);
                        LuaScriptMgr.Push(L, IsDestPosition);

                        if (LuaDLL.lua_pcall(L, 9, 0, 0) != 0)
                        {
                            HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                        }
                    }
                    LuaDLL.lua_settop(L, oldTop);
                }
            }

            totalTime = (DateTime.Now - startTime).Ticks;
        }
        bw.Close();
        br.Close();
        ms.Close();
        ms.Dispose();

        UnityEngine.Debug.LogErrorFormat("ProcessMoveProtocol * {0} = {1} ticks", count, totalTime);
    }

    private void ProcessProtocol(CS2CPrtcData p,bool isSimple)
    {
        if (p != null && p.Type > 0)
        {
#if false
            // test 1
            BytesDecodeTest(p);
            // test 2
            TestBytes(p, false, false);
#endif

            var isSpecial = _ProcessBeginTicks > 0 && p.TimeStamp < _ProcessBeginTicks;

            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "ProcessProtocol");
                if (!LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_pushinteger(L, p.Type);
                    if (p.Buffer != null)
                        LuaDLL.lua_pushlstring(L, p.Buffer, p.Buffer.Length);
                    else
                        LuaDLL.lua_pushstring(L, String.Empty);
                    LuaDLL.lua_pushboolean(L, isSpecial);
                    LuaDLL.lua_pushboolean(L, isSimple);

                    if (LuaDLL.lua_pcall(L, 4, 0, 0) != 0)
                    {
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                    }
                }
                LuaDLL.lua_settop(L, oldTop);
            }
        }
    }

    private int FetchProtocols()
    {
        lock (_CsSession)
        {
            if (LatencyMillisecond <= 0)
            {
                _UnprocessedPrtcList.AddRange(_NewPrtcList);
                _NewPrtcList.Clear();
            }
            else
            {
                List<CS2CPrtcData>.Enumerator e = _NewPrtcList.GetEnumerator();
                var curTicks = DateTime.Now.Ticks;
                while (e.MoveNext())
                {
                    if (e.Current != null && (curTicks - e.Current.TimeStamp) > LatencyMillisecond * 10000)
                    {
                        _UnprocessedPrtcList.Add(e.Current);
                    }
                }
                e.Dispose();

                List<CS2CPrtcData>.Enumerator e1 = _UnprocessedPrtcList.GetEnumerator();
                while (e1.MoveNext())
                {
                    _NewPrtcList.Remove(e1.Current);
                }
                e1.Dispose();
            }
        }
        return _UnprocessedPrtcList.Count;
    }

    private CS2CPrtcData PopupProtocol()
    {
        if (_UnprocessedPrtcList.Count > 0)
        {
            var prtc = _UnprocessedPrtcList[0];
            _UnprocessedPrtcList.RemoveAt(0);
            return prtc;

        }
        else
            return null;
    }

    private void Connect()
    {
        if (!IsLocalServerMode)
        {
            if (_NetClientNode != null)
            {
                // 正常连接中，不做处理
                if(_ConnectState == ConnectState.Connecting || _ConnectState == ConnectState.Connected)
                    return;
            }

            // 建立新的链接
            _NetClientNode = new NetClient();
            _NetClientNode.HandleReceivedMessage += OnReceiveSocketPrtc;
            _NetClientNode.OnGatewayEvent = OnEvent;
            _NetClientNode.Start(IP, Port);
        }
    }

    private void ClearProtocols()
    {
        LatencyMillisecond = 0;

        // 清理之前，一些关键协议需要处理，不可忽略
        var count = FetchProtocols();
        for (int i = 0; i < count; i++)
        {
            var p = PopupProtocol();
            if (p != null)
            {
                IntPtr L = LuaScriptMgr.Instance.GetL();
                if (L != IntPtr.Zero)
                {
                    int oldTop = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_getglobal(L, "ClearProtocol");
                    if (!LuaDLL.lua_isnil(L, -1))
                    {
                        LuaDLL.lua_pushinteger(L, p.Type);
                        if (p.Buffer != null)
                            LuaDLL.lua_pushlstring(L, p.Buffer, p.Buffer.Length);
                        else
                            LuaDLL.lua_pushstring(L, String.Empty);
                        LuaDLL.lua_pushboolean(L, false);
                        LuaDLL.lua_pushboolean(L, false);

                        if (LuaDLL.lua_pcall(L, 4, 0, 0) != 0)
                        {
                            HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                        }
                    }
                    LuaDLL.lua_settop(L, oldTop);
                }
            }
        }

        _UnprocessedPrtcList.Clear();
        lock (_CsSession)
        {
            _NewPrtcList.Clear();
        }

        IsSendingPaused = false;
        _ToSendPrtcList.Clear();
    }

    private void OnEvent(EVENT eventId)
    {
        UnityEngine.Debug.LogWarningFormat("GameSession OnEvent {0}", eventId);

        switch (eventId)
        {
            case EVENT.CONNECTED:
                {
                    _ConnectState = ConnectState.Connected;
                    _GatewayEvent = EVENT.CONNECTED;
                }
                break;

            case EVENT.CONNECT_FAILED:
                {
                    _ConnectState = ConnectState.Disconnected;
                    _GatewayEvent = EVENT.CONNECT_FAILED;
                }
                break;

            case EVENT.CLOSED: 
            case EVENT.DISCONNECTED:
                {
                    _ConnectState = ConnectState.Disconnected;
                    _GatewayEvent = EVENT.DISCONNECTED;
                }
                break;
        }

        if (_ConnectState == ConnectState.Disconnected)
        {
            ClearProtocols();

            if (_Reconnect != null)
            {
                _Reconnect();
                _Reconnect = null;
            }   
        }
    }

    private void CheckGatewayEvent()
    {
        if (_GatewayEvent == EVENT.NONE) return;

        LuaScriptMgr.Instance.CallLuaOnConnectionEventFunc((int)_GatewayEvent);
        _GatewayEvent = EVENT.NONE;
    }

    // 真实服务器模式下 接收协议处理
    private static void OnReceiveSocketPrtc(ExSocket exSocket, byte[] data)
    {
        if (data == null || data.Length < sizeof (int))
        {
            HobaDebuger.LogWarning("Protocol Data Length is too short");
            return;
        }

        int id = System.BitConverter.ToInt32(data, 0);
        byte[] pbs = null;
        if(data.Length > sizeof(int))
        {
            int len = data.Length - sizeof(int);
            pbs = new byte[len];
            System.Buffer.BlockCopy(data, 4, pbs, 0, len);
        }

        CS2CPrtcData prtc = _PrtcDataPool.GetObject(); 
        prtc.Set(id, pbs);
        Instance().AddNewProtocol(prtc);
    }

    private void CheckConnection(int seconds)
    {
        if (_NetClientNode == null)
            return;

        if (_ConnectState != ConnectState.Connected || IsProcessingPaused)
        {
            _LastRecvAliveTime = 0;
            return;
        }

        if (_LastRecvAliveTime != 0)
        {
            long nowTicks = DateTime.Now.Ticks;
            if (nowTicks - _LastRecvAliveTime > 5 * 10000000)           //如果5秒钟内没收到协议，发送ping
            {
                if (_LastSendPingTime < _LastRecvAliveTime)        //如果从上次接收协议开始， 5秒内没发过ping，发送
                {
                    DoSendPing();

                    _LastSendPingTime = nowTicks;
                    _PingInterval = 0.0f;

                    _LastRecvAliveTime = nowTicks;              //这时接收时间重新计算
                }
            }
            if (nowTicks - _LastRecvAliveTime > seconds * 10000000)        
            {
                if (_LastSendPingTime >= _LastRecvAliveTime)            //如果已经发送过ping
                {
                    Common.HobaDebuger.LogWarningFormat("Disconnect By CheckConnection! interval: {0}", seconds);

                    _LastRecvAliveTime = 0;

                    _ConnectState = ConnectState.Disconnected;
                    _GatewayEvent = EVENT.DISCONNECTED;

                    ClearProtocols();
                }
            }
        }
    }

    private void KeepAlive(float pingInterval, float dt)
    {
        if(_ConnectState == ConnectState.Connected)
        {
            _PingInterval += dt;

            if (_PingInterval > pingInterval)
            {
                DoSendPing();

                _LastSendPingTime = DateTime.Now.Ticks;
                _PingInterval = 0.0f;
            }
        }
    }

    private void DoSendPing()
    {
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "SendProtocol_Ping");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                var ms = (DateTime.UtcNow - Main.DateTimeBegin).TotalMilliseconds /*+ EntryPoint._ServerTimeGap*/;
                LuaScriptMgr.Push(L, ms);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    private int AddNewProtocol(CS2CPrtcData prct)
    {
        //if (prct.Type == S2CTypePing)
        {
            // 记录上次ping时间
            _LastRecvAliveTime = DateTime.Now.Ticks;
        }

        int iSize = 0;
        lock (_CsSession)
        {
            _NewPrtcList.Add(prct);
            iSize = _NewPrtcList.Count;
        }

        return iSize;
    }

    private static byte[] MakeByteArrayWithLength(byte[] buffer, int id)
    {
        int bufferLength = buffer.Length;
        int nHead = sizeof(int) + bufferLength;      //head 4bytes
        int nLength = sizeof(int) + sizeof(int) + bufferLength;
        byte[] result = new byte[nLength];
        //byte[] headBytes = BitConverter.GetBytes(nHead);
        //byte[] intBytes = BitConverter.GetBytes(id);

        int nOffset = 0;

        //Buffer.BlockCopy(headBytes, 0, result, nOffset, headBytes.Length);
        IntToBytes(nHead, result, nOffset);
        nOffset += 4;

        //Buffer.BlockCopy(intBytes, 0, result, nOffset, intBytes.Length);
        IntToBytes(id, result, nOffset);
        nOffset += 4;

        Buffer.BlockCopy(buffer, 0, result, nOffset, bufferLength);

        return result;
    }

    public static void IntToBytes(int value, byte[] bytes, int offset)
    {
        bytes[offset + 3] = (byte)((value >> 24) & 0xFF);
        bytes[offset + 2] = (byte)((value >> 16) & 0xFF);
        bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
        bytes[offset + 0] = (byte)(value & 0xFF);
    }  
}