using System.Collections.Generic;
using Common;
using System.IO;
using System;

// ReSharper disable once CheckNamespace
namespace Template
{
    public class TemplateFileInfo
    {
        public List<int> KeyList = new List<int>();
    }

    public interface ITemplateManager : ITemplateDataManager
    {
        void ParseTemplateAll(bool bCleanData = false);

        void UpdateTemplateDataFromTemplate();

        float GetHeaderVersion();

        int GetHeaderData();

        List<TemplateFileInfo> GetTemplateFileInfoList();
    }

    public static class TemplateManagerCollection
    {
        public delegate ITemplateManager OnGetDataTemplateManager();

        public static Dictionary<string, ITemplateManager> TemplateManagerMap = new Dictionary<string, ITemplateManager>();

        public static void ClearAll()
        {
            foreach(var kv in TemplateManagerMap)
            {
                kv.Value.Clear();
            }
            TemplateManagerMap.Clear();
        }

        public static Dictionary<string, OnGetDataTemplateManager>.KeyCollection TemplateNameCollection
        {
            get { return DicGetTemplateManager.Keys; }
        }

        private static Dictionary<string, OnGetDataTemplateManager> DicGetTemplateManager = new Dictionary<string, OnGetDataTemplateManager>();

        static TemplateManagerCollection()
        {
            DicGetTemplateManager["ActivityStore"] = delegate { return ActivityStoreModule.Manager.Instance; };
            DicGetTemplateManager["Achievement"] = delegate { return AchievementModule.Manager.Instance; };
            DicGetTemplateManager["Actor"] = delegate { return ActorModule.Manager.Instance; };
            DicGetTemplateManager["AdventureGuide"] = delegate { return AdventureGuideModule.Manager.Instance; };
            DicGetTemplateManager["Asset"] = delegate { return AssetModule.Manager.Instance; };
            DicGetTemplateManager["AttachedPropertyGenerator"] = delegate { return AttachedPropertyGeneratorModule.Manager.Instance; };
            DicGetTemplateManager["AttachedPropertyGroupGenerator"] = delegate { return AttachedPropertyGroupGeneratorModule.Manager.Instance; };
            DicGetTemplateManager["AttachedProperty"] = delegate { return AttachedPropertyModule.Manager.Instance; };
            DicGetTemplateManager["Banner"] = delegate { return BannerModule.Manager.Instance; };
            DicGetTemplateManager["CharmItem"] = delegate { return CharmItemModule.Manager.Instance; };
            DicGetTemplateManager["CharmPage"] = delegate { return CharmPageModule.Manager.Instance; };
            DicGetTemplateManager["CharmField"] = delegate { return CharmFieldModule.Manager.Instance; };
            DicGetTemplateManager["CharmUpgrade"] = delegate { return CharmUpgradeModule.Manager.Instance; };
            DicGetTemplateManager["CountGroup"] = delegate { return CountGroupModule.Manager.Instance; };
            DicGetTemplateManager["CyclicQuest"] = delegate { return CyclicQuestModule.Manager.Instance; };
            DicGetTemplateManager["CyclicQuestReward"] = delegate { return CyclicQuestRewardModule.Manager.Instance; };
            DicGetTemplateManager["Designation"] = delegate { return DesignationModule.Manager.Instance; };
            DicGetTemplateManager["Dialogue"] = delegate { return DialogueModule.Manager.Instance; };
            DicGetTemplateManager["Dress"] = delegate { return DressModule.Manager.Instance; };
            DicGetTemplateManager["DressScore"] = delegate { return DressScoreModule.Manager.Instance; };
            DicGetTemplateManager["DropLibrary"] = delegate { return DropLibraryModule.Manager.Instance; };
            DicGetTemplateManager["DropLimit"] = delegate { return DropLimitModule.Manager.Instance; };
            DicGetTemplateManager["DropRule"] = delegate { return DropRuleModule.Manager.Instance; };
            DicGetTemplateManager["DyeAndEmbroidery"] = delegate { return DyeAndEmbroideryModule.Manager.Instance; };
            DicGetTemplateManager["EliminateReward"] = delegate { return EliminateRewardModule.Manager.Instance; };
            DicGetTemplateManager["Email"] = delegate { return EmailModule.Manager.Instance; };
            DicGetTemplateManager["Enchant"] = delegate { return EnchantModule.Manager.Instance; };
            DicGetTemplateManager["EquipConsumeConfig"] = delegate { return EquipConsumeConfigModule.Manager.Instance; };
            DicGetTemplateManager["EquipInforce"] = delegate { return EquipInforceModule.Manager.Instance; };
            DicGetTemplateManager["EquipRefine"] = delegate { return EquipRefineModule.Manager.Instance; };
            DicGetTemplateManager["EquipSuit"] = delegate { return EquipSuitModule.Manager.Instance; };
            DicGetTemplateManager["ExecutionUnit"] = delegate { return ExecutionUnitModule.Manager.Instance; };
            DicGetTemplateManager["Expedition"] = delegate { return ExpeditionModule.Manager.Instance; };
            DicGetTemplateManager["Faction"] = delegate { return FactionModule.Manager.Instance; };
            DicGetTemplateManager["FactionRelationship"] = delegate { return FactionRelationshipModule.Manager.Instance; };
            DicGetTemplateManager["FightPropertyConfig"] = delegate { return FightPropertyConfigModule.Manager.Instance; };
            DicGetTemplateManager["FightProperty"] = delegate { return FightPropertyModule.Manager.Instance; };
            DicGetTemplateManager["Fortress"] = delegate { return FortressModule.Manager.Instance; };
            DicGetTemplateManager["Fund"] = delegate { return FundModule.Manager.Instance; };
            DicGetTemplateManager["Fun"] = delegate { return FunModule.Manager.Instance; };
            DicGetTemplateManager["GloryLevel"] = delegate { return GloryLevelModule.Manager.Instance; };
            DicGetTemplateManager["Goods"] = delegate { return GoodsModule.Manager.Instance; };
            DicGetTemplateManager["GuildBattle"] = delegate { return GuildBattleModule.Manager.Instance; };
            DicGetTemplateManager["GuildDonate"] = delegate { return GuildDonateModule.Manager.Instance; };
            DicGetTemplateManager["GuildBuildLevel"] = delegate { return GuildBuildLevelModule.Manager.Instance; };
            DicGetTemplateManager["GuildConvoy"] = delegate { return GuildConvoyModule.Manager.Instance; };
            DicGetTemplateManager["GuildDefend"] = delegate { return GuildDefendModule.Manager.Instance; };
            DicGetTemplateManager["GuildSkill"] = delegate { return GuildSkillModule.Manager.Instance; };
            DicGetTemplateManager["GuildExpedition"] = delegate { return GuildExpeditionModule.Manager.Instance; };
            DicGetTemplateManager["GuildIcon"] = delegate { return GuildIconModule.Manager.Instance; };
            DicGetTemplateManager["GuildLevel"] = delegate { return GuildLevelModule.Manager.Instance; };
            DicGetTemplateManager["GuildPermission"] = delegate { return GuildPermissionModule.Manager.Instance; };
            DicGetTemplateManager["GuildPrayItem"] = delegate { return GuildPrayItemModule.Manager.Instance; };
            DicGetTemplateManager["GuildPrayPool"] = delegate { return GuildPrayPoolModule.Manager.Instance; };
            DicGetTemplateManager["GuildRewardPoints"] = delegate { return GuildRewardPointsModule.Manager.Instance; };
            DicGetTemplateManager["GuildSalary"] = delegate { return GuildSalaryModule.Manager.Instance; };
            DicGetTemplateManager["GuildShop"] = delegate { return GuildShopModule.Manager.Instance; };
            DicGetTemplateManager["GuildBuff"] = delegate { return GuildBuffModule.Manager.Instance; };
            DicGetTemplateManager["GuildSmithy"] = delegate { return GuildSmithyModule.Manager.Instance; };
            DicGetTemplateManager["GuildWareHouseLevel"] = delegate { return GuildWareHouseLevelModule.Manager.Instance; };
            //DicGetTemplateManager["Hearsay"] = delegate { return HearsayModule.Manager.Instance; };
            DicGetTemplateManager["GuildBuildLevel"] = delegate { return GuildBuildLevelModule.Manager.Instance; };
            DicGetTemplateManager["Horse"] = delegate { return HorseModule.Manager.Instance; };
            DicGetTemplateManager["Instance"] = delegate { return InstanceModule.Manager.Instance; };
            DicGetTemplateManager["ItemApproach"] = delegate { return ItemApproachModule.Manager.Instance; };
            DicGetTemplateManager["ItemMachining"] = delegate { return ItemMachiningModule.Manager.Instance; };
            DicGetTemplateManager["Item"] = delegate { return ItemModule.Manager.Instance; };
            DicGetTemplateManager["LegendaryGroup"] = delegate { return LegendaryGroupModule.Manager.Instance; };
            DicGetTemplateManager["LegendaryPropertyUpgrade"] = delegate { return LegendaryPropertyUpgradeModule.Manager.Instance; };
            DicGetTemplateManager["Letter"] = delegate { return LetterModule.Manager.Instance; };
            DicGetTemplateManager["LevelUpExp"] = delegate { return LevelUpExpModule.Manager.Instance; };
            DicGetTemplateManager["Liveness"] = delegate { return LivenessModule.Manager.Instance; };
            DicGetTemplateManager["ManualAnecdote"] = delegate { return ManualAnecdoteModule.Manager.Instance; };
            DicGetTemplateManager["ManualTotalReward"] = delegate { return ManualTotalRewardModule.Manager.Instance; };
            DicGetTemplateManager["ManualEntrie"] = delegate { return ManualEntrieModule.Manager.Instance; };
            DicGetTemplateManager["Map"] = delegate { return MapModule.Manager.Instance; };
            DicGetTemplateManager["MarketItem"] = delegate { return MarketItemModule.Manager.Instance; };
            DicGetTemplateManager["Market"] = delegate { return MarketModule.Manager.Instance; };
            DicGetTemplateManager["MetaFightPropertyConfig"] = delegate { return MetaFightPropertyConfigModule.Manager.Instance; };
            DicGetTemplateManager["Mine"] = delegate { return MineModule.Manager.Instance; };
            DicGetTemplateManager["Money"] = delegate { return MoneyModule.Manager.Instance; };
            DicGetTemplateManager["MonsterAffix"] = delegate { return MonsterAffixModule.Manager.Instance; };
            DicGetTemplateManager["Monster"] = delegate { return MonsterModule.Manager.Instance; };
            DicGetTemplateManager["MonsterPosition"] = delegate { return MonsterPositionModule.Manager.Instance; };
            DicGetTemplateManager["MonsterProperty"] = delegate { return MonsterPropertyModule.Manager.Instance; };
            DicGetTemplateManager["MonthlyCard"] = delegate { return MonthlyCardModule.Manager.Instance; };
            DicGetTemplateManager["NavigationData"] = delegate { return NavigationDataModule.Manager.Instance; };
            DicGetTemplateManager["Npc"] = delegate { return NpcModule.Manager.Instance; };
            DicGetTemplateManager["NpcSale"] = delegate { return NpcSaleModule.Manager.Instance; };
            DicGetTemplateManager["NpcShop"] = delegate { return NpcShopModule.Manager.Instance; };
            DicGetTemplateManager["Obstacle"] = delegate { return ObstacleModule.Manager.Instance; };
            DicGetTemplateManager["PetLevel"] = delegate { return PetLevelModule.Manager.Instance; };
            DicGetTemplateManager["Pet"] = delegate { return PetModule.Manager.Instance; };
            DicGetTemplateManager["PetQualityInfo"] = delegate { return PetQualityInfoModule.Manager.Instance; };
            DicGetTemplateManager["PlayerStrongCell"] = delegate { return PlayerStrongCellModule.Manager.Instance; };
            DicGetTemplateManager["PlayerStrong"] = delegate { return PlayerStrongModule.Manager.Instance; };
            DicGetTemplateManager["PlayerStrongValue"] = delegate { return PlayerStrongValueModule.Manager.Instance; };
            DicGetTemplateManager["Profession"] = delegate { return ProfessionModule.Manager.Instance; };
            DicGetTemplateManager["PublicDrop"] = delegate { return PublicDropModule.Manager.Instance; };
            DicGetTemplateManager["PVP3v3"] = delegate { return PVP3v3Module.Manager.Instance; };
            DicGetTemplateManager["QuestChapter"] = delegate { return QuestChapterModule.Manager.Instance; };
            DicGetTemplateManager["QuestGroup"] = delegate { return QuestGroupModule.Manager.Instance; };
            DicGetTemplateManager["Quest"] = delegate { return QuestModule.Manager.Instance; };
            DicGetTemplateManager["QuickStore"] = delegate { return QuickStoreModule.Manager.Instance; };
            DicGetTemplateManager["Rank"] = delegate { return RankModule.Manager.Instance; };
            DicGetTemplateManager["RankReward"] = delegate { return RankRewardModule.Manager.Instance; };
            DicGetTemplateManager["Reputation"] = delegate { return ReputationModule.Manager.Instance; };
            DicGetTemplateManager["Reward"] = delegate { return RewardModule.Manager.Instance; };
            DicGetTemplateManager["RuneLevelUp"] = delegate { return RuneLevelUpModule.Manager.Instance; };
            DicGetTemplateManager["Rune"] = delegate { return RuneModule.Manager.Instance; };
            DicGetTemplateManager["ScriptCalendar"] = delegate { return ScriptCalendarModule.Manager.Instance; };
            DicGetTemplateManager["ScriptConfig"] = delegate { return ScriptConfigModule.Manager.Instance; };
            //DicGetTemplateManager["SensitiveWord"] = delegate { return SensitiveWordModule.Manager.Instance; };
            DicGetTemplateManager["Service"] = delegate { return ServiceModule.Manager.Instance; };
            DicGetTemplateManager["Sign"] = delegate { return SignModule.Manager.Instance; };
            DicGetTemplateManager["SkillLearnCondition"] = delegate { return SkillLearnConditionModule.Manager.Instance; };
            DicGetTemplateManager["SkillLevelUpCondition"] = delegate { return SkillLevelUpConditionModule.Manager.Instance; };
            DicGetTemplateManager["SkillLevelUp"] = delegate { return SkillLevelUpModule.Manager.Instance; };
            DicGetTemplateManager["Skill"] = delegate { return SkillModule.Manager.Instance; };
            DicGetTemplateManager["SpecialId"] = delegate { return SpecialIdModule.Manager.Instance; };
            DicGetTemplateManager["SkillMastery"] = delegate { return SkillMasteryModule.Manager.Instance; };
            DicGetTemplateManager["State"] = delegate { return StateModule.Manager.Instance; };
            DicGetTemplateManager["Store"] = delegate { return StoreModule.Manager.Instance; };
            DicGetTemplateManager["StoreTag"] = delegate { return StoreTagModule.Manager.Instance; };
            DicGetTemplateManager["Suit"] = delegate { return SuitModule.Manager.Instance; };
            DicGetTemplateManager["SystemNotify"] = delegate { return SystemNotifyModule.Manager.Instance; };
            DicGetTemplateManager["TalentGroup"] = delegate { return TalentGroupModule.Manager.Instance; };
            DicGetTemplateManager["TalentLevelUp"] = delegate { return TalentLevelUpModule.Manager.Instance; };
            DicGetTemplateManager["Talent"] = delegate { return TalentModule.Manager.Instance; };
            DicGetTemplateManager["TeamRoomConfig"] = delegate { return TeamRoomConfigModule.Manager.Instance; };
            DicGetTemplateManager["Text"] = delegate { return TextModule.Manager.Instance; };
            DicGetTemplateManager["TowerDungeon"] = delegate { return TowerDungeonModule.Manager.Instance; };
            DicGetTemplateManager["Trans"] = delegate { return TransModule.Manager.Instance; };
            DicGetTemplateManager["User"] = delegate { return UserModule.Manager.Instance; };
            DicGetTemplateManager["WingGradeUp"] = delegate { return WingGradeUpModule.Manager.Instance; };
            DicGetTemplateManager["WingLevelUp"] = delegate { return WingLevelUpModule.Manager.Instance; };
            DicGetTemplateManager["WingLevelWeight"] = delegate { return WingLevelWeightModule.Manager.Instance; };
            DicGetTemplateManager["Wing"] = delegate { return WingModule.Manager.Instance; };
            DicGetTemplateManager["WingTalentLevel"] = delegate { return WingTalentLevelModule.Manager.Instance; };
            DicGetTemplateManager["WingTalent"] = delegate { return WingTalentModule.Manager.Instance; };
            DicGetTemplateManager["WingTalentPage"] = delegate { return WingTalentPageModule.Manager.Instance; };
            DicGetTemplateManager["WorldBossConfig"] = delegate { return WorldBossConfigModule.Manager.Instance; };
            DicGetTemplateManager["EquipSurmount"] = delegate { return EquipSurmountModule.Manager.Instance; };
            DicGetTemplateManager["EquipQuench"] = delegate { return EquipQuenchModule.Manager.Instance; };
            DicGetTemplateManager["EquipInherit"] = delegate { return EquipInheritModule.Manager.Instance; };
            DicGetTemplateManager["LuckyInforce"] = delegate { return LuckyInforceModule.Manager.Instance; };
            DicGetTemplateManager["StoneInforce"] = delegate { return StoneInforceModule.Manager.Instance; };
            DicGetTemplateManager["TaskLuck"] = delegate { return TaskLuckModule.Manager.Instance; };
            DicGetTemplateManager["DailyTask"] = delegate { return DailyTaskModule.Manager.Instance; };
            DicGetTemplateManager["DailyTaskBox"] = delegate { return DailyTaskBoxModule.Manager.Instance; };
            DicGetTemplateManager["SpecialSign"] = delegate { return SpecialSignModule.Manager.Instance; };
            DicGetTemplateManager["HotTimeConfigure"] = delegate { return HotTimeConfigureModule.Manager.Instance; };
            DicGetTemplateManager["DungeonIntroductionPopup"] = delegate { return DungeonIntroductionPopupModule.Manager.Instance; };
            DicGetTemplateManager["InforceDecompose"] = delegate { return InforceDecomposeModule.Manager.Instance; };
            DicGetTemplateManager["OnlineReward"] = delegate { return OnlineRewardModule.Manager.Instance; };
            DicGetTemplateManager["ExpFind"] = delegate { return ExpFindModule.Manager.Instance; };
            DicGetTemplateManager["HangQuest"] = delegate { return HangQuestModule.Manager.Instance; };
            DicGetTemplateManager["GrowthGuidance"] = delegate { return GrowthGuidanceModule.Manager.Instance; };
            DicGetTemplateManager["EliteBossConfig"] = delegate { return EliteBossConfigModule.Manager.Instance; };
            DicGetTemplateManager["FestivalActivity"] = delegate { return FestivalActivityModule.Manager.Instance; };
            DicGetTemplateManager["Dice"] = delegate { return DiceModule.Manager.Instance; };
        }


        public static ITemplateManager GetTemplateManager(string name)
        {
            ITemplateManager v;
            if (TemplateManagerMap.TryGetValue(name, out v))
                return v;

            v = null;

            OnGetDataTemplateManager fnGetTemplateManager;
            if (DicGetTemplateManager.TryGetValue(name, out fnGetTemplateManager))
                v = fnGetTemplateManager.Invoke();      

            if (v != null)
                TemplateManagerMap.Add(name, v);

            return v;
        }
    }

    public class BaseManager<TThis, TTemplate> : ITemplateManager
        where TThis : BaseManager<TThis, TTemplate>, new()
        where TTemplate : TemplateBase
    {
        public static TThis Instance
        {
            get 
            {
                if (_Instance == null)
                    _Instance = new TThis();
                return _Instance;
            } 
        }

        private static TThis _Instance;

        public void Clear()
        {
            TemplateFileInfoList.Clear();
            TemplateDataMap.Clear();
            TemplateMap.Clear();
            _Instance = null;
        }


        protected readonly Dictionary<int, byte[]> TemplateDataMap = new Dictionary<int, byte[]>();

        protected readonly List<TemplateFileInfo> TemplateFileInfoList = new List<TemplateFileInfo>();

        protected float HeaderVersion = 0;
        protected int HeaderData = 0;

        public Dictionary<int, byte[]> GetTemplateDataMap()
        {
            return TemplateDataMap;
        }

        protected readonly Dictionary<int, TTemplate> TemplateMap = new Dictionary<int, TTemplate>();

        public TTemplate Deserialize(System.IO.Stream input)
        {
            return ProtoBuf.Serializer.Deserialize<TTemplate>(input);
        }

        private void Serialize(System.IO.Stream output, TTemplate templ)
        {
            ProtoBuf.Serializer.Serialize<TTemplate>(output, templ);
        }

        public Dictionary<int, TTemplate> GetTemplateMap()
        {
            return TemplateMap;
        }

        public TTemplate GetTemplate(int tid)
        {
            TTemplate result;
            if (TemplateMap.TryGetValue(tid, out result))
                return result;

            byte[] pbData;
            if (!TemplateDataMap.TryGetValue(tid, out pbData))
            {
                TemplateMap[tid] = null;
                result = null;
            }
            else
            {
                using (var memoryStream = new MemoryStream(pbData))
                {
                    //var template = Serializer.Deserialize<TTemplate>(memoryStream);
                    var template = Deserialize(memoryStream);
                    TemplateMap[tid] = template;
                    result = template;
                }
            }
            return result;
        }

        public float GetHeaderVersion()
        {
            return HeaderVersion;
        }

        public int GetHeaderData()
        {
            return HeaderData;
        }

        public List<TemplateFileInfo> GetTemplateFileInfoList()
        {
            return TemplateFileInfoList;
        }

        public void ParseTemplateAll(bool bCleanData = false)
        {
            foreach (var v in TemplateDataMap)
            {
                using (var memoryStream = new MemoryStream(v.Value))
                {
                    //var template = Serializer.Deserialize<TTemplate>(memoryStream);
                    var template = Deserialize(memoryStream);
                    TemplateMap[v.Key] = template;
                }
            }
            if (bCleanData)
                TemplateDataMap.Clear();
        }

        public void UpdateTemplateDataFromTemplate()
        {
            foreach (var kv in TemplateMap)
            {
                using (var memoryStream = new MemoryStream())
                {
                    //Serializer.Serialize<TTemplate>(memoryStream, kv.Value);
                    Serialize(memoryStream, kv.Value);
                    TemplateDataMap[kv.Key] = memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// data文件头有8个字节
        /// </summary>
        /// <param name="memStream"></param>
        /// <param name="version"></param>
        /// <param name="data"></param>
        private void ReadHeader(BinaryReader br, out float version, out int data)
        {
            version = br.ReadSingle();
            data = br.ReadInt32();
        }

        public byte[] GetTemplateData(int tid)
        {
            byte[] v;
            if (TemplateDataMap.TryGetValue(tid, out v))
                return v;
            else
                return null;
        }

        protected void LoadFromBin(string filePathName, bool isClearOldData = true)
        {
            if (isClearOldData)
            {
                TemplateMap.Clear();
                TemplateDataMap.Clear();
                TemplateFileInfoList.Clear();
            }

            TemplateFileInfo templateFileInfo = new TemplateFileInfo();

            string fullPathName;
            if (!Template.Path.IsLocaleDataPath(filePathName))
                fullPathName = System.IO.Path.Combine(Path.GetFullBinPath(), filePathName);
            else
                fullPathName = System.IO.Path.Combine(Path.GetFullLocalePath(), filePathName);
  
#if UNITY_IPHONE
			fullPathName = fullPathName.Replace("file://", "");
#endif

            byte[] allbytes = null;
            try
            {
#if UNITY_EDITOR
                allbytes = File.ReadAllBytes(fullPathName);
#else
                allbytes = Util.ReadFile(fullPathName);
#endif
            }
            catch(Exception e)
            {
                HobaDebuger.LogWarningFormat("Exception in LoadFromBin: {0}!", e.Message);
                allbytes = null;
            }

            if (allbytes == null)
                return;

            MemoryStream memStream = new MemoryStream(allbytes);
            BinaryReader br = new BinaryReader(memStream);
            try
            {
                var totalSize = 0;
                //memStream.Seek(8, SeekOrigin.Current);
                ReadHeader(br, out HeaderVersion, out HeaderData);
                totalSize = 8;
                while (totalSize < memStream.Length)
                {
                    var tid = br.ReadInt32();
                    var size = br.ReadInt32();
                    byte[] pb = new byte[size];
                    memStream.Read(pb, 0, size);
                    if (TemplateDataMap.ContainsKey(tid))
                    {
                        throw new DataException(HobaText.Format("重复的 {0} ID({1})", typeof(TTemplate).Name, tid));
                    }
                    TemplateDataMap.Add(tid, pb);
                    totalSize += (pb.Length + 2 * sizeof(int));

                    templateFileInfo.KeyList.Add(tid);
                }
            }
            catch (EndOfStreamException e)
            {
                HobaDebuger.LogErrorFormat("The length of data file {0} is error, {1}", fullPathName, e.Message);
            }
            catch (IOException)
            {
                HobaDebuger.LogErrorFormat("IOException raised in file {0}!", fullPathName);
            }
            finally
            {
                br.Close();
                memStream.Close();
            }

            TemplateFileInfoList.Add(templateFileInfo);
        }
    };

}
