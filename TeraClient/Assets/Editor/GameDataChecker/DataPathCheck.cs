using Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Template;

namespace GameDataChecker
{
    public class DataPathCheck : Singleton<DataPathCheck>
    {
        protected const string AssetBundlesFolderName = "/AssetBundles/";

        // 基础路径设置
        private static string _GameResBasePath = "";
        private static string _BaseAssetBundleURL = "";
        private static string _UpdateAssetBundleURL = "";

        private static HashSet<string> _AssetSet = new HashSet<string>();

        private static SortedDictionary<string, List<PathCheckInfo>> _DicAssetPathInfo = new SortedDictionary<string, List<PathCheckInfo>>();
        private static SortedDictionary<string, List<PathCheckInfo>> _DicIconPathInfo = new SortedDictionary<string, List<PathCheckInfo>>();

        public const string Icon_Base_Path_0 = "Assets/Outputs/";
        public const string Icon_Base_Path_1 = "Assets/Outputs/CommonAtlas/";
        public const string Icon_Base_Path_2 = "Assets/Outputs/CommonAtlas/Icon/";

        public class PathCheckInfo
        {
            public int TID = 0;
            public string FieldName = string.Empty;
            public string ErrorPath = string.Empty;
        }

        #region utility

        private void SortPathCheckInfo(SortedDictionary<string, List<PathCheckInfo>> dic)
        {
            foreach (var kv in dic)
            {
                List<PathCheckInfo> list = kv.Value;
                list.Sort(
                delegate (PathCheckInfo x, PathCheckInfo y)
                {
                    int eq = x.TID.CompareTo(y.TID); 

                    if (eq == 0)
                        eq = x.FieldName.CompareTo(y.FieldName);

                    if (eq == 0)
                        eq = x.ErrorPath.CompareTo(y.ErrorPath);

                    return eq;
                });
            }
        }

        private void AddAssetPathCheckInfo(string templateName, int tid, string fieldName, string errorPath)
        {
            if (!_DicAssetPathInfo.ContainsKey(templateName))
                _DicAssetPathInfo.Add(templateName, new List<PathCheckInfo>());

            List<PathCheckInfo> list;
            if (!_DicAssetPathInfo.TryGetValue(templateName, out list))
            {
                list = new List<PathCheckInfo>();
                _DicAssetPathInfo.Add(templateName, list);
            }

            list.Add(new PathCheckInfo()
            {
                FieldName = fieldName,
                TID = tid,
                ErrorPath = errorPath,
            });
        }

        private void AddIconPathCheckInfo(string templateName, int tid, string fieldName, string errorPath)
        {
            if (!_DicIconPathInfo.ContainsKey(templateName))
                _DicIconPathInfo.Add(templateName, new List<PathCheckInfo>());

            List<PathCheckInfo> list;
            if (!_DicIconPathInfo.TryGetValue(templateName, out list))
            {
                list = new List<PathCheckInfo>();
                _DicIconPathInfo.Add(templateName, list);
            }

            list.Add(new PathCheckInfo() 
            {
                FieldName = fieldName,
                TID = tid,
                ErrorPath = errorPath,
            });
        }

        private static string GetPlatformFolderForAssetBundles()
        {
#if UNITY_EDITOR
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                default:
                    return "";
            }
#else
            switch(Application.platform)
		    {
		        case RuntimePlatform.Android:
			        return "Android";
		        case RuntimePlatform.IPhonePlayer:
			        return "iOS";
		        case RuntimePlatform.WindowsPlayer:
			        return "Windows";
		        default:
			        return "";
		    }
#endif
        }

        public static bool IsUpdateFileExist(string filename)
        {
            var fullPath = HobaText.Format("{0}/{1}", _UpdateAssetBundleURL, filename);
            return File.Exists(fullPath);
        }

        private static void LoadPathIDData()
        {
            byte[] pathidContent = null;
            if (IsUpdateFileExist("PATHID.dat"))         //使用更新目录下的pathid.dat
            {
                var filePath = HobaText.Format("{0}{1}", _UpdateAssetBundleURL, "PATHID.dat");
                pathidContent = Util.ReadFile(filePath);
            }
            else
            {
                var filePath = HobaText.Format("{0}{1}", _GameResBasePath, "PATHID.dat");
                pathidContent = Util.ReadFile(filePath);
            }

            if (pathidContent != null && pathidContent.Length > 0)
            {
                StreamReader sr = new StreamReader(new MemoryStream(pathidContent));

                var strLine = sr.ReadLine();
                while (strLine != null)
                {
                    string[] asset_temp = strLine.Split(',');
                    if (asset_temp.Length >= 2)
                    {
                        _AssetSet.Add(asset_temp[1]);
                    }

                    strLine = sr.ReadLine();
                };

                sr.Close();
            }
        }

        private static bool IsValidPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "0")
                return true;

            if (path.EndsWith("mp4") || path.EndsWith("xml"))
                return true;

            return _AssetSet.Contains(path);
        }

        private static bool IsValidIconPath(string basePath, string path, string ext = ".png")
        {
            if (string.IsNullOrEmpty(path) || path == "0")
                return true;

            string fullpath = basePath + path + ext;
            bool bContain = _AssetSet.Contains(fullpath);
            if (!bContain)
            {
                int xxx = 0;
            }
            return bContain;
        }

        #endregion

        [MenuItem("Tools/游戏数据检查/模板路径有效性检查", false, 2)]
        public static void Check()
        {
            EditorUtility.DisplayProgressBar("提示", "数据初始化", 0.3f);
            Instance.Init();
            Instance.InitData();
            EditorUtility.DisplayProgressBar("提示", "数据检测", 0.6f);
            Instance.CheckDataPath();
            EditorUtility.ClearProgressBar();
            bool bCheck = EditorUtility.DisplayDialog("提示", "检测完成,请查看对应txt文件-M1Client/CheckResult_GameData/模板路径有效性检查.txt", "好的");
            if (bCheck)
            {
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
                string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
                if (Directory.Exists(logDir))
                    Util.OpenDir(logDir);
#endif
            }
        }

        Dictionary<int, Actor> _ActorData;
        Dictionary<int, Asset> _AssetData;
        Dictionary<int, Dress> _DressData;
        Dictionary<int, Horse> _HorseData;
        Dictionary<int, Item> _ItemData;
        Dictionary<int, Mine> _MineData;
        Dictionary<int, Monster> _MonsterData;
        Dictionary<int, Npc> _NpcData;
        Dictionary<int, Obstacle> _ObstacleData;
        Dictionary<int, Profession> _ProfessionData;
        Dictionary<int, SpecialId> _SpecialIdData;
        Dictionary<int, State> _StateData;
        Dictionary<int, Wing> _WingData;


        Dictionary<int, Achievement> _AchievementData;
        Dictionary<int, AdventureGuide> _AdventureGuideData;
        Dictionary<int, DyeAndEmbroidery> _DyeAndEmbroideryData;
        Dictionary<int, Expedition> _ExpeditionData;
        Dictionary<int, Fun> _FunData;
        Dictionary<int, Goods> _GoodsData;
        Dictionary<int, GuildExpedition> _GuildExpeditionData;
        Dictionary<int, GuildIcon> _GuildIconData;
        Dictionary<int, Instance> _InstanceData;
        Dictionary<int, ItemApproach> _ItemApproachData;
        Dictionary<int, ManualEntrie> _ManualEntrieData;
        Dictionary<int, Map> _MapData;
        Dictionary<int, Money> _MoneyData;
        Dictionary<int, MonthlyCard> _MonthlyCardData;
        Dictionary<int, Pet> _PetData;
        Dictionary<int, PlayerStrong> _PlayerStrongData;
        Dictionary<int, Reputation> _ReputationData;
        Dictionary<int, Rune> _RuneData;
        Dictionary<int, Service> _ServiceData;
        Dictionary<int, Skill> _SkillData;
        Dictionary<int, SkillMastery> _SkillMasteryData;
        Dictionary<int, Store> _StoreData;
        Dictionary<int, Talent> _TalentData;
        Dictionary<int, WingTalentPage> _WingTalentPageData;
        Dictionary<int, WorldBossConfig> _WorldBossConfigData;
        Dictionary<int, ExpFind> _ExpFindData;


        /*
            Actor   GfxAssetPath
            Asset   Path
            Dress   AssetPath1
            Dress   AssetPath2
            Dress   FightFxPath
            Horse   ModelAssetPath
            Item    LootModelAssetPath
            Item    MaleModelAssetPath
            Item    FemaleModelAssetPath
            Item    MaleLeftHandModelAssetPath
            Item    FemaleLeftHandModelAssetPath
            Item    MaleRightHandModelAssetPath
            Item    FemaleRightHandModelAssetPath
            Item    InforceEffectPath1_Left
            Item    InforceEffectPath1_Right
            Item    InforceEffectPath2_Left
            Item    InforceEffectPath2_Right
            Item    InforceEffectPath3_Left
            Item    InforceEffectPath3_Right
            Mine    DisappearAssetPath
            Mine    ModelAssetPath
            Mine    EffectAssetPath
            Monster ModelAssetPath
            Npc     ModelAssetPath
            Obstacle    ModelAssetPath
            Profession  MaleModelAssetPath
            Profession  FemaleModelAssetPath
            SpecialId   Value
            State.ExecutionUnits[2].Event.Transform  BodyAssetPath
            State.ExecutionUnits[2].Event.Transform  HairAssetPath
            Wing    ModelAssetPath
            Wing    SpecialEffectPath1 
            Wing    SpecialEffectPath2
            Wing    SpecialEffectPath3
        */

        /*
            Achievement IconPath
            AdventureGuide IconPath 
            AdventureGuide IconPath2
            ColorConfig IconPath    Icon_Base_Path_2
            Dress IconPath          Icon_Base_Path_2
            DyeAndEmbroidery        Icon_Base_Path_2
            Expedition Icon         Icon_Base_Path_0
            Fun IconPath            Icon_Base_Path_2
            Goods IconPath          Icon_Base_Path_2
            GuildExpedition         
            GuildIcon  IconPath     Icon_Base_Path_2
            Horse  IconPath         Icon_Base_Path_2
            Instance IconPath 
            ItemApproach IconPath   Icon_Base_Path_2
            Item IconAtlasPath      Icon_Base_Path_2
            ManualEntrie IconPath   .png
            Map MapIcon             Icon_Base_Path_2
            Money IconPath          Icon_Base_Path_2
            Monster IconAtlasPath   Icon_Base_Path_2
            MonthlyCard IconPath    Icon_Base_Path_2
            Npc HalfPortrait        Icon_Base_Path_0
            Pet IconPath            Icon_Base_Path_2
            Pet GuideIconPath       Icon_Base_Path_2
            PlayerStrong IconPath   Icon_Base_Path_2
            Profession FemaleIconAtlasPath  Icon_Base_Path_2
            Profession MaleIconAtlasPath  Icon_Base_Path_2
            Reputation IconAtlasPath  
            Reputation BackGroundPath 
            Rune RuneIcon           Icon_Base_Path_2
            Rune RuneSmallIcon      Icon_Base_Path_2
            Service OverHeadIcon    Icon_Base_Path_2
            Skill IconName          Icon_Base_Path_2
            SkillMastery IconPath   Icon_Base_Path_2
            State IconPath          Icon_Base_Path_2
            State.ExecutionUnits[0].Event.ChangeSkillIcon.IconPath        Icon_Base_Path_2 
            Store.RecommendStructs[0].RcommendGoodss[0].IconPath    Icon_Base_Path_2
            Talent Icon             Icon_Base_Path_2
            Wing  IconPath          Icon_Base_Path_2
            WingTalentPage IconPath Icon_Base_Path_2
            WorldBossConfig BossIcon   
            WorldBossConfig WorldBossPath
            WorldBossConfig WorldBossTipsPath
            ExpFind     Icon        Icon_Base_Path_2
        */

        public void Init()
        {
            string gameResPath =System.IO.Path.Combine(Application.dataPath, "../../GameRes/");

            //设置assetBundle路径
            string platformFolderForAssetBundles = GetPlatformFolderForAssetBundles();
            _GameResBasePath = gameResPath + AssetBundlesFolderName + platformFolderForAssetBundles + "/";
            _BaseAssetBundleURL = gameResPath + AssetBundlesFolderName + platformFolderForAssetBundles + "/";
            _UpdateAssetBundleURL = _BaseAssetBundleURL + "Update/";

            LoadPathIDData();

            //预先设置路径
            Template.Path.BasePath = System.IO.Path.Combine(Application.dataPath, "../../GameRes/");
            Template.Path.BinPath = "Data/";
        }

        public void InitData()
        {
            //解析所有数据
            foreach (string name in Template.TemplateManagerCollection.TemplateNameCollection)
            {
                var dataMgr = Template.TemplateManagerCollection.GetTemplateManager(name);
                dataMgr.ParseTemplateAll(true);

                //
                if (name == "Actor")
                    _ActorData = (dataMgr as Template.ActorModule.Manager).GetTemplateMap();
                if (name == "Asset")
                    _AssetData = (dataMgr as Template.AssetModule.Manager).GetTemplateMap();
                if (name == "Dress")
                    _DressData = (dataMgr as Template.DressModule.Manager).GetTemplateMap();
                if (name == "Horse")
                    _HorseData = (dataMgr as Template.HorseModule.Manager).GetTemplateMap();
                if (name == "Item")
                    _ItemData = (dataMgr as Template.ItemModule.Manager).GetTemplateMap();
                if (name == "Mine")
                    _MineData = (dataMgr as Template.MineModule.Manager).GetTemplateMap();
                if (name == "Monster")
                    _MonsterData = (dataMgr as Template.MonsterModule.Manager).GetTemplateMap();
                if (name == "Npc")
                    _NpcData = (dataMgr as Template.NpcModule.Manager).GetTemplateMap();
                if (name == "Obstacle")
                    _ObstacleData = (dataMgr as Template.ObstacleModule.Manager).GetTemplateMap();
                if (name == "Profession")
                    _ProfessionData = (dataMgr as Template.ProfessionModule.Manager).GetTemplateMap();
                if (name == "SpecialId")
                    _SpecialIdData = (dataMgr as Template.SpecialIdModule.Manager).GetTemplateMap();
                if (name == "State")
                    _StateData = (dataMgr as Template.StateModule.Manager).GetTemplateMap();
                if (name == "Wing")
                    _WingData = (dataMgr as Template.WingModule.Manager).GetTemplateMap();


                if (name == "Achievement")
                    _AchievementData = (dataMgr as Template.AchievementModule.Manager).GetTemplateMap();
                if (name == "AdventureGuide")
                    _AdventureGuideData = (dataMgr as Template.AdventureGuideModule.Manager).GetTemplateMap();
                if (name == "DyeAndEmbroidery")
                    _DyeAndEmbroideryData = (dataMgr as Template.DyeAndEmbroideryModule.Manager).GetTemplateMap();
                if (name == "Expedition")
                    _ExpeditionData = (dataMgr as Template.ExpeditionModule.Manager).GetTemplateMap();
                if (name == "Fun")
                    _FunData = (dataMgr as Template.FunModule.Manager).GetTemplateMap();
                if (name == "Goods")
                    _GoodsData = (dataMgr as Template.GoodsModule.Manager).GetTemplateMap();
                if (name == "GuildExpedition")
                    _GuildExpeditionData = (dataMgr as Template.GuildExpeditionModule.Manager).GetTemplateMap();
                if (name == "GuildIcon")
                    _GuildIconData = (dataMgr as Template.GuildIconModule.Manager).GetTemplateMap();
                if (name == "Instance")
                    _InstanceData = (dataMgr as Template.InstanceModule.Manager).GetTemplateMap();
                if (name == "ItemApproach")
                    _ItemApproachData = (dataMgr as Template.ItemApproachModule.Manager).GetTemplateMap();
                if (name == "ManualEntrie")
                    _ManualEntrieData = (dataMgr as Template.ManualEntrieModule.Manager).GetTemplateMap();
                if (name == "Map")
                    _MapData = (dataMgr as Template.MapModule.Manager).GetTemplateMap();
                if (name == "Money")
                    _MoneyData = (dataMgr as Template.MoneyModule.Manager).GetTemplateMap();
                if (name == "MonthlyCard")
                    _MonthlyCardData = (dataMgr as Template.MonthlyCardModule.Manager).GetTemplateMap();
                if (name == "Pet")
                    _PetData = (dataMgr as Template.PetModule.Manager).GetTemplateMap();
                if (name == "PlayerStrong")
                    _PlayerStrongData = (dataMgr as Template.PlayerStrongModule.Manager).GetTemplateMap();
                if (name == "Reputation")
                    _ReputationData = (dataMgr as Template.ReputationModule.Manager).GetTemplateMap();
                if (name == "Rune")
                    _RuneData = (dataMgr as Template.RuneModule.Manager).GetTemplateMap();
                if (name == "Service")
                    _ServiceData = (dataMgr as Template.ServiceModule.Manager).GetTemplateMap();
                if (name == "Skill")
                    _SkillData = (dataMgr as Template.SkillModule.Manager).GetTemplateMap();
                if (name == "SkillMastery")
                    _SkillMasteryData = (dataMgr as Template.SkillMasteryModule.Manager).GetTemplateMap();
                if (name == "Store")
                    _StoreData = (dataMgr as Template.StoreModule.Manager).GetTemplateMap();
                if (name == "Talent")
                    _TalentData = (dataMgr as Template.TalentModule.Manager).GetTemplateMap();
                if (name == "WingTalentPage")
                    _WingTalentPageData = (dataMgr as Template.WingTalentPageModule.Manager).GetTemplateMap();
                if (name == "WorldBossConfig")
                    _WorldBossConfigData = (dataMgr as Template.WorldBossConfigModule.Manager).GetTemplateMap();
                if (name == "ExpFind")
                    _ExpFindData = (dataMgr as Template.ExpFindModule.Manager).GetTemplateMap();
            }

          
        }

        void Check_AssetPath()
        {
            foreach(var v in _ActorData.Values)
            {
                if (!IsValidPath(v.GfxAssetPath))
                    AddAssetPathCheckInfo("Actor", v.Id, "GfxAssetPath", v.GfxAssetPath);
            }

            foreach (var v in _AssetData.Values)
            {
                if (!IsValidPath(v.Path))
                    AddAssetPathCheckInfo("Asset", v.Id, "Path", v.Path);
            }

            foreach (var v in _DressData.Values)
            {
                if (!IsValidPath(v.AssetPath1))
                    AddAssetPathCheckInfo("Dress", v.Id, "AssetPath1", v.AssetPath1);

                if (!IsValidPath(v.AssetPath2))
                    AddAssetPathCheckInfo("Dress", v.Id, "AssetPath2", v.AssetPath2);

                if (!IsValidPath(v.FightFxPath))
                    AddAssetPathCheckInfo("Dress", v.Id, "FightFxPath", v.FightFxPath);
            }

            foreach (var v in _HorseData.Values)
            {
                if (!IsValidPath(v.ModelAssetPath))
                    AddAssetPathCheckInfo("Horse", v.Id, "ModelAssetPath", v.ModelAssetPath);
            }

            foreach (var v in _ItemData.Values)
            {
                if (!IsValidPath(v.LootModelAssetPath))
                    AddAssetPathCheckInfo("Item", v.Id, "LootModelAssetPath", v.LootModelAssetPath);

                if (!IsValidPath(v.MaleModelAssetPath))
                    AddAssetPathCheckInfo("Item", v.Id, "MaleModelAssetPath", v.MaleModelAssetPath);

                if (!IsValidPath(v.FemaleModelAssetPath))
                    AddAssetPathCheckInfo("Item", v.Id, "FemaleModelAssetPath", v.FemaleModelAssetPath);

                if (!IsValidPath(v.MaleLeftHandModelAssetPath))
                    AddAssetPathCheckInfo("Item", v.Id, "MaleLeftHandModelAssetPath", v.MaleLeftHandModelAssetPath);

                if (!IsValidPath(v.FemaleLeftHandModelAssetPath))
                    AddAssetPathCheckInfo("Item", v.Id, "FemaleLeftHandModelAssetPath", v.FemaleLeftHandModelAssetPath);

                if (!IsValidPath(v.MaleRightHandModelAssetPath))
                    AddAssetPathCheckInfo("Item", v.Id, "MaleRightHandModelAssetPath", v.MaleRightHandModelAssetPath);

                if (!IsValidPath(v.FemaleRightHandModelAssetPath))
                    AddAssetPathCheckInfo("Item", v.Id, "FemaleRightHandModelAssetPath", v.FemaleRightHandModelAssetPath);

                if (!string.IsNullOrEmpty(v.InforceEffectPath1_Left))
                {
                    string[] ret = v.InforceEffectPath1_Left.Split('*');
                    foreach (string s in ret)
                    {
                        if (!IsValidPath(s))
                            AddAssetPathCheckInfo("Item", v.Id, "InforceEffectPath1_Left", s);
                    }
                }

                if (!string.IsNullOrEmpty(v.InforceEffectPath1_Right))
                {
                    string[] ret = v.InforceEffectPath1_Right.Split('*');
                    foreach (string s in ret)
                    {
                        if (!IsValidPath(s))
                            AddAssetPathCheckInfo("Item", v.Id, "InforceEffectPath1_Right", s);
                    }
                }

                if (!string.IsNullOrEmpty(v.InforceEffectPath2_Left))
                {
                    string[] ret = v.InforceEffectPath2_Left.Split('*');
                    foreach (string s in ret)
                    {
                        if (!IsValidPath(s))
                            AddAssetPathCheckInfo("Item", v.Id, "InforceEffectPath2_Left", s);
                    }
                }

                if (!string.IsNullOrEmpty(v.InforceEffectPath2_Right))
                {
                    string[] ret = v.InforceEffectPath2_Right.Split('*');
                    foreach (string s in ret)
                    {
                        if (!IsValidPath(s))
                            AddAssetPathCheckInfo("Item", v.Id, "InforceEffectPath2_Right", s);
                    }
                }

                if (!string.IsNullOrEmpty(v.InforceEffectPath3_Left))
                {
                    string[] ret = v.InforceEffectPath3_Left.Split('*');
                    foreach (string s in ret)
                    {
                        if (!IsValidPath(s))
                            AddAssetPathCheckInfo("Item", v.Id, "InforceEffectPath3_Left", s);
                    }
                }

                if (!string.IsNullOrEmpty(v.InforceEffectPath3_Right))
                {
                    string[] ret = v.InforceEffectPath3_Right.Split('*');
                    foreach (string s in ret)
                    {
                        if (!IsValidPath(s))
                            AddAssetPathCheckInfo("Item", v.Id, "InforceEffectPath3_Right", s);
                    }
                }
            }

            foreach (var v in _MineData.Values)
            {
                if (!IsValidPath(v.DisappearAssetPath))
                    AddAssetPathCheckInfo("Mine", v.Id, "DisappearAssetPath", v.DisappearAssetPath);

                if (!IsValidPath(v.ModelAssetPath))
                    AddAssetPathCheckInfo("Mine", v.Id, "ModelAssetPath", v.ModelAssetPath);

                if (!IsValidPath(v.EffectAssetPath))
                    AddAssetPathCheckInfo("Mine", v.Id, "EffectAssetPath", v.EffectAssetPath);
            }

            foreach (var v in _MonsterData.Values)
            {
                if (!IsValidPath(v.ModelAssetPath))
                    AddAssetPathCheckInfo("Monster", v.Id, "ModelAssetPath", v.ModelAssetPath);
            }

            foreach (var v in _NpcData.Values)
            {
                if (!IsValidPath(v.ModelAssetPath))
                    AddAssetPathCheckInfo("Npc", v.Id, "ModelAssetPath", v.ModelAssetPath);
            }

            foreach (var v in _ObstacleData.Values)
            {
                if (!IsValidPath(v.ModelAssetPath))
                    AddAssetPathCheckInfo("Obstacle", v.Id, "ModelAssetPath", v.ModelAssetPath);
            }

            foreach (var v in _ProfessionData.Values)
            {
                if (!IsValidPath(v.MaleModelAssetPath))
                    AddAssetPathCheckInfo("Profession", v.Id, "MaleModelAssetPath", v.MaleModelAssetPath);

                if (!IsValidPath(v.FemaleModelAssetPath))
                    AddAssetPathCheckInfo("Profession", v.Id, "FemaleModelAssetPath", v.FemaleModelAssetPath);
            }

            foreach (var v in _SpecialIdData.Values)
            {
                if (!string.IsNullOrEmpty(v.Value) && v.Value.EndsWith(".prefab") && !IsValidPath(v.Value))
                    AddAssetPathCheckInfo("SpecialId", v.Id, "Value", v.Value);
            }

            foreach (var v in _StateData.Values)
            {
                foreach (var unit in v.ExecutionUnits)
                {
                    var trans = unit.Event.Transform;
                    if (trans == null)
                        continue;

                    if (!IsValidPath(trans.BodyAssetPath))
                        AddAssetPathCheckInfo("State", v.Id, "ExecutionUnits.Event.Transform.BodyAsstePath", trans.BodyAssetPath);

                    if (!IsValidPath(trans.HairAssetPath))
                        AddAssetPathCheckInfo("State", v.Id, "ExecutionUnits.Event.Transform.HairAssetPath", trans.HairAssetPath);

                    if (!IsValidPath(trans.HeadAssetPath))
                        AddAssetPathCheckInfo("State", v.Id, "ExecutionUnits.Event.Transform.HeadAssetPath", trans.HeadAssetPath);

                    if (!IsValidPath(trans.WeaponAssetPath))
                        AddAssetPathCheckInfo("State", v.Id, "ExecutionUnits.Event.Transform.WeaponAssetPath", trans.WeaponAssetPath);
                    
                }
            }

            foreach (var v in _WingData.Values)
            {
                if (!IsValidPath(v.ModelAssetPath))
                    AddAssetPathCheckInfo("Wing", v.Id, "ModelAssetPath", v.ModelAssetPath);

                if (!IsValidPath(v.SpecialEffectPath1))
                    AddAssetPathCheckInfo("Wing", v.Id, "SpecialEffectPath1", v.SpecialEffectPath1);

                if (!IsValidPath(v.SpecialEffectPath2))
                    AddAssetPathCheckInfo("Wing", v.Id, "SpecialEffectPath2", v.SpecialEffectPath2);

                if (!IsValidPath(v.SpecialEffectPath3))
                    AddAssetPathCheckInfo("Wing", v.Id, "SpecialEffectPath3", v.SpecialEffectPath3);
            }
        }

        void Check_IconPath()
        {
            foreach (var v in _AchievementData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("Achievement", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _AdventureGuideData.Values)
            {
                if (!IsValidIconPath(string.Empty, v.IconPath, string.Empty))
                    AddIconPathCheckInfo("AdventureGuide", v.Id, "IconPath", v.IconPath);
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath2))
                    AddIconPathCheckInfo("AdventureGuide", v.Id, "IconPath2", v.IconPath2);
            }

            foreach (var v in _DressData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("Dress", v.Id, "IconPath", v.IconPath);
            }

            /*
            foreach (var v in _DyeAndEmbroideryData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("DyeAndEmbroidery", v.Id, "IconPath", v.IconPath);
            }
            */

            foreach (var v in _ExpeditionData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_0, v.Icon))
                    AddIconPathCheckInfo("Expedition", v.Id, "Icon", v.Icon);
            }

            foreach (var v in _FunData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("Fun", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _GoodsData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("Goods", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _GuildExpeditionData.Values)
            {
                if (!IsValidIconPath(string.Empty, v.Icon))
                    AddIconPathCheckInfo("GuildExpedition", v.Id, "Icon", v.Icon);
            }

            foreach (var v in _GuildIconData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("GuildIcon", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _HorseData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("Horse", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _InstanceData.Values)
            {
                if (!IsValidIconPath(string.Empty, v.IconPath, string.Empty))
                    AddIconPathCheckInfo("Instance", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _ItemApproachData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("ItemApproach", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _ItemData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconAtlasPath))
                    AddIconPathCheckInfo("Item", v.Id, "IconAtlasPath", v.IconAtlasPath);
            }

            foreach (var v in _ManualEntrieData.Values)
            {
                if (!IsValidIconPath(string.Empty, v.IconPath))
                    AddIconPathCheckInfo("ManualEntrie", v.Id, "IconPath", v.IconPath);
            }

            /*
            foreach (var v in _MapData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.MapIcon))
                    AddIconPathCheckInfo("Map", v.Id, "MapIcon", v.MapIcon);
            }
             * */

            foreach (var v in _MoneyData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("Money", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _MonsterData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconAtlasPath))
                    AddIconPathCheckInfo("Monster", v.Id, "IconAtlasPath", v.IconAtlasPath);
            }

            foreach (var v in _MonthlyCardData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("MonthlyCard", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _NpcData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_0, v.HalfPortrait))
                    AddIconPathCheckInfo("Npc", v.Id, "HalfPortrait", v.HalfPortrait);
            }

            foreach (var v in _PetData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("Pet", v.Id, "IconPath", v.IconPath);

                if (!IsValidIconPath(Icon_Base_Path_2, v.GuideIconPath))
                    AddIconPathCheckInfo("Pet", v.Id, "GuideIconPath", v.GuideIconPath);
            }

            foreach (var v in _PlayerStrongData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("PlayerStrong", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _ProfessionData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.FemaleIconAtlasPath))
                    AddIconPathCheckInfo("Profession", v.Id, "FemaleIconAtlasPath", v.FemaleIconAtlasPath);

                if (!IsValidIconPath(Icon_Base_Path_2, v.MaleIconAtlasPath))
                    AddIconPathCheckInfo("Profession", v.Id, "MaleIconAtlasPath", v.MaleIconAtlasPath);
            }

            foreach (var v in _ReputationData.Values)
            {
                if (!IsValidIconPath(string.Empty, v.IconAtlasPath))
                    AddIconPathCheckInfo("Reputation", v.Id, "IconAtlasPath", v.IconAtlasPath);

                if (!IsValidIconPath(string.Empty, v.BackGroundPath, string.Empty))
                    AddIconPathCheckInfo("Reputation", v.Id, "BackGroundPath", v.BackGroundPath);
            }

            foreach (var v in _RuneData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.RuneIcon))
                    AddIconPathCheckInfo("Rune", v.Id, "RuneIcon", v.RuneIcon);

                if (!IsValidIconPath(Icon_Base_Path_2, v.RuneSmallIcon))
                    AddIconPathCheckInfo("Rune", v.Id, "RuneSmallIcon", v.RuneSmallIcon);
            }

            foreach (var v in _ServiceData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.OverHeadIcon))
                    AddIconPathCheckInfo("Service", v.Id, "OverHeadIcon", v.OverHeadIcon);
            }

            foreach (var v in _SkillData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconName))
                    AddIconPathCheckInfo("Skill", v.Id, "IconName", v.IconName);
            }

            foreach (var v in _SkillMasteryData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("SkillMastery", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _StateData.Values)
            {
                if (v.IsShowIcon && !IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("State", v.Id, "IconPath", v.IconPath);

                for (int i = 0; i < v.ExecutionUnits.Count; ++i)
                {
                    var unit = v.ExecutionUnits[i];
                    if (unit.Event == null || unit.Event.ChangeSkillIcon == null)
                        continue;

                    if (!IsValidIconPath(Icon_Base_Path_2, unit.Event.ChangeSkillIcon.IconPath))
                        AddIconPathCheckInfo("State", v.Id, string.Format("ExecutionUnits[{0}].Event.ChangeSkillIcon.IconPath", i), unit.Event.ChangeSkillIcon.IconPath);
                }
            }

            foreach (var v in _StoreData.Values)
            {
                for (int i = 0; i < v.RecommendStructs.Count; ++i)
                {
                    var recommendStruct = v.RecommendStructs[i];
                    if (recommendStruct == null)
                        continue;

                    for (int k = 0; k < recommendStruct.RcommendGoodss.Count; ++k)
                    {
                        var goods = recommendStruct.RcommendGoodss[k];
                        if (goods == null)
                            continue;
                        if (!IsValidIconPath(Icon_Base_Path_2, goods.IconPath))
                            AddIconPathCheckInfo("Store", v.Id, string.Format("RecommendStructs[{0}].RcommendGoodss[{1}].IconPath", i, k), goods.IconPath);
                    }
                }
            }

            foreach (var v in _TalentData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.Icon))
                    AddIconPathCheckInfo("Talent", v.Id, "Icon", v.Icon);
            }

            foreach (var v in _WingData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("Wing", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _WingTalentPageData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.IconPath))
                    AddIconPathCheckInfo("WingTalentPage", v.Id, "IconPath", v.IconPath);
            }

            foreach (var v in _WorldBossConfigData.Values)
            {
                if (!IsValidIconPath(string.Empty, v.BossIcon))
                    AddIconPathCheckInfo("WorldBossConfig", v.Id, "BossIcon", v.BossIcon);

                if (!IsValidIconPath(string.Empty, v.WorldBossPath, string.Empty))
                    AddIconPathCheckInfo("WorldBossConfig", v.Id, "WorldBossPath", v.WorldBossPath);

                if (!IsValidIconPath(string.Empty, v.WorldBossTipsPath))
                    AddIconPathCheckInfo("WorldBossConfig", v.Id, "WorldBossTipsPath", v.WorldBossTipsPath);
            }

            foreach (var v in _ExpFindData.Values)
            {
                if (!IsValidIconPath(Icon_Base_Path_2, v.Icon))
                    AddIconPathCheckInfo("ExpFind", v.Id, "Icon", v.Icon);
            }
        }

        private void CheckDataPath()
        {
            Check_AssetPath();

            Check_IconPath();

            LogReport("模板路径有效性检查.txt");
        }

        private void LogReport(string filename)
        {
            //写log 
            string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            string logFile = System.IO.Path.Combine(logDir, filename);
            TextLogger.Path = logFile;
            File.Delete(logFile);

            //

            SortPathCheckInfo(_DicAssetPathInfo);
            SortPathCheckInfo(_DicIconPathInfo);

            if (_DicAssetPathInfo.Count == 0 && _DicIconPathInfo.Count == 0)
            {
                //EditorUtility.DisplayDialog("完成", "检查通过!", "确定");
                TextLogger.Instance.WriteLine("模板路径有效性检查!");
            }
            else
            {
                TextLogger.Instance.WriteLine("");
                TextLogger.Instance.WriteLine("检查Asset路径...\n\n");
                foreach (var kv in _DicAssetPathInfo)
                {
                    string templateName = kv.Key;
                    List<PathCheckInfo> list = kv.Value;

                    TextLogger.Instance.WriteLine("");
                    TextLogger.Instance.WriteLine(string.Format("检查模板: {0}\n", templateName));
                    foreach (var item in list)
                    {
                        TextLogger.Instance.WriteLine(string.Format("Asset路径错误, TID: {0}, 字段: {1}, 路径: {2}", item.TID, item.FieldName, item.ErrorPath));
                    }
                }

                TextLogger.Instance.WriteLine("");
                TextLogger.Instance.WriteLine("检查Icon路径...\n\n");
                foreach (var kv in _DicIconPathInfo)
                {
                    string templateName = kv.Key;
                    List<PathCheckInfo> list = kv.Value;

                    TextLogger.Instance.WriteLine("");
                    TextLogger.Instance.WriteLine(string.Format("检查模板: {0}\n", templateName));
                    foreach (var item in list)
                    {
                        TextLogger.Instance.WriteLine(string.Format("Icon路径错误, TID: {0}, 字段: {1}, 路径: {2}", item.TID, item.FieldName, item.ErrorPath));
                    }
                }
            }
        }
    }
}