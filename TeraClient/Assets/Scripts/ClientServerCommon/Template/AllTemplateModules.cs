using Common;
using System.Collections.Generic;

namespace Template.ActivityStoreModule
{
    public class Manager : BaseManager<Manager, ActivityStore>
    {
        public Manager()
        {
            LoadFromBin("ActivityStore.data");
        }
    };
}


namespace Template.AchievementModule
{
    public class Manager : BaseManager<Manager, Achievement>
    {
        public Manager()
        {
            LoadFromBin("Achievement.data");
        }
    };
}

namespace Template.ActorModule
{
    public class Manager : BaseManager<Manager, Actor>
    {
        public Manager()
        {
            LoadFromBin("Actor0.data", false);
            LoadFromBin("Actor1.data", false);
        }

    };
}

namespace Template.AdventureGuideModule
{
    public class Manager : BaseManager<Manager, AdventureGuide>
    {
        public Manager()
        {
            LoadFromBin("AdventureGuide.data");
        }
    };
}

namespace Template.AssetModule
{
    public class Manager : BaseManager<Manager, Asset>
    {
        public Manager()
        {
            LoadFromBin("Asset.data");
        }
    };
}

namespace Template.AttachedPropertyGeneratorModule
{
    public class Manager : BaseManager<Manager, AttachedPropertyGenerator>
    {
        public Manager()
        {
            LoadFromBin("AttachedPropertyGenerator.data");
        }
    };
}

namespace Template.AttachedPropertyGroupGeneratorModule
{
    public class Manager : BaseManager<Manager, AttachedPropertyGroupGenerator>
    {
        public Manager()
        {
            LoadFromBin("AttachedPropertyGroupGenerator.data");
        }
    };
}

namespace Template.AttachedPropertyModule
{
    public class Manager : BaseManager<Manager, AttachedProperty>
    {
        public Manager()
        {
            LoadFromBin("AttachedProperty.data");
        }
    };
}

namespace Template.BannerModule
{
    public class Manager : BaseManager<Manager, Banner>
    {
        public Manager()
        {
            LoadFromBin("Banner.data");
        }
    }
}

namespace Template.CharmFieldModule
{
    public class Manager : BaseManager<Manager, CharmField>
    {
        public Manager()
        {
            LoadFromBin("CharmField.data");
        }
    };
}

namespace Template.CharmItemModule
{
    public class Manager : BaseManager<Manager, CharmItem>
    {
        public Manager()
        {
            LoadFromBin("CharmItem.data");
        }
    };
}

namespace Template.CharmPageModule
{
    public class Manager : BaseManager<Manager, CharmPage>
    {
        public Manager()
        {
            LoadFromBin("CharmPage.data");
        }
    };
}

namespace Template.CharmUpgradeModule
{
    public class Manager : BaseManager<Manager, CharmUpgrade>
    {
        public Manager()
        {
            LoadFromBin("CharmUpgrade.data");
        }
    };
}

namespace Template.CountGroupModule
{
    public class Manager : BaseManager<Manager, CountGroup>
    {
        public Manager()
        {
            LoadFromBin("CountGroup.data");
        }
    };
}

namespace Template.CyclicQuestModule
{
    public class Manager : BaseManager<Manager, CyclicQuest>
    {
        public Manager()
        {
            LoadFromBin("CyclicQuest.data");
        }
    };
}

namespace Template.CyclicQuestRewardModule
{
    public class Manager : BaseManager<Manager, CyclicQuestReward>
    {
        public Manager()
        {
            LoadFromBin("CyclicQuestReward.data");
        }
    };
}

namespace Template.DesignationModule
{
    public class Manager : BaseManager<Manager, Designation>
    {
        public Manager()
        {
            LoadFromBin("Designation.data");
        }
    };
}

namespace Template.DialogueModule
{
    public class Manager : BaseManager<Manager, Dialogue>
    {
        public Manager()
        {
            LoadFromBin("Dialogue.data");
        }
    };
}

namespace Template.DressModule
{
    public class Manager : BaseManager<Manager, Dress>
    {
        public Manager()
        {
            LoadFromBin("Dress.data");
        }
    };
}

