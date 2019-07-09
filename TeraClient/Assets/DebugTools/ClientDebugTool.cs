using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class ClientDebugTool : MonoBehaviour
{

    [MenuItem("Hoba Tools/Code Hotfix %`")]
    static void ClientCodeHotFix()
    {
#if UNITY_EDITOR
        LuaScriptMgr.Instance.CallLuaFunction("HotFix.HotFixCode");
#endif
    }

    [MenuItem("Hoba Tools/游戏内断线重连  %1")]
    static void ClientDisConnect()
    {
#if UNITY_EDITOR || UNITY_STANDALONE

        if (CGameSession.Instance().IsConnected())
        {
            LuaScriptMgr.Instance.CallLuaOnConnectionEventFunc((int)Common.Net.EVENT.DISCONNECTED);
            CGameSession.Instance().Close();
        }

#endif
    }
}
#endif
