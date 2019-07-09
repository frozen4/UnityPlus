public class CLogReport
{
    public static void Init()
    {
#if USING_NELO2
        // use Nelo2
        // TODO:
#elif USING_BUGLY
        //use Bugly
        BuglyManager.Initialize();
#elif USING_FABRIC
        //use Fabric
        FabricManager.Initialize();
#else
        // ToDo: 考虑一下Win下如何实现

#endif
    }

    public static void ReportUserId(string userId)
    {
#if USING_FABRIC
        FabricManager.SetUserId(userId);
#else

#endif
    }

    public static void ReportRoleInfo(string info)
    {
#if USING_FABRIC
        FabricManager.SetRoleInfo(info);
#else

#endif
    }

    public static void Reset()
    {
#if USING_FABRIC
        FabricManager.Reset();
#else

#endif
    }
}