namespace Template.DressScoreModule
{
    public class Manager : BaseManager<Manager, DressScore>
    {
        public Manager()
        {
            LoadFromBin("DressScore.data");
        }
    };
}

namespace Template.DropLibraryModule
{
    public class Manager : BaseManager<Manager, DropLibrary>
    {
        public Manager()
        {
            LoadFromBin("DropLibrary.data");
        }
    };
}

namespace Template.DropLimitModule
{
    public class Manager : BaseManager<Manager, DropLimit>
    {
        public Manager()
        {
            LoadFromBin("DropLimit.data");
        }
    };
}

namespace Template.DropRuleModule
{
    public class Manager : BaseManager<Manager, DropRule>
    {
        public Manager()
        {
            LoadFromBin("DropRule.data");
        }
    };
}

namespace Template.DyeAndEmbroideryModule
{
    public class Manager : BaseManager<Manager, DyeAndEmbroidery>
    {
        public Manager()
        {
            LoadFromBin("DyeAndEmbroidery.data");
        }
    };
}

namespace Template.EliminateRewardModule
{
    public class Manager : BaseManager<Manager, EliminateReward>
    {
        public Manager()
        {
            LoadFromBin("EliminateReward.data");
        }
    };
}

namespace Template.EmailModule
{
    public class Manager : BaseManager<Manager, Email>
    {
        public Manager()
        {
            LoadFromBin("Email.data");
        }
    };
}

namespace Template.EnchantModule
{
    public class Manager : BaseManager<Manager, Enchant>
    {
        public Manager()
        {
            LoadFromBin("Enchant.data");
        }
    };
}

namespace Template.EquipConsumeConfigModule
{
    public class Manager : BaseManager<Manager, EquipConsumeConfig>
    {
        public Manager()
        {
            LoadFromBin("EquipConsumeConfig.data");
        }
    };
}

namespace Template.EquipInforceModule
{
    public class Manager : BaseManager<Manager, EquipInforce>
    {
        public Manager()
        {
            LoadFromBin("EquipInforce.data");
        }
    };
}

namespace Template.EquipInheritModule
{
    public class Manager : BaseManager<Manager, EquipInherit>
    {
        public Manager()
        {
            LoadFromBin("EquipInherit.data");
        }
    };
}

namespace Template.EquipQuenchModule
{
    public class Manager : BaseManager<Manager, EquipQuench>
    {
        public Manager()
        {
            LoadFromBin("EquipQuench.data");
        }
    };
}

namespace Template.EquipRefineModule
{
    public class Manager : BaseManager<Manager, EquipRefine>
    {
        public Manager()
        {
            LoadFromBin("EquipRefine.data");
        }
    };
}

namespace Template.EquipSuitModule
{
    public class Manager : BaseManager<Manager, EquipSuit>
    {
        public Manager()
        {
            LoadFromBin("EquipSuit.data");
        }
    };
}

namespace Template.EquipSurmountModule
{
    public class Manager : BaseManager<Manager, EquipSurmount>
    {
        public Manager()
        {
            LoadFromBin("EquipSurmount.data");
        }
    };
}

namespace Template.ExecutionUnitModule
{
    public class Manager : BaseManager<Manager, ExecutionUnit>
    {
        public Manager()
        {
            LoadFromBin("ExecutionUnit.data");
        }
    };
}

namespace Template.ExpeditionModule
{
    public class Manager : BaseManager<Manager, Expedition>
    {
        public Manager()
        {
            LoadFromBin("Expedition.data");
        }
    };
}

namespace Template.FactionModule
{
    public class Manager : BaseManager<Manager, Faction>
    {
        public Manager()
        {
            LoadFromBin("Faction.data");
        }
    };
}

namespace Template.FactionRelationshipModule
{
    public class Manager : BaseManager<Manager, FactionRelationship>
    {
        public Manager()
        {
            LoadFromBin("FactionRelationship.data");
        }
    };
}

namespace Template.FightPropertyConfigModule
{
    public class Manager : BaseManager<Manager, FightPropertyConfig>
    {
        public Manager()
        {
            LoadFromBin("FightPropertyConfig.data");
        }
    };
}

namespace Template.FightPropertyModule
{
    public class Manager : BaseManager<Manager, FightProperty>
    {
        public Manager()
        {
            LoadFromBin("FightProperty.data");
        }
    };
}

namespace Template.FortressModule
{
    public class Manager : BaseManager<Manager, Fortress>
    {
        public Manager()
        {
            LoadFromBin("Fortress.data");
        }
    };
}

namespace Template.FundModule
{
    public class Manager : BaseManager<Manager, Fund>
    {
        public Manager()
        {
            LoadFromBin("Fund.data");
        }
    };
}

namespace Template.GloryLevelModule
{
    public class Manager : BaseManager<Manager, GloryLevel>
    {
        public Manager()
        {
            LoadFromBin("GloryLevel.data");
        }
    };
}

namespace Template.GoodsModule
{
    public class Manager : BaseManager<Manager, Goods>
    {
        public Manager()
        {
            LoadFromBin("Goods.data");
        }
    };
}


namespace Template.GuildBattleModule
{
    public class Manager : BaseManager<Manager, GuildBattle>
    {
        public Manager()
        {
            LoadFromBin("GuildBattle.data");
        }
    };
}

namespace Template.GuildDonateModule
{
    public class Manager : BaseManager<Manager, GuildDonate>
    {
        public Manager()
        {
            LoadFromBin("GuildDonate.data");
        }
    };
}

namespace Template.GuildBuildLevelModule
{
    public class Manager : BaseManager<Manager, GuildBuildLevel>
    {
        public Manager()
        {
            LoadFromBin("GuildBuildLevel.data");
        }
    };
}

namespace Template.GuildConvoyModule
{
    public class Manager : BaseManager<Manager, GuildConvoy>
    {
        public Manager()
        {
            LoadFromBin("GuildConvoy.data");
        }
    };
}

namespace Template.GuildDefendModule
{
    public class Manager : BaseManager<Manager, GuildDefend>
    {
        public Manager()
        {
            LoadFromBin("GuildDefend.data");
        }
    };
}

namespace Template.GuildSkillModule
{
    public class Manager : BaseManager<Manager, GuildSkill>
    {
        public Manager()
        {
            LoadFromBin("GuildSkill.data");
        }
    };
}

namespace Template.GuildExpeditionModule
{
    public class Manager : BaseManager<Manager, GuildExpedition>
    {
        public Manager()
        {
            LoadFromBin("GuildExpedition.data");
        }
    };
}

namespace Template.GuildIconModule
{
    public class Manager : BaseManager<Manager, GuildIcon>
    {
        public Manager()
        {
            LoadFromBin("GuildIcon.data");
        }
    };
}

namespace Template.GuildLevelModule
{
    public class Manager : BaseManager<Manager, GuildLevel>
    {
        public Manager()
        {
            LoadFromBin("GuildLevel.data");
        }
    };
}

namespace Template.GuildPermissionModule
{
    public class Manager : BaseManager<Manager, GuildPermission>
    {
        public Manager()
        {
            LoadFromBin("GuildPermission.data");
        }
    };
}

namespace Template.GuildPrayItemModule
{
    public class Manager : BaseManager<Manager, GuildPrayItem>
    {
        public Manager()
        {
            LoadFromBin("GuildPrayItem.data");
        }
    };
}

namespace Template.GuildPrayPoolModule
{
    public class Manager : BaseManager<Manager, GuildPrayPool>
    {
        public Manager()
        {
            LoadFromBin("GuildPrayPool.data");
        }
    };
}

namespace Template.GuildRewardPointsModule
{
    public class Manager : BaseManager<Manager, GuildRewardPoints>
    {
        public Manager()
        {
            LoadFromBin("GuildRewardPoints.data");
        }
    };
}

namespace Template.GuildSalaryModule
{
    public class Manager : BaseManager<Manager, GuildSalary>
    {
        public Manager()
        {
            LoadFromBin("GuildSalary.data");
        }
    };
}

namespace Template.GuildShopModule
{
    public class Manager : BaseManager<Manager, GuildShop>
    {
        public Manager()
        {
            LoadFromBin("GuildShop.data");
        }
    };
}

namespace Template.GuildBuffModule
{
    public class Manager : BaseManager<Manager, GuildBuff>
    {
        public Manager()
        {
            LoadFromBin("GuildBuff.data");
        }
    };
}

namespace Template.GuildSmithyModule
{
    public class Manager : BaseManager<Manager, GuildSmithy>
    {
        public Manager()
        {
            LoadFromBin("GuildSmithy.data");
        }
    };
}

namespace Template.GuildWareHouseLevelModule
{
    public class Manager : BaseManager<Manager, GuildWareHouseLevel>
    {
        public Manager()
        {
            LoadFromBin("GuildWareHouseLevel.data");
        }
    };
}


namespace Template.HorseModule
{
    public class Manager : BaseManager<Manager, Horse>
    {
        public Manager()
        {
            LoadFromBin("Horse.data");
        }
    };
}

namespace Template.InstanceModule
{
    public class Manager : BaseManager<Manager, Instance>
    {
        public Manager()
        {
            LoadFromBin("Instance.data");
        }
    };
}

namespace Template.ItemApproachModule
{
    public class Manager : BaseManager<Manager, ItemApproach>
    {
        public Manager()
        {
            LoadFromBin("ItemApproach.data");
        }
    };
}

namespace Template.ItemMachiningModule
{
    public class Manager : BaseManager<Manager, ItemMachining>
    {
        public Manager()
        {
            LoadFromBin("ItemMachining.data");
        }
    };
}

namespace Template.ItemModule
{
    public class Manager : BaseManager<Manager, Item>
    {
        public Manager()
        {
            LoadFromBin("Item.data");
        }
    };
}

namespace Template.LegendaryGroupModule
{
    public class Manager : BaseManager<Manager, LegendaryGroup>
    {
        public Manager()
        {
            LoadFromBin("LegendaryGroup.data");
        }
    };
}

namespace Template.LegendaryPropertyUpgradeModule
{
    public class Manager : BaseManager<Manager, LegendaryPropertyUpgrade>
    {
        public Manager()
        {
            LoadFromBin("LegendaryPropertyUpgrade.data");
        }
    };
}

namespace Template.LetterModule
{
    public class Manager : BaseManager<Manager, Letter>
    {
        public Manager()
        {
            LoadFromBin("Letter.data");
        }
    };
}

namespace Template.LevelUpExpModule
{
    public class Manager : BaseManager<Manager, LevelUpExp>
    {
        public Manager()
        {
            LoadFromBin("LevelUpExp.data");
        }
    };
}

namespace Template.LivenessModule
{
    public class Manager : BaseManager<Manager, Liveness>
    {
        public Manager()
        {
            LoadFromBin("Liveness.data");
        }
    };
}

namespace Template.ManualAnecdoteModule
{
    public class Manager : BaseManager<Manager, ManualAnecdote>
    {
        public Manager()
        {
            LoadFromBin("ManualAnecdote.data");
        }
    };
}

namespace Template.ManualEntrieModule
{
    public class Manager : BaseManager<Manager, ManualEntrie>
    {
        public Manager()
        {
            LoadFromBin("ManualEntrie.data");
        }
    };
}

namespace Template.ManualTotalRewardModule
{
    public class Manager : BaseManager<Manager, ManualTotalReward>
    {
        public Manager()
        {
            LoadFromBin("ManualTotalReward.data");
        }
    };
}

namespace Template.MapModule
{
    public partial class Manager : BaseManager<Manager, Map>
    {
        public Manager()
        {
            LoadFromBin("Map.data");
        }
    };
}

namespace Template.MarketItemModule
{
    public class Manager : BaseManager<Manager, MarketItem>
    {
        public Manager()
        {
            LoadFromBin("MarketItem.data");
        }
    };
}

namespace Template.MarketModule
{
    public class Manager : BaseManager<Manager, Market>
    {
        public Manager()
        {
            LoadFromBin("Market.data");
        }
    };
}

namespace Template.MetaFightPropertyConfigModule
{
    public class Manager : BaseManager<Manager, MetaFightPropertyConfig>
    {
        public Manager()
        {
            LoadFromBin("MetaFightPropertyConfig.data");
        }
    };
}

namespace Template.MineModule
{
    public class Manager : BaseManager<Manager, Mine>
    {
        public Manager()
        {
            LoadFromBin("Mine.data");
        }
    };
}

namespace Template.MoneyModule
{
    public class Manager : BaseManager<Manager, Money>
    {
        public Manager()
        {
            LoadFromBin("Money.data");
        }

    }
}

namespace Template.MonsterAffixModule
{

    public class Manager : BaseManager<Manager, MonsterAffix>
    {
        public Manager()
        {
            LoadFromBin("MonsterAffix.data");
        }
    };

}

namespace Template.MonsterModule
{
    public class Manager : BaseManager<Manager, Monster>
    {
        public Manager()
        {
            LoadFromBin("Monster.data");
        }
    };
}

namespace Template.MonsterPositionModule
{
    public class Manager : BaseManager<Manager, MonsterPosition>
    {
        public Manager()
        {
            LoadFromBin("MonsterPosition.data");
        }
    };
}

namespace Template.MonsterPropertyModule
{
    public class Manager : BaseManager<Manager, MonsterProperty>
    {
        public Manager()
        {
            LoadFromBin("MonsterProperty.data");
        }
    };
}

namespace Template.MonthlyCardModule
{
    public class Manager : BaseManager<Manager, MonthlyCard>
    {
        public Manager()
        {
            LoadFromBin("MonthlyCard.data");
        }
    };
}

namespace Template.NavigationDataModule
{
    public class Manager : BaseManager<Manager, NavigationData>
    {
        public Manager()
        {
            LoadFromBin("NavigationData.data");
        }
    };
}

namespace Template.NpcModule
{
    public class Manager : BaseManager<Manager, Npc>
    {
        public Manager()
        {
            LoadFromBin("Npc.data");
        }
    };
}

namespace Template.NpcShopModule
{
    public class Manager : BaseManager<Manager, NpcShop>
    {
        public Manager()
        {
            LoadFromBin("NpcShop.data");
        }
    };
}

namespace Template.ObstacleModule
{
    public class Manager : BaseManager<Manager, Obstacle>
    {
        public Manager()
        {
            LoadFromBin("Obstacle.data");
        }
    };
}

namespace Template.PetLevelModule
{
    public class Manager : BaseManager<Manager, PetLevel>
    {
        public Manager()
        {
            LoadFromBin("PetLevel.data");
        }
    };
}

namespace Template.PetModule
{
    public class Manager : BaseManager<Manager, Pet>
    {
        public Manager()
        {
            LoadFromBin("Pet.data");
        }
    };
}

namespace Template.PetQualityInfoModule
{
    public class Manager : BaseManager<Manager, PetQualityInfo>
    {
        public Manager()
        {
            LoadFromBin("PetQualityInfo.data");
        }
    };
}

namespace Template.PlayerStrongCellModule
{
    public class Manager : BaseManager<Manager, PlayerStrongCell>
    {
        public Manager()
        {
            LoadFromBin("PlayerStrongCell.data");
        }
    };
}

namespace Template.PlayerStrongModule
{
    public class Manager : BaseManager<Manager, PlayerStrong>
    {
        public Manager()
        {
            LoadFromBin("PlayerStrong.data");
        }
    };
}

namespace Template.PlayerStrongValueModule
{
    public class Manager : BaseManager<Manager, PlayerStrongValue>
    {
        public Manager()
        {
            LoadFromBin("PlayerStrongValue.data");
        }
    };
}

namespace Template.ProfessionModule
{
    public class Manager : BaseManager<Manager, Profession>
    {
        public Manager()
        {
            LoadFromBin("Profession.data");
        }
    };
}

namespace Template.PublicDropModule
{
    public class Manager : BaseManager<Manager, PublicDrop>
    {
        public Manager()
        {
            LoadFromBin("PublicDrop.data");
        }
    };
}

namespace Template.PVP3v3Module
{
    public class Manager : BaseManager<Manager, PVP3v3>
    {
        public Manager()
        {
            LoadFromBin("PVP3v3.data");
        }
    };
}

namespace Template.QuestChapterModule
{
    public class Manager : BaseManager<Manager, QuestChapter>
    {
        public Manager()
        {
            LoadFromBin("QuestChapter.data");
        }
    };
}

namespace Template.QuestGroupModule
{
    public class Manager : BaseManager<Manager, QuestGroup>
    {
        public Manager()
        {
            LoadFromBin("QuestGroup.data");
        }
    };
}

namespace Template.QuestModule
{
    public class Manager : BaseManager<Manager, Quest>
    {
        public Manager()
        {
            LoadFromBin("Quest0.data", false);
            LoadFromBin("Quest1.data", false);
            LoadFromBin("Quest2.data", false);
            LoadFromBin("Quest3.data", false);
            LoadFromBin("Quest4.data", false);
            LoadFromBin("Quest5.data", false);
            LoadFromBin("Quest6.data", false);
            LoadFromBin("Quest7.data", false);
            LoadFromBin("Quest8.data", false);
        }

    };
}

namespace Template.QuickStoreModule
{
    public class Manager : BaseManager<Manager, QuickStore>
    {
        public Manager()
        {
            LoadFromBin("QuickStore.data");
        }
    };
}

namespace Template.RankModule
{
    public class Manager : BaseManager<Manager, Rank>
    {
        public Manager()
        {
            LoadFromBin("Rank.data");
        }
    };
}

namespace Template.RankRewardModule
{
    public class Manager : BaseManager<Manager, RankReward>
    {
        public Manager()
        {
            LoadFromBin("RankReward.data");
        }
    };
}

namespace Template.ReputationModule
{
    public class Manager : BaseManager<Manager, Reputation>
    {
        public Manager()
        {
            LoadFromBin("Reputation.data");
        }
    };
}

namespace Template.RewardModule
{
    public class Manager : BaseManager<Manager, Reward>
    {
        public Manager()
        {
            LoadFromBin("Reward.data");
        }
    };
}

namespace Template.RuneLevelUpModule
{
    public class Manager : BaseManager<Manager, RuneLevelUp>
    {
        public Manager()
        {
            LoadFromBin("RuneLevelUp.data");
        }
    };
}

namespace Template.RuneModule
{
    public class Manager : BaseManager<Manager, Rune>
    {
        public Manager()
        {
            LoadFromBin("Rune.data");
        }
    };
}

namespace Template.SceneModule
{
    public partial class Manager : BaseManager<Manager, Scene>
    {
        public Manager()
        {
            LoadFromBin("Scene.data");
        }
    };
}

namespace Template.ScriptCalendarModule
{
    public class Manager : BaseManager<Manager, ScriptCalendar>
    {
        public Manager()
        {
            LoadFromBin("ScriptCalendar.data");
        }
    }
}

namespace Template.ScriptConfigModule
{
    public class Manager : BaseManager<Manager, ScriptConfig>
    {
        public Manager()
        {
            LoadFromBin("ScriptConfig.data");
        }
    }
}

namespace Template.SensitiveWordModule
{
    public class Manager : BaseManager<Manager, SensitiveWord>
    {
        public Manager()
        {
            LoadFromBin("SensitiveWord.data");
        }
    };
}

namespace Template.ServiceModule
{
    public class Manager : BaseManager<Manager, Service>
    {
        public Manager()
        {
            LoadFromBin("Service.data");
        }
    };
}

namespace Template.SignModule
{

    public class Manager : BaseManager<Manager, Sign>
    {
        public Manager()
        {
            LoadFromBin("Sign.data");
        }

    };
}

namespace Template.SkillLearnConditionModule
{
    public class Manager : BaseManager<Manager, SkillLearnCondition>
    {
        public Manager()
        {
            LoadFromBin("SkillLearnCondition.data");
        }

    };
}

namespace Template.SkillLevelUpConditionModule
{
    public class Manager : BaseManager<Manager, SkillLevelUpCondition>
    {
        public Manager()
        {
            LoadFromBin("SkillLevelUpCondition.data");
        }
    };
}

namespace Template.SkillLevelUpModule
{
    public class Manager : BaseManager<Manager, SkillLevelUp>
    {
        public Manager()
        {
            LoadFromBin("SkillLevelUp.data");
        }
    };
}

namespace Template.SkillMasteryModule
{
    public class Manager : BaseManager<Manager, SkillMastery>
    {
        public Manager()
        {
            LoadFromBin("SkillMastery.data");
        }
    };
}

namespace Template.SkillModule
{
    public class Manager : BaseManager<Manager, Skill>
    {
        public Manager()
        {
            LoadFromBin("Skill0.data", false);
            LoadFromBin("Skill1.data", false);
        }
    };
}

namespace Template.SpecialIdModule
{
    public class Manager : BaseManager<Manager, SpecialId>
    {
        public Manager()
        {
            LoadFromBin("SpecialId.data");
        }
    };
}

namespace Template.StoneInforceModule
{
    public class Manager : BaseManager<Manager, StoneInforce>
    {
        public Manager()
        {
            LoadFromBin("StoneInforce.data");
        }
    };
}

namespace Template.StoreModule
{
    public class Manager : BaseManager<Manager, Store>
    {
        public Manager()
        {
            LoadFromBin("Store.data");
        }
    };
}

namespace Template.StoreTagModule
{
    public class Manager : BaseManager<Manager, StoreTag>
    {
        public Manager()
        {
            LoadFromBin("StoreTag.data");
        }
    };
}

namespace Template.SuitModule
{
    public class Manager : BaseManager<Manager, Suit>
    {
        public Manager()
        {
            LoadFromBin("Suit.data");
        }
    };
}

namespace Template.SystemNotifyModule
{
    public class Manager : BaseManager<Manager, SystemNotify>
    {
        public Manager()
        {
            LoadFromBin("SystemNotify.data");
        }
    };
}

namespace Template.TalentGroupModule
{
    public class Manager : BaseManager<Manager, TalentGroup>
    {
        public Manager()
        {
            LoadFromBin("TalentGroup.data");
        }
    };
}

namespace Template.TalentLevelUpModule
{
    public class Manager : BaseManager<Manager, TalentLevelUp>
    {
        public Manager()
        {
            LoadFromBin("TalentLevelUp.data");
        }
    };
}

namespace Template.TalentModule
{
    public class Manager : BaseManager<Manager, Talent>
    {
        public Manager()
        {
            LoadFromBin("Talent.data");
        }
    };
}

namespace Template.TeamRoomConfigModule
{
    public class Manager : BaseManager<Manager, TeamRoomConfig>
    {
        public Manager()
        {
            LoadFromBin("TeamRoomConfig.data");
        }
    };
}

namespace Template.TextModule
{
    public class Manager : BaseManager<Manager, Text>
    {
        public Manager()
        {
            LoadFromBin("Text.data");
        }
    };
}

namespace Template.TowerDungeonModule
{
    public class Manager : BaseManager<Manager, TowerDungeon>
    {
        public Manager()
        {
            LoadFromBin("TowerDungeon.data");
        }
    };
}

namespace Template.TransModule
{
    public class Manager : BaseManager<Manager, Trans>
    {
        public Manager()
        {
            LoadFromBin("Trans.data");
        }
    };
}

namespace Template.UserModule
{
    public class Manager : BaseManager<Manager, User>
    {
        public Manager()
        {
            LoadFromBin("User.data");
        }
    };
}

namespace Template.WingGradeUpModule
{
    public class Manager : BaseManager<Manager, WingGradeUp>
    {
        public Manager()
        {
            LoadFromBin("WingGradeUp.data");
        }
    };
}

namespace Template.WingLevelUpModule
{
    public class Manager : BaseManager<Manager, WingLevelUp>
    {
        public Manager()
        {
            LoadFromBin("WingLevelUp.data");
        }
    };
}

namespace Template.WingLevelWeightModule
{
    public class Manager : BaseManager<Manager, WingLevelWeight>
    {
        public Manager()
        {
            LoadFromBin("WingLevelWeight.data");
        }
    };
}

namespace Template.WingModule
{
    public class Manager : BaseManager<Manager, Wing>
    {
        public Manager()
        {
            LoadFromBin("Wing.data");
        }
    };
}

namespace Template.WingTalentLevelModule
{
    public class Manager : BaseManager<Manager, WingTalentLevel>
    {
        public Manager()
        {
            LoadFromBin("WingTalentLevel.data");
        }
    };
}

namespace Template.WingTalentModule
{
    public class Manager : BaseManager<Manager, WingTalent>
    {
        public Manager()
        {
            LoadFromBin("WingTalent.data");
        }
    };
}

namespace Template.WorldBossConfigModule
{
    public class Manager : BaseManager<Manager, WorldBossConfig>
    {
        public Manager()
        {
            LoadFromBin("WorldBossConfig.data");
        }
    };
}

namespace Template.WingTalentPageModule
{
    public class Manager : BaseManager<Manager, WingTalentPage>
    {
        public Manager()
        {
            LoadFromBin("WingTalentPage.data");
        }
    };
}

namespace Template.StateModule
{
    public class Manager : BaseManager<Manager, State>
    {
        public Manager()
        {
            LoadFromBin("State.data");
        }
    };
}

namespace Template.NpcSaleModule
{

    public class Manager : BaseManager<Manager, NpcSale>
    {
        public Manager()
        {
            LoadFromBin("NpcSale.data");
        }

    };
}

namespace Template.LuckyInforceModule
{
    public class Manager : BaseManager<Manager, LuckyInforce>
    {
        public Manager()
        {
            LoadFromBin("LuckyInforce.data");
        }
    };
}

namespace Template.FunModule
{
    public class Manager : BaseManager<Manager, Fun>
    {
        public Manager()
        {
            LoadFromBin("Fun.data");
        }
    };
}


namespace Template.TaskLuckModule
{
    public class Manager : BaseManager<Manager, TaskLuck>
    {
        public Manager()
        {
            LoadFromBin("TaskLuck.data");
        }
    };
}

namespace Template.DailyTaskModule
{
    public class Manager : BaseManager<Manager, DailyTask>
    {
        public Manager()
        {
            LoadFromBin("DailyTask.data");
        }
    };
}

namespace Template.DailyTaskBoxModule
{
    public class Manager : BaseManager<Manager, DailyTaskBox>
    {
        public Manager()
        {
            LoadFromBin("DailyTaskBox.data");
        }
    };
}

namespace Template.SpecialSignModule
{
    public class Manager : BaseManager<Manager, SpecialSign>
    {
        public Manager()
        {
            LoadFromBin("SpecialSign.data");
        }
    };
}

namespace Template.HotTimeConfigureModule
{
    public class Manager : BaseManager<Manager, HotTimeConfigure>
    {
        public Manager()
        {
            LoadFromBin("HotTimeConfigure.data");
        }
    };
}

namespace Template.DungeonIntroductionPopupModule
{
    public class Manager : BaseManager<Manager, DungeonIntroductionPopup>
    {
        public Manager()
        {
            LoadFromBin("DungeonIntroductionPopup.data");
        }
    };
}

namespace Template.InforceDecomposeModule
{
    public class Manager : BaseManager<Manager, InforceDecompose>
    {
        public Manager()
        {
            LoadFromBin("InforceDecompose.data");
        }
    };
}

namespace Template.OnlineRewardModule
{
    public class Manager : BaseManager<Manager, OnlineReward>
    {
        public Manager()
        {
            LoadFromBin("OnlineReward.data");
        }
    };
}
namespace Template.ExpFindModule
{
    public class Manager : BaseManager<Manager, ExpFind>
    {
        public Manager()
        {
            LoadFromBin("ExpFind.data");
        }
    };
}

namespace Template.HangQuestModule
{
	public class Manager : BaseManager<Manager, HangQuest>
	{
		public Manager()
		{
			LoadFromBin("HangQuest.data");
		}
	};
}

namespace Template.GrowthGuidanceModule
{
    public class Manager : BaseManager<Manager, GrowthGuidance>
    {
        public Manager()
        {
            LoadFromBin("GrowthGuidance.data");
        }
    };
}

namespace Template.EliteBossConfigModule
{
    public class Manager : BaseManager<Manager, EliteBossConfig>
    {
        public Manager()
        {
            LoadFromBin("EliteBossConfig.data");
        }
    };
}

namespace Template.FestivalActivityModule
{
    public class Manager : BaseManager<Manager, FestivalActivity>
    {
        public Manager()
        {
            LoadFromBin("FestivalActivity.data");
        }
    };
}

namespace Template.DiceModule
{
    public class Manager : BaseManager<Manager, Dice>
    {
        public Manager()
        {
            LoadFromBin("Dice.data");
        }
    };
}