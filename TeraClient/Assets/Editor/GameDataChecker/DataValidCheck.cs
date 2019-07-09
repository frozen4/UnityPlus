using Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Template;

#pragma warning disable 0414

namespace GameDataChecker
{
    public class DataValidCheck : Singleton<DataValidCheck>
    {
        
        [MenuItem("Tools/游戏数据检查/模板关联数据有效性检查", false, 3)]
        public static void Check()
        {
            EditorUtility.DisplayProgressBar("提示", "数据初始化", 0.3f);
            Instance.Init();
            Instance.InitData();
            EditorUtility.DisplayProgressBar("提示", "数据检测", 0.6f);
            Instance.CheckData();
            EditorUtility.ClearProgressBar();
            bool bCheck = EditorUtility.DisplayDialog("提示", "检测完成,请查看对应txt文件-M1Client/CheckResult_GameData/模板关联数据有效性检查.txt", "好的");
            if (bCheck)
            {
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
                string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
                if (Directory.Exists(logDir))
                    Util.OpenDir(logDir);
#endif
            }
        }

        Dictionary<int, Achievement> _AchievementData;
        Dictionary<int, Actor> _ActorData;
        Dictionary<int, Asset> _AssetData;
        Dictionary<int, AttachedPropertyGenerator> _AttachedPropertyGeneratorData;
        Dictionary<int, AttachedPropertyGroupGenerator> _AttachedPropertyGroupGeneratorData;
        Dictionary<int, AttachedProperty> _AttachedPropertyData;
        Dictionary<int, Designation> _DesignationData;
        Dictionary<int, Dialogue> _DialogueData;
        Dictionary<int, Dress> _DressData;
        Dictionary<int, DropLibrary> _DropLibraryData;
        Dictionary<int, DropRule> _DropRuleData;
        Dictionary<int, DyeAndEmbroidery> _DyeAndEmbroideryData;
        Dictionary<int, EquipConsumeConfig> _EquipConsumeConfigData;
        Dictionary<int, EquipInforce> _EquipInforceData;
        Dictionary<int, EquipInherit> _EquipInheritData;
        Dictionary<int, EquipRefine> _EquipRefineData;
        Dictionary<int, EquipSurmount> _EquipSurmountData;
        Dictionary<int, EquipQuench> _EquipQuenchData;
        Dictionary<int, ExecutionUnit> _ExecutionUnitData;
        Dictionary<int, FightProperty> _FightPropertyData;
        Dictionary<int, Fun> _FunData;
        Dictionary<int, Horse> _HorseData;
        Dictionary<int, Instance> _InstanceData;
        Dictionary<int, Item> _ItemData;
        Dictionary<int, MetaFightPropertyConfig> _MetaFightPropertyConfigData;
        Dictionary<int, Mine> _MineData;
        Dictionary<int, Money> _MoneyData;
        Dictionary<int, Monster> _MonsterData;
        Dictionary<int, MonsterProperty> _MonsterPropertyData;
        Dictionary<int, NpcSale> _NpcSaleData;
        Dictionary<int, Quest> _QuestData;
        Dictionary<int, QuestChapter> _QuestChapterData;
        Dictionary<int, Reward> _RewardData;
        Dictionary<int, LegendaryGroup> _LegendaryGroupData;
        Dictionary<int, ItemApproach> _ItemApproachData;
        Dictionary<int, ItemMachining> _ItemMachiningData;
        Dictionary<int, Enchant> _EnchantData;
        Dictionary<int, Talent> _TalentData;
        Dictionary<int, Pet> _PetData;
        Dictionary<int, WingTalentLevel> _WingTalentLevelData;
        Dictionary<int, Wing> _WingData;
        Dictionary<int, WingTalentPage> _WingTalentPageData;
        Dictionary<int, CharmItem> _CharmItemData;
        Dictionary<int, CharmUpgrade> _CharmUpgradeData;
        Dictionary<int, GuildSmithy> _GuildSmithyData;

        //data.proto
        private const int ItemEvent_EquipEnchant = 20;

        public void Init()
        {
            //预先设置路径
            Template.Path.BasePath = System.IO.Path.Combine(Application.dataPath, "../../GameRes/");
            Template.Path.BinPath = "Data/";
        }

        /// <summary>
        /// 初始化所有需要检测资源的Data
        /// </summary>
        public void InitData()
        {
            //解析所有数据
            foreach (string name in Template.TemplateManagerCollection.TemplateNameCollection)
            {
                var dataMgr = Template.TemplateManagerCollection.GetTemplateManager(name);
                dataMgr.ParseTemplateAll(true);

                //
                if (name == "Achievement")
                    _AchievementData = (dataMgr as Template.AchievementModule.Manager).GetTemplateMap();
                else if (name == "Actor")
                    _ActorData = (dataMgr as Template.ActorModule.Manager).GetTemplateMap();
                else if (name == "Asset")
                    _AssetData = (dataMgr as Template.AssetModule.Manager).GetTemplateMap();
                else if (name == "AttachedPropertyGenerator")
                    _AttachedPropertyGeneratorData = (dataMgr as Template.AttachedPropertyGeneratorModule.Manager).GetTemplateMap();
                else if (name == "AttachedPropertyGroupGenerator")
                    _AttachedPropertyGroupGeneratorData = (dataMgr as Template.AttachedPropertyGroupGeneratorModule.Manager).GetTemplateMap();
                else if (name == "AttachedProperty")
                    _AttachedPropertyData = (dataMgr as Template.AttachedPropertyModule.Manager).GetTemplateMap();
                else if (name == "Designation")
                    _DesignationData = (dataMgr as Template.DesignationModule.Manager).GetTemplateMap();
                else if (name == "Dialogue")
                    _DialogueData = (dataMgr as Template.DialogueModule.Manager).GetTemplateMap();
                else if (name == "Dress")
                    _DressData = (dataMgr as Template.DressModule.Manager).GetTemplateMap();
                else if (name == "DropLibrary")
                    _DropLibraryData = (dataMgr as Template.DropLibraryModule.Manager).GetTemplateMap();
                else if (name == "DropRule")
                    _DropRuleData = (dataMgr as Template.DropRuleModule.Manager).GetTemplateMap();
                else if (name == "DyeAndEmbroidery")
                    _DyeAndEmbroideryData = (dataMgr as Template.DyeAndEmbroideryModule.Manager).GetTemplateMap();
                else if (name == "EquipConsumeConfig")
                    _EquipConsumeConfigData = (dataMgr as Template.EquipConsumeConfigModule.Manager).GetTemplateMap();
                else if (name == "EquipInforce")
                    _EquipInforceData = (dataMgr as Template.EquipInforceModule.Manager).GetTemplateMap();
                else if (name == "EquipInherit")
                    _EquipInheritData = (dataMgr as Template.EquipInheritModule.Manager).GetTemplateMap();
                else if (name == "EquipRefine")
                    _EquipRefineData = (dataMgr as Template.EquipRefineModule.Manager).GetTemplateMap();
                else if (name == "EquipSurmount")
                    _EquipSurmountData = (dataMgr as Template.EquipSurmountModule.Manager).GetTemplateMap();
                else if (name == "EquipQuench")
                    _EquipQuenchData = (dataMgr as Template.EquipQuenchModule.Manager).GetTemplateMap();
                else if (name == "ExecutionUnit")
                    _ExecutionUnitData = (dataMgr as Template.ExecutionUnitModule.Manager).GetTemplateMap();
                else if (name == "FightProperty")
                    _FightPropertyData = (dataMgr as Template.FightPropertyModule.Manager).GetTemplateMap();
                else if (name == "Fun")
                    _FunData = (dataMgr as Template.FunModule.Manager).GetTemplateMap();
                else if (name == "Horse")
                    _HorseData = (dataMgr as Template.HorseModule.Manager).GetTemplateMap();
                else if (name == "Instance")
                    _InstanceData = (dataMgr as Template.InstanceModule.Manager).GetTemplateMap();
                else if (name == "Item")
                    _ItemData = (dataMgr as Template.ItemModule.Manager).GetTemplateMap();
                else if (name == "MetaFightPropertyConfig")
                    _MetaFightPropertyConfigData = (dataMgr as Template.MetaFightPropertyConfigModule.Manager).GetTemplateMap();
                else if (name == "Mine")
                    _MineData = (dataMgr as Template.MineModule.Manager).GetTemplateMap();
                else if (name == "Money")
                    _MoneyData = (dataMgr as Template.MoneyModule.Manager).GetTemplateMap();
                else if (name == "Monster")
                    _MonsterData = (dataMgr as Template.MonsterModule.Manager).GetTemplateMap();
                else if (name == "MonsterProperty")
                    _MonsterPropertyData = (dataMgr as Template.MonsterPropertyModule.Manager).GetTemplateMap();
                else if (name == "NpcSale")
                    _NpcSaleData = (dataMgr as Template.NpcSaleModule.Manager).GetTemplateMap();
                else if (name == "Quest")
                    _QuestData = (dataMgr as Template.QuestModule.Manager).GetTemplateMap();
                else if (name == "QuestChapter")
                    _QuestChapterData = (dataMgr as Template.QuestChapterModule.Manager).GetTemplateMap();
                else if (name == "Reward")
                    _RewardData = (dataMgr as Template.RewardModule.Manager).GetTemplateMap();
                else if (name == "LegendaryGroup")
                    _LegendaryGroupData = (dataMgr as Template.LegendaryGroupModule.Manager).GetTemplateMap();
                else if (name == "ItemApproach")
                    _ItemApproachData = (dataMgr as Template.ItemApproachModule.Manager).GetTemplateMap();
                else if (name == "ItemMachining")
                    _ItemMachiningData = (dataMgr as Template.ItemMachiningModule.Manager).GetTemplateMap();
                else if (name == "Enchant")
                    _EnchantData = (dataMgr as Template.EnchantModule.Manager).GetTemplateMap();
                else if (name == "Talent")
                    _TalentData = (dataMgr as Template.TalentModule.Manager).GetTemplateMap();
                else if (name == "Pet")
                    _PetData = (dataMgr as Template.PetModule.Manager).GetTemplateMap();
                else if (name == "WingTalentLevel")
                    _WingTalentLevelData = (dataMgr as Template.WingTalentLevelModule.Manager).GetTemplateMap();
                else if (name == "Wing")
                    _WingData = (dataMgr as Template.WingModule.Manager).GetTemplateMap();
                else if (name == "WingTalentPage")
                    _WingTalentPageData = (dataMgr as Template.WingTalentPageModule.Manager).GetTemplateMap();
                else if (name == "CharmItem")
                    _CharmItemData = (dataMgr as Template.CharmItemModule.Manager).GetTemplateMap();
                else if (name == "CharmUpgrade")
                    _CharmUpgradeData = (dataMgr as Template.CharmUpgradeModule.Manager).GetTemplateMap();
                else if (name == "GuildSmithy")
                    _GuildSmithyData = (dataMgr as Template.GuildSmithyModule.Manager).GetTemplateMap();
            }
        }


        void Check_AttachedProperty()
        {
            //AttachedProperty <-> MetaFightPropertyConfig
            foreach (var v in _AttachedPropertyData.Values)
            {
                foreach (var substance in v.Substances)
                {
                    if (substance.AdjustMetaProperty == null)
                        continue;

                    int id = substance.AdjustMetaProperty.Id;
                    if (id > 0 && !_MetaFightPropertyConfigData.ContainsKey(id))
                    {
                        _ErrorString += string.Format("附加属性ID： {0} 中存在错误的基元战斗属性ID： {1}\n", v.Id, id);
                    }
                }
            }
        }

        void Check_AttachedPropertyGenerator()
        {
            //AttachedPropertyGenerator <-> AttachedProperty 
            foreach (var v in _AttachedPropertyGeneratorData.Values)
            {
                if (v.FightPropertyId != 0 && !_AttachedPropertyData.ContainsKey(v.FightPropertyId))
                {
                    _ErrorString += string.Format("装备附加属性生成器ID： {0} 中存在错误的附加属性ID： {1}\n", v.Id, v.FightPropertyId);
                }
            }
        }

        void Check_AttachedPropertyGroupGenerator()
        {
            //AttachedPropertyGroupGenerator <-> AttachedpropertyGenerator
            foreach (var v in _AttachedPropertyGroupGeneratorData.Values)
            {
                if (v.ConfigData != null)
                {
                    foreach (var config in v.ConfigData.AttachedPropertyGeneratorConfigs)
                    {
                        if (config.Id != 0 && !_AttachedPropertyGeneratorData.ContainsKey(config.Id))
                        {
                            _ErrorString += string.Format("装备附加属性生成器组ID： {0} 中存在错误的装备附加属性生成器ID： {1}\n", v.Id, config.Id);
                        }
                    }
                }
            }
        }

        void Check_Reward()
        {
            //Reward <-> Money
            //Reward <-> Item
            //Reward <-> Designation
            foreach (var v in _RewardData.Values)
            {
                if (!_MoneyData.ContainsKey(v.MoneyId1) && v.MoneyId1 != 0)
                    _ErrorString += string.Format("奖励ID： {0} 中存在错误的货币ID： {1}\n", v.Id, v.MoneyId1);
                if (!_MoneyData.ContainsKey(v.MoneyId2) && v.MoneyId2 != 0)
                    _ErrorString += string.Format("奖励ID： {0} 中存在错误的货币ID： {1}\n", v.Id, v.MoneyId2);
                if (!_MoneyData.ContainsKey(v.MoneyId3) && v.MoneyId3 != 0)
                    _ErrorString += string.Format("奖励ID： {0} 中存在错误的货币ID： {1}\n", v.Id, v.MoneyId3);
                if (!_MoneyData.ContainsKey(v.MoneyId4) && v.MoneyId4 != 0)
                    _ErrorString += string.Format("奖励ID： {0} 中存在错误的货币ID： {1}\n", v.Id, v.MoneyId4);
                if (v.ItemRelated != null)
                {
                    foreach (var item in v.ItemRelated.RewardItems)
                    {
                        if (!_ItemData.ContainsKey(item.Id) && item.Id != 0)
                            _ErrorString += string.Format("奖励ID： {0} 中存在错误的道具ID： {1}\n", v.Id, item.Id);
                    }
                }
                if (v.WarriorRelated != null)
                {
                    foreach (var item in v.WarriorRelated.RewardItems)
                    {
                        if (!_ItemData.ContainsKey(item.Id) && item.Id != 0)
                            _ErrorString += string.Format("奖励ID： {0} 中存在错误的道具ID： {1}\n", v.Id, item.Id);
                    }
                }
                if (v.ArcherRelated != null)
                {
                    foreach (var item in v.ArcherRelated.RewardItems)
                    {
                        if (!_ItemData.ContainsKey(item.Id) && item.Id != 0)
                            _ErrorString += string.Format("奖励ID： {0} 中存在错误的道具ID： {1}\n", v.Id, item.Id);
                    }
                }
                if (v.AssassinRelated != null)
                {
                    foreach (var item in v.AssassinRelated.RewardItems)
                    {
                        if (!_ItemData.ContainsKey(item.Id) && item.Id != 0)
                            _ErrorString += string.Format("奖励ID： {0} 中存在错误的道具ID： {1}\n", v.Id, item.Id);
                    }
                }
                if (v.PreistRelated != null)
                {
                    foreach (var item in v.PreistRelated.RewardItems)
                    {
                        if (!_ItemData.ContainsKey(item.Id) && item.Id != 0)
                            _ErrorString += string.Format("奖励ID： {0} 中存在错误的道具ID： {1}\n", v.Id, item.Id);
                    }
                }

                if (v.DesignationId > 0)
                {
                    if (!_DesignationData.ContainsKey(v.DesignationId))
                        _ErrorString += string.Format("奖励ID： {0} 中存在错误的称号ID： {1}\n", v.Id, v.DesignationId);
                }
            }
        }

        void Check_DropRule()
        {
            //DropRule <-> Money
            //DropRule <-> Item
            //DropRule <-> DropLibrary
            foreach (var v in _DropRuleData.Values)
            {
                if (!_MoneyData.ContainsKey(v.CostMoneyId) && v.CostMoneyId != 0)
                    _ErrorString += string.Format("彩票规则ID： {0} 中存在错误的货币ID： {1}\n", v.Id, v.CostMoneyId);
                if (!_ItemData.ContainsKey(v.CostItemId1) && v.CostItemId1 != 0)
                    _ErrorString += string.Format("彩票规则ID： {0} 中存在错误的道具ID： {1}\n", v.Id, v.CostItemId1);
                if (!_ItemData.ContainsKey(v.CostItemId2) && v.CostItemId2 != 0)
                    _ErrorString += string.Format("彩票规则ID： {0} 中存在错误的道具ID： {1}\n", v.Id, v.CostItemId2);
                if (!_RewardData.ContainsKey(v.DescRewardid) && v.DescRewardid != 0)
                    _ErrorString += string.Format("彩票规则ID： {0} 中存在错误的奖励ID： {1}\n", v.Id, v.DescRewardid);
                if (!_DropLibraryData.ContainsKey(v.GiftLibId) && v.GiftLibId != 0)
                    _ErrorString += string.Format("彩票规则ID： {0} 中存在错误的赠送掉落ID： {1}\n", v.Id, v.GiftLibId);
                for (int i = 0; i < v.DropLibWeights.Count; i++)
                {
                    if (!_DropLibraryData.ContainsKey(v.DropLibWeights[i].DropLibId) && v.DropLibWeights[i].DropLibId != 0)
                        _ErrorString += string.Format("彩票规则ID： {0} 中存在错误的掉落ID： {1}\n", v.Id, v.DropLibWeights[i].DropLibId);
                }
            }
        }

        void Check_EquipConsumeConfig()
        {
            //EquipConsumeConfig <-> Money
            //EuqipConsumeConfig <-> Item
            foreach (var v in _EquipConsumeConfigData.Values)
            {
                if (v.Money != null)
                {
                    foreach (var pair in v.Money.ConsumePairs)
                    {
                        int id = pair.ConsumeId;
                        if (!_MoneyData.ContainsKey(id) && id != 0)
                            _ErrorString += string.Format("装备重铸ID： {0} 中存在错误的货币ID： {1}\n", v.Id, id);
                    }
                }
                if (v.Item != null)
                {
                    foreach (var pair in v.Item.ConsumePairs)
                    {
                        int id = pair.ConsumeId;
                        if (!_ItemData.ContainsKey(id) && id != 0)
                            _ErrorString += string.Format("装备重铸ID： {0} 中存在错误的道具ID： {1}\n", v.Id, id);
                    }
                }
            }
        }

        void Check_EquipInforce()
        {
            //EquipInforce <-> Money
            //EquipInforce <-> Item
            foreach (var v in _EquipInforceData.Values)
            {
                foreach (var data in v.InforceDatas)
                {
                    if (data.CostMoneyId != 0 && !_MoneyData.ContainsKey(data.CostMoneyId))
                        _ErrorString += string.Format("装备强化ID： {0} 中存在错误的货币ID： {1}\n", v.Id, data.CostMoneyId);
                    if (data.RefundItemId != 0 && !_ItemData.ContainsKey(data.RefundItemId))
                        _ErrorString += string.Format("装备强化ID： {0} 中存在错误的物品ID： {1}\n", v.Id, data.RefundItemId);
                }
            }
        }

        void Check_EquipQuench()
        {
            //EquipQuench <-> Money
            //EquipQuench <-> Item
            foreach (var v in _EquipQuenchData.Values)
            {
                foreach (var quenchData in v.QuenchDatas)
                {
                    if (!_MoneyData.ContainsKey(quenchData.CostMoneyId) && quenchData.CostMoneyId != 0)
                        _ErrorString += string.Format("装备淬火ID： {0} 中存在错误的货币ID： {1}\n", v.Id, quenchData.CostMoneyId);
                    if (!_ItemData.ContainsKey(quenchData.CostItemId) && quenchData.CostItemId != 0)
                        _ErrorString += string.Format("装备淬火ID： {0} 中存在错误的道具ID： {1}\n", v.Id, quenchData.CostItemId);
                }
            }
        }

        void Check_EquipRefine()
        {
            //EquipRefine <-> Money
            //EquipRefine <-> Item
            foreach (var v in _EquipRefineData.Values)
            {
                foreach (var refineData in v.RefineDatas)
                {
                    if (!_MoneyData.ContainsKey(refineData.CostMoneyId) && refineData.CostMoneyId != 0)
                        _ErrorString += string.Format("装备精炼ID： {0} 中存在错误的货币ID： {1}\n", v.Id, refineData.CostMoneyId);
                    if (!_ItemData.ContainsKey(refineData.CostItemId) && refineData.CostItemId != 0)
                        _ErrorString += string.Format("装备精炼ID： {0} 中存在错误的道具ID： {1}\n", v.Id, refineData.CostItemId);
                }
            }
        }

        void Check_EquipSurmount()
        {
            //EquipSurmount <-> Money
            //EquipSurmount <-> Item
            foreach (var v in _EquipSurmountData.Values)
            {
                foreach (var data in v.SurmountDatas)
                {
                    if (!_MoneyData.ContainsKey(data.CostMoneyId) && data.CostMoneyId != 0)
                        _ErrorString += string.Format("装备突破ID： {0} 中存在错误的货币ID： {1}\n", v.Id, data.CostMoneyId);
                    if (!_ItemData.ContainsKey(data.CostItemId) && data.CostItemId != 0)
                        _ErrorString += string.Format("装备突破ID： {0} 中存在错误的道具ID： {1}\n", v.Id, data.CostItemId);
                }
            }
        }

        void Check_Instance()
        {
            //Instance <-> Reward
            //Instance <-> DropLibrary
            //Instance <-> Item
            foreach (var v in _InstanceData.Values)
            {

                if (!_RewardData.ContainsKey(v.RewardId) && v.RewardId != 0)
                    _ErrorString += string.Format("地牢ID： {0} 中存在错误的奖励ID： {1}\n", v.Id, v.RewardId);
                if (!_RewardData.ContainsKey(v.LeaderRewardId) && v.LeaderRewardId != 0)
                    _ErrorString += string.Format("地牢ID： {0} 中存在错误的Leader奖励ID： {1}\n", v.Id, v.LeaderRewardId);
                if (!_DropLibraryData.ContainsKey(v.PassDungeonDropLibraryId) && v.PassDungeonDropLibraryId != 0)
                    _ErrorString += string.Format("地牢ID： {0} 中存在错误的通关奖励掉落ID： {1}\n", v.Id, v.PassDungeonDropLibraryId);
                if (!_DropLibraryData.ContainsKey(v.FailedDungeonDropLibraryId) && v.FailedDungeonDropLibraryId != 0)
                    _ErrorString += string.Format("地牢ID： {0} 中存在错误的失败结算掉落ID： {1}\n", v.Id, v.FailedDungeonDropLibraryId);
                if (!_ItemData.ContainsKey(v.ConsumeItemId) && v.ConsumeItemId != 0)
                    _ErrorString += string.Format("地牢ID： {0} 中存在错误的消耗道具ID： {1}\n", v.Id, v.ConsumeItemId);
            }
        }

        void Check_Item()
        {
            //Item <-> EquipInforce
            //Item <-> EquipRefine
            //Item <-> EquipQuench
            //Item <-> EquipSurmount
            //Item <-> EquipConsumeConfig
            //Item <-> AttachedPropertyGenerator
            //Item <-> AttachedPropertyGroupGenerator
            //Item <-> ItemApproach
            //Item <-> LegendaryGroup
            //Item <-> ItemMachining
            //Item <-> Enchant
            foreach (var v in _ItemData.Values)
            {
                if (!_EquipInforceData.ContainsKey(v.ReinforceConfigId) && v.ReinforceConfigId != 0)
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的装备强化ID： {1}\n", v.Id, v.ReinforceConfigId);
                if (!_EquipRefineData.ContainsKey(v.EquipRefineTId) && v.EquipRefineTId != 0)
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的装备精炼ID： {1}\n", v.Id, v.EquipRefineTId);
                if (!_EquipQuenchData.ContainsKey(v.QuenchTid) && v.QuenchTid != 0)
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的装备淬火ID： {1}\n", v.Id, v.QuenchTid);
                if (!_EquipSurmountData.ContainsKey(v.SurmountTid) && v.SurmountTid != 0)
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的装备突破ID： {1}\n", v.Id, v.SurmountTid);
                if (!_EquipConsumeConfigData.ContainsKey(v.RecastCostId) && v.RecastCostId != 0)
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的装备重铸ID： {1}\n", v.Id, v.RecastCostId);
                if (!_AttachedPropertyGeneratorData.ContainsKey(v.AttachedPropertyGeneratorId) && v.AttachedPropertyGeneratorId != 0)
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的装备附加属性ID： {1}\n", v.Id, v.AttachedPropertyGeneratorId);
                if (!_AttachedPropertyGroupGeneratorData.ContainsKey(v.AttachedPropertyGroupGeneratorId) && v.AttachedPropertyGroupGeneratorId != 0)
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的装备附加属性组ID： {1}\n", v.Id, v.AttachedPropertyGroupGeneratorId);

                if (!string.IsNullOrEmpty(v.ApproachID))
                {
                    string[] ret = v.ApproachID.Split('*');
                    foreach (string s in ret)
                    {
                        int i;
                        if (!int.TryParse(s, out i))
                        {
                            _ErrorString += string.Format("Item ApproachID格式错误 Id: {0}, ApproachID: {1}\n", v.Id, v.ApproachID);
                            continue;
                        }
                        
                        if (i != 0 && !_ItemApproachData.ContainsKey(i))
                        {
                            _ErrorString += string.Format("道具ID： {0} 中存在错误的物品获取来源ID： {1}\n", v.Id, i);
                        }
                    }
                }

                if (v.LegendaryGroupId != 0 && !_LegendaryGroupData.ContainsKey(v.LegendaryGroupId))
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的传奇属性组ID： {1}\n", v.Id, v.LegendaryGroupId);
                if (v.DecomposeId != 0 && !_ItemMachiningData.ContainsKey(v.DecomposeId))
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的物品分解ID： {1}\n", v.Id, v.DecomposeId);
                if (v.ComposeId != 0 && !_ItemMachiningData.ContainsKey(v.ComposeId))
                    _ErrorString += string.Format("道具ID： {0} 中存在错误的物品合成ID： {1}\n", v.Id, v.ComposeId);

                if (v.EventType1 == ItemEvent_EquipEnchant && !string.IsNullOrEmpty(v.Type1Param1))
                {
                    string[] ret = v.Type1Param1.Split('*');
                    foreach (string s in ret)
                    {
                        int i;
                        if (!int.TryParse(s, out i))
                        {
                            _ErrorString += string.Format("Item Type1Param1格式错误 Id:{0}, Type1Param1: {1}", v.Id, v.Type1Param1);
                            continue;
                        }

                        if (i != 0 && !_EnchantData.ContainsKey(i))
                            _ErrorString += string.Format("道具ID： {0} 中存在错误的附魔卷轴事件参数1ID： {1}\n", v.Id, i);
                    }
                }
            }
        }

        void Check_Mine()
        {
            //Mine <-> Item
            //Mine <-> DropLibrary
            foreach (var v in _MineData.Values)
            {
                if (!_ItemData.ContainsKey(v.SuccessItemId) && v.SuccessItemId != 0)
                    _ErrorString += string.Format("可采集单位ID： {0} 中存在错误的道具ID： {1}\n", v.Id, v.SuccessItemId);
                if (!_DropLibraryData.ContainsKey(v.SuccessDropId) && v.SuccessDropId != 0)
                    _ErrorString += string.Format("可采集单位ID： {0} 中存在错误的掉落ID： {1}\n", v.Id, v.SuccessDropId);
            }
        }

        void Check_Quest()
        {
            //Quest <-> Reward
            //Quest <-> Item
            foreach (var v in _QuestData.Values)
            {
                if (v.RewardId != 0 && !_RewardData.ContainsKey(v.RewardId))
                    _ErrorString += string.Format("任务ID： {0} 中存在错误的奖励ID： {1}\n", v.Id, v.RewardId);

                if (v.ObjectiveRelated != null)
                {
                    foreach (var objective in v.ObjectiveRelated.QuestObjectives)
                    {
                        if (objective.UseItem != null)
                        {
                            if (!_ItemData.ContainsKey(objective.UseItem.ItemTId) &&
                                objective.UseItem.ItemTId != 0)
                                _ErrorString += string.Format("任务ID： {0} 中存在错误的目标使用道具ID： {1}\n", v.Id, objective.UseItem.ItemTId);
                        }
                        if (objective.HoldItem != null)
                        {
                            if (!_ItemData.ContainsKey(objective.HoldItem.ItemTId) &&
                                objective.HoldItem.ItemTId != 0)
                                _ErrorString += string.Format("任务ID： {0} 中存在错误的目标持有道具ID： {1}\n", v.Id, objective.HoldItem.ItemTId);
                        }
                    }
                }
                if (v.EventRelated != null)
                {
                    foreach (var ev in v.EventRelated.QuestEvents)
                    {
                        if (ev.DelItem != null)
                        {
                            if (!_ItemData.ContainsKey(ev.DelItem.ItemTID) &&
                                ev.DelItem.ItemTID != 0)
                                _ErrorString += string.Format("任务ID： {0} 中存在错误的事件删除道具ID： {1}\n", v.Id, ev.DelItem.ItemTID);
                        }
                        if (ev.GetItem != null)
                        {
                            if (!_ItemData.ContainsKey(ev.GetItem.ItemTID) &&
                                ev.GetItem.ItemTID != 0)
                                _ErrorString += string.Format("任务ID： {0} 中存在错误的事件获得道具ID： {1}\n", v.Id, ev.GetItem.ItemTID);
                        }
                    }
                }
            }
        }

        void Check_QuestChapter()
        {
            foreach (var v in _QuestChapterData.Values)
            {
                string[] ret = v.QuestGroupId.Split('*');
                if (ret.Length > 4)
                {
                    _ErrorString += string.Format("任务章节ID： {0} 中任务组ID不能超过4个： {1}\n", v.Id, v.QuestGroupId);
                }
            }
        }
        void Check_Monster()
        {
            //Monster <-> MonsterProperty
            //Monster <-> DropLibrary
            foreach (var v in _MonsterData.Values)
            {
                int propertyTid = v.MonsterPropertyId * 10000 + 1;
                if (!_MonsterPropertyData.ContainsKey(propertyTid))
                    _ErrorString += string.Format("怪物ID： {0} 中存在错误的怪物属性ID： {1}\n", v.Id, v.MonsterPropertyId);
                if (v.DropLibData != null)
                {
                    for (int i = 0; i < v.DropLibData.DropLibs.Count; i++)
                    {
                        if (!_DropLibraryData.ContainsKey(v.DropLibData.DropLibs[i].DropLibId))
                            _ErrorString += string.Format("怪物ID： {0} 中存在错误的掉落ID： {1}\n", v.Id, v.DropLibData.DropLibs[i].DropLibId);
                    }
                }
                if (!_DropLibraryData.ContainsKey(v.UnderAttackDropId) && v.UnderAttackDropId != 0)
                    _ErrorString += string.Format("怪物ID： {0} 中存在错误的掉落ID： {1}\n", v.Id, v.UnderAttackDropId);
            }
        }

        void Check_NpcSale()
        {
            //NpcSale <-> Item
            foreach (var v in _NpcSaleData.Values)
            {
                var saleList = v.NpcSaleSubs;
                foreach (var sale in saleList)
                {
                    foreach (var item in sale.NpcSaleItems)
                    {
                        int itemid = item.ItemId;
                        int moneyid = item.CostMoneyId;

                        if (!_ItemData.ContainsKey(itemid) && itemid != 0)
                            _ErrorString += string.Format("Npc售卖ID： {0} 中存在错误的道具ID： {1}\n", v.Id, itemid);

                        if (!_MoneyData.ContainsKey(moneyid) && moneyid != 0)
                            _ErrorString += string.Format("Npc售卖ID： {0} 中存在错误的货币ID： {1}\n", v.Id, moneyid);
                    }
                }
            }
        }

        void Check_Achievement()
        {
            //Achievement <-> Reward
            foreach (var v in _AchievementData.Values)
            {
                if (v.RewardId > 0)
                {
                    if (!_RewardData.ContainsKey(v.RewardId))
                        _ErrorString += string.Format("成就ID： {0} 中存在错误的奖励ID： {1}\n", v.Id, v.RewardId);
                }
            }

            foreach (var v1 in _AchievementData.Values)
            {
                foreach (var v2 in _AchievementData.Values)
                {
                    if (v1.Id == v2.Id)
                        continue;

                    if (v1.TypeID == v2.TypeID && v1.TypeName != v2.TypeName)
                    {
                        _ErrorString += string.Format("成就中存在冲突的分类, TypeID: {0} 模板Id: {1}, 名称: {2}, 冲突模板Id: {3}, 名称: {4}\n", v1.TypeID, v1.Id, v1.TypeName, v2.Id, v2.TypeName);
                    }

                    if (v1.TypeID == v2.TypeID)
                    {
                        if (v1.TypeID2 == v2.TypeID2 && v1.TypeName2 != v2.TypeName2)
                        {
                            _ErrorString += string.Format("成就中存在冲突的小分类2, TypeID: {0} 模板Id: {1}, TypeID2: {2}, 名称: {3}, 冲突模板Id: {4}, TypeID2: {5}, 名称: {6}\n", v1.TypeID, v1.Id, v1.TypeID2, v1.TypeName2, v2.Id, v2.TypeID2, v2.TypeName2);
                        }
                    }
                }
            }

            foreach (var v in _AchievementData.Values)
            {
                string[] ret = v.ReachParm.Split('*');
                foreach (string s in ret)
                {
                    int i;
                    if (!int.TryParse(s, out i))
                    {
                        _ErrorString += string.Format("Achievement ReachParm格式错误 Id:{0}, ReachParm: {1}", v.Id, v.ReachParm);
                        break;
                    }
                }
            }
        }

        void Check_LegendaryGroup()
        {
            //LegendaryGroup <-> Talent
            foreach (var v in _LegendaryGroupData.Values)
            {
                foreach(var pair in v.Legendarys.LegendaryPairs)
                {
                    if (pair.TalentID != 0 && !_TalentData.ContainsKey(pair.TalentID))
                        _ErrorString += string.Format("传奇属性组ID： {0} 中存在错误的天赋ID： {1}\n", v.Id, pair.TalentID);
                }
            }
        }

        void Check_Pet()
        {
            //Pet <-> LegendaryGroup
            //Pet <-> ItemApproach
            //Pet <-> AttachedPropertyGeneratorIds
            //Pet <-> AttachedPropertyGroupGeneratorIds
            foreach (var v in _PetData.Values)
            {
                if (v.TelantSkillGroupId != 0 && !_LegendaryGroupData.ContainsKey(v.TelantSkillGroupId))
                    _ErrorString += string.Format("宠物ID： {0} 中存在错误的传奇属性组ID： {1}\n", v.Id, v.TelantSkillGroupId);

                if (!string.IsNullOrEmpty(v.ApproachIDs))
                {
                    string[] ret = v.ApproachIDs.Split('*');
                    foreach (string s in ret)
                    {
                        int i;
                        if (!int.TryParse(s, out i))
                        {
                            _ErrorString += string.Format("宠物的ApproachID格式错误 Id: {0}, ApproachIDs: {1}\n", v.Id, v.ApproachIDs);
                            continue;
                        }

                        if (i != 0 && !_ItemApproachData.ContainsKey(i))
                        {
                            _ErrorString += string.Format("宠物ID： {0} 中存在错误的物品获取来源ID： {1}\n", v.Id, i);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(v.AttachedPropertyGeneratorIds))
                {
                    string[] ret = v.AttachedPropertyGeneratorIds.Split('*');
                    if (ret.Length != 5)
                    {
                        _ErrorString += string.Format("宠物ID： {0} 中附加属性生成器的个数不为5\n", v.Id);
                    }
                    else
                    {
                        foreach (string s in ret)
                        {
                            int i;
                            if (!int.TryParse(s, out i))
                            {
                                _ErrorString += string.Format("宠物的AttachedPropertyGeneratorIds格式错误 Id: {0}, AttachedPropertyGeneratorIds: {1}\n", v.Id, v.AttachedPropertyGeneratorIds);
                                continue;
                            }

                            if (i != 0 && !_AttachedPropertyGeneratorData.ContainsKey(i))
                            {
                                _ErrorString += string.Format("宠物ID： {0} 中存在错误的附加属性生成器ID： {1}\n", v.Id, i);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(v.AttachedPropertyGroupGeneratorIds))
                {
                    string[] ret = v.AttachedPropertyGroupGeneratorIds.Split('*');
                    foreach (string s in ret)
                    {
                        int i;
                        if (!int.TryParse(s, out i))
                        {
                            _ErrorString += string.Format("宠物的AttachedPropertyGroupGeneratorIds格式错误 Id: {0}, AttachedPropertyGroupGeneratorIds: {1}\n", v.Id, v.AttachedPropertyGroupGeneratorIds);
                            continue;
                        }

                        if (i != 0 && !_AttachedPropertyGroupGeneratorData.ContainsKey(i))
                        {
                            _ErrorString += string.Format("宠物ID： {0} 中存在错误的附加属性生成器组ID： {1}\n", v.Id, i);
                        }
                    }
                }

            }
        }

        void Check_WingTalentLevel()
        {
            //WingTalentLevel <-> Talent
            foreach (var v in _WingTalentLevelData.Values)
            {
                if (v.TalentID != 0 && !_TalentData.ContainsKey(v.TalentID))
                    _ErrorString += string.Format("飞翼天赋等级ID： {0} 中存在错误的天赋ID： {1}\n", v.Id, v.TalentID);
            }
        }

        void Check_ItemMachining()
        {
            //ItemMachining <-> Money
            //ItemMachining <-> Item
            foreach (var v in _ItemMachiningData.Values)
            {
                if (v.MoneyId != 0 && !_MoneyData.ContainsKey(v.MoneyId))
                    _ErrorString += string.Format("物品加工ID: {0} 中存在错误的货币ID: {1}\n", v.Id, v.MoneyId);

                if (v.SrcItemData != null)
                {
                    foreach(var item in v.SrcItemData.SrcItems)
                    {
                        if (item.ItemId != 0 && !_ItemData.ContainsKey(item.ItemId))
                            _ErrorString += string.Format("物品加工ID: {0} 中存在错误的SrcItems物品ID: {1}\n", v.Id, item.ItemId);
                    }
                }

                if (v.DestItemData != null)
                {
                    foreach(var item in v.DestItemData.DestItems)
                    {
                        if (item.DestType == (int)ItemMachining.EDestType.Money)
                        {
                            if (item.ItemId != 0 && !_MoneyData.ContainsKey(item.ItemId))
                                _ErrorString += string.Format("物品加工ID: {0} 中存在错误的DestItems货币ID: {1}\n", v.Id, item.ItemId);
                        }
                        else if (item.DestType == (int)ItemMachining.EDestType.Item)
                        {
                            if (item.ItemId != 0 && !_ItemData.ContainsKey(item.ItemId))
                                _ErrorString += string.Format("物品加工ID: {0} 中存在错误的DestItems物品ID: {1}\n", v.Id, item.ItemId);
                        }
                    }
                }
            }
        }

        void Check_Wing()
        {
            //Wing <-> Approach
            //Wing <-> WingTalentPage
            foreach (var v in _WingData.Values)
            {
                if (v.ItemID != 0 && !_ItemData.ContainsKey(v.ItemID))
                    _ErrorString += string.Format("飞翼ID： {0} 中存在错误的物品 ID： {1}\n", v.Id, v.ItemID);
                if (v.WingTalentPage1 != 0 && !_WingTalentPageData.ContainsKey(v.WingTalentPage1))
                    _ErrorString += string.Format("飞翼ID： {0} 中存在错误的飞翼天赋页1 ID： {1}\n", v.Id, v.WingTalentPage1);
                if (v.WingTalentPage2 != 0 && !_WingTalentPageData.ContainsKey(v.WingTalentPage2))
                    _ErrorString += string.Format("飞翼ID： {0} 中存在错误的飞翼天赋页2 ID： {1}\n", v.Id, v.WingTalentPage2);
                if (v.WingTalentPage3 != 0 && !_WingTalentPageData.ContainsKey(v.WingTalentPage3))
                    _ErrorString += string.Format("飞翼ID： {0} 中存在错误的飞翼天赋页3 ID： {1}\n", v.Id, v.WingTalentPage3);

            }
        }

        void Check_CharmItem()
        {
            //CharmItem <-> CharmUpgrade
            foreach (var v in _CharmItemData.Values)
            {
                if (v.UpgradeTargetId != 0 && !_CharmUpgradeData.ContainsKey(v.UpgradeTargetId))
                    _ErrorString += string.Format("神符道具ID： {0} 中存在错误的神符升级ID： {1}\n", v.Id, v.UpgradeTargetId);

                if (v.TargetCharmId != 0 && !_CharmItemData.ContainsKey(v.TargetCharmId))
                    _ErrorString += string.Format("神符道具ID： {0} 中存在错误的神符升级目标神符ID： {1}\n", v.Id, v.TargetCharmId);
            }
        }

        void Check_CharmUpgrade()
        {
            //CharmUpgrade <-> Item
            foreach (var v in _CharmUpgradeData.Values)
            {
                if (v.CostItemTId != 0 && !_ItemData.ContainsKey(v.CostItemTId))
                    _ErrorString += string.Format("神符升级ID： {0} 中存在错误的消耗道具ID： {1}\n", v.Id, v.CostItemTId);
            }
        }

        void Check_GuildSmithy()
        {
            //GuildSmithy <-> ItemMachining
            foreach (var v in _GuildSmithyData.Values)
            {
                foreach (var item in v.MachiningItems)
                {
                    if (item.MachiningID != 0 && !_ItemMachiningData.ContainsKey(item.MachiningID))
                        _ErrorString += string.Format("工会铁匠铺ID： {0} 中存在错误的物品加工ID： {1}\n", v.Id, item.MachiningID);
                }
            }
        }

        private void CheckData()
        {
            _ErrorString = string.Empty;

            Check_AttachedProperty();
            Check_AttachedPropertyGenerator();
            Check_AttachedPropertyGroupGenerator();
            Check_Reward();
            Check_DropRule();
            Check_EquipConsumeConfig();
            Check_EquipInforce();
            Check_EquipQuench();
            Check_EquipRefine();
            Check_EquipSurmount();
            Check_Item();
            Check_Quest();
            Check_QuestChapter();
            Check_NpcSale();
            Check_Achievement();
            Check_LegendaryGroup();
            Check_Pet();
            Check_WingTalentLevel();
            Check_ItemMachining();
            Check_Wing();
            Check_CharmItem();
            Check_CharmUpgrade();
            Check_GuildSmithy();

            LogReport("模板关联数据有效性检查.txt");
        }

        private string _ErrorString = "";
        private void LogReport(string filename)
        {
            //写log 
            string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            string logFile = System.IO.Path.Combine(logDir, filename);
            TextLogger.Path = logFile;
            File.Delete(logFile);

            if (_ErrorString.Length == 0)
            {
                //EditorUtility.DisplayDialog("完成", "检查通过!", "确定");
                TextLogger.Instance.WriteLine("模板关联数据有效性检查!");
            }
            else
            {
                //EditorUtility.DisplayDialog("错误", _ErrorString, "确定");
                TextLogger.Instance.WriteLine(_ErrorString);
            }
        }

    }

}
