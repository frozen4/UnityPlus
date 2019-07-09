using Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Template;


public partial class LuaDataCheck : Singleton<LuaDataCheck>
{

    public Dictionary<int, Monster> _MonsterData = new Dictionary<int, Monster>();
    public Dictionary<int, Npc> _NpcData = new Dictionary<int, Npc>();
    public Dictionary<int, Obstacle> _ObstacleData = new Dictionary<int, Obstacle>();
    public Dictionary<int, Mine> _MineData = new Dictionary<int, Mine>();
    public Dictionary<int, Profession> _ProfessionData = new Dictionary<int, Profession>();
    public Dictionary<int, Item> _ItemData = new Dictionary<int, Item>();
    public Dictionary<int, Horse> _HorseData = new Dictionary<int, Horse>();
    public Dictionary<int, Pet> _PetData = new Dictionary<int, Pet>();
    public Dictionary<int, Wing> _WingData = new Dictionary<int, Wing>();
    public Dictionary<int, WingTalentPage> _WingTalentPageData = new Dictionary<int, WingTalentPage>();
    public Dictionary<int, Money> _MoneyData = new Dictionary<int, Money>();
    public Dictionary<int, Dress> _DressData = new Dictionary<int, Dress>();
    public Dictionary<int, DyeAndEmbroidery> _DyeAndEmbroideryData = new Dictionary<int, DyeAndEmbroidery>();
    public Dictionary<int, ExecutionUnit> _ExecutionUnitData = new Dictionary<int, ExecutionUnit>();
    public Dictionary<int, Scene> _SceneData = new Dictionary<int, Scene>();
    public Dictionary<int, Map> _MapData = new Dictionary<int, Map>();
    public Dictionary<int, Instance> _InstanceData = new Dictionary<int, Instance>();
    public Dictionary<int, Achievement> _AchievementData = new Dictionary<int, Achievement>();
    public Dictionary<int, Designation> _DesignationData = new Dictionary<int, Designation>();
    public Dictionary<int, Fun> _FunData = new Dictionary<int, Fun>();
    public Dictionary<int, ManualEntrie> _ManualEntrieData = new Dictionary<int, ManualEntrie>();
    public Dictionary<int, Reputation> _ReputationData = new Dictionary<int, Reputation>();
    public Dictionary<int, Skill> _SkillData = new Dictionary<int, Skill>();
    public Dictionary<int, Actor> _ActorData = new Dictionary<int, Actor>();
    public Dictionary<int, State> _StateData = new Dictionary<int, State>();
    public Dictionary<int, WorldBossConfig> _WorldBossConfigData = new Dictionary<int, WorldBossConfig>();
    public Dictionary<int, TeamRoomConfig> _TeamRoomConfigData = new Dictionary<int, TeamRoomConfig>();
    public Dictionary<int, Service> _ServiceData = new Dictionary<int, Service>();
    public Dictionary<int, Rune> _RuneData = new Dictionary<int, Rune>();
    public Dictionary<int, Talent> _TalentData = new Dictionary<int, Talent>();

    public const string Model_Title = "模型";
    public const string Icon_Title = "图标";

    public const string Icon_Base_Path_0 = "Assets/Outputs/";
    public const string Icon_Base_Path_1 = "Assets/Outputs/CommonAtlas/";
    public const string Icon_Base_Path_2 = "Assets/Outputs/CommonAtlas/Icon/";

    public const string Monster_Type = "Monster.data";
    public const string Npc_Type = "Npc.data";
    public const string Obstacle_Type = "Obstacle.data";
    public const string Mine_Type = "Mine.data";
    public const string Profession_Type = "Profession.data";
    public const string Item_Type = "Item.data";
    public const string Horse_Type = "Horse.data";
    public const string Pet_Type = "Pet.data";
    public const string Wing_Type = "Wing.data";
    public const string WingTalentPage_Type = "WingTalentPage.data";
    public const string Money_Type = "Money.data";
    public const string Dress_Type = "Dress.data";
    public const string DyeAndEmbroidery_Type = "DyeAndEmbroidery.data";
    public const string ExecutionUnit_Type = "ExecutionUnit.data";
    public const string Scene_Type = "Scene.data";
    public const string Map_Type = "Map.data";
    public const string Instance_Type = "Instance.data";
    public const string Achievement_Type = "Achievement.data";
    public const string Designation_Type = "Designation.data";
    public const string ActivityContent_Type = "ActivityContent.data";
    public const string Fun_Type = "Fun.data";
    public const string ColorConfig_Type = "ColorConfig.data";
    public const string ManualEntrie_Type = "ManualEntrie.data";
    public const string Reputation_Type = "Reputation.data";
    public const string Skill_Type = "Skill.data";
    public const string Actor_Type = "Actor.data";
    public const string State_Type = "State.data";
    public const string WorldBossConfig_Type = "WorldBossConfig.data";
    public const string TeamRoomConfig_Type = "TeamRoomConfig.data";
    public const string Service_Type = "Service.data";
    public const string Rune_Type = "Rune.data";
    public const string Talent_Type = "Talent.data";

    private const string Log_1 = "Asset.data缺少对应的Id";
    private const string Log_2 = "AssetBundle缺少对应的资源";
    private const string Log_3 = "错误的图标路径";

    private SortedDictionary<LuaDataCheckType, List<LuaDataCheckInfo>> _ModelCheckInfo = new SortedDictionary<LuaDataCheckType, List<LuaDataCheckInfo>>();
    private SortedDictionary<LuaDataCheckType, List<LuaDataCheckInfo>> _IconCheckInfo = new SortedDictionary<LuaDataCheckType, List<LuaDataCheckInfo>>();

    public void Init()
    {
        //预先设置路径
        Template.Path.BasePath = System.IO.Path.Combine(Application.dataPath, "../../GameRes/");
        Template.Path.BinPath = "Data/";
        //初始化数据存储
        _MonsterData = new Dictionary<int, Monster>();
        _NpcData = new Dictionary<int, Npc>();
        _ObstacleData = new Dictionary<int, Obstacle>();
        _MineData = new Dictionary<int, Mine>();
        _ProfessionData = new Dictionary<int, Profession>();
        _ItemData = new Dictionary<int, Item>();
        _HorseData = new Dictionary<int, Horse>();
        _PetData = new Dictionary<int, Pet>();
        _WingData = new Dictionary<int, Wing>();
        _WingTalentPageData = new Dictionary<int, WingTalentPage>();
        _MoneyData = new Dictionary<int, Money>();
        _DressData = new Dictionary<int, Dress>();
        _DyeAndEmbroideryData = new Dictionary<int, DyeAndEmbroidery>();
        _ExecutionUnitData = new Dictionary<int, ExecutionUnit>();
        _MapData = new Dictionary<int, Map>();
        _InstanceData = new Dictionary<int, Instance>();
        _AchievementData = new Dictionary<int, Achievement>();
        _DesignationData = new Dictionary<int, Designation>();
        _FunData = new Dictionary<int, Fun>();
        _ManualEntrieData = new Dictionary<int, ManualEntrie>();
        _ReputationData = new Dictionary<int, Reputation>();
        _SkillData = new Dictionary<int, Skill>();
        _ActorData = new Dictionary<int, Actor>();
        _StateData = new Dictionary<int, State>();
        _WorldBossConfigData = new Dictionary<int, WorldBossConfig>();
        _TeamRoomConfigData = new Dictionary<int, TeamRoomConfig>();
        _ServiceData = new Dictionary<int, Service>();
        _RuneData = new Dictionary<int, Rune>();
        _TalentData = new Dictionary<int, Talent>();

        _ModelCheckInfo = new SortedDictionary<LuaDataCheckType, List<LuaDataCheckInfo>>();
        _IconCheckInfo = new SortedDictionary<LuaDataCheckType, List<LuaDataCheckInfo>>();
    }

    /// <summary>
    /// 初始化所有需要检测资源的Data
    /// </summary>
    public void InitData()
    {
        var monsterManager = Template.MonsterModule.Manager.Instance;
        monsterManager.ParseTemplateAll(true);
        _MonsterData = monsterManager.GetTemplateMap();

        var npcManager = Template.NpcModule.Manager.Instance;
        npcManager.ParseTemplateAll(true);
        _NpcData = npcManager.GetTemplateMap();

        var obstacleManager = Template.ObstacleModule.Manager.Instance;
        obstacleManager.ParseTemplateAll(true);
        _ObstacleData = obstacleManager.GetTemplateMap();

        var mineManager = Template.MineModule.Manager.Instance;
        mineManager.ParseTemplateAll(true);
        _MineData = mineManager.GetTemplateMap();

        var professionManager = Template.ProfessionModule.Manager.Instance;
        professionManager.ParseTemplateAll(true);
        _ProfessionData = professionManager.GetTemplateMap();

        var itemManager = Template.ItemModule.Manager.Instance;
        itemManager.ParseTemplateAll(true);
        _ItemData = itemManager.GetTemplateMap();

        var horseManager = Template.HorseModule.Manager.Instance;
        horseManager.ParseTemplateAll(true);
        _HorseData = horseManager.GetTemplateMap();

        var petManager = Template.PetModule.Manager.Instance;
        petManager.ParseTemplateAll(true);
        _PetData = petManager.GetTemplateMap();

        var wingManager = Template.WingModule.Manager.Instance;
        wingManager.ParseTemplateAll(true);
        _WingData = wingManager.GetTemplateMap();

        var wingTalentPageManager = Template.WingTalentPageModule.Manager.Instance;
        wingTalentPageManager.ParseTemplateAll(true);
        _WingTalentPageData = wingTalentPageManager.GetTemplateMap();

        var moneyManager = Template.MoneyModule.Manager.Instance;
        moneyManager.ParseTemplateAll(true);
        _MoneyData = moneyManager.GetTemplateMap();

        var dressManager = Template.DressModule.Manager.Instance;
        dressManager.ParseTemplateAll(true);
        _DressData = dressManager.GetTemplateMap();

        var dyeAndEmbroideryManager = Template.DyeAndEmbroideryModule.Manager.Instance;
        dyeAndEmbroideryManager.ParseTemplateAll(true);
        _DyeAndEmbroideryData = dyeAndEmbroideryManager.GetTemplateMap();

        var executionUnitManager = Template.ExecutionUnitModule.Manager.Instance;
        executionUnitManager.ParseTemplateAll(true);
        _ExecutionUnitData = executionUnitManager.GetTemplateMap();

        var mapManager = Template.MapModule.Manager.Instance;
        mapManager.ParseTemplateAll(true);
        _MapData = mapManager.GetTemplateMap();

        var instanceManager = Template.InstanceModule.Manager.Instance;
        instanceManager.ParseTemplateAll(true);
        _InstanceData = instanceManager.GetTemplateMap();

        var achievementManager = Template.AchievementModule.Manager.Instance;
        achievementManager.ParseTemplateAll(true);
        _AchievementData = achievementManager.GetTemplateMap();

        var designationManager = Template.DesignationModule.Manager.Instance;
        designationManager.ParseTemplateAll(true);
        _DesignationData = designationManager.GetTemplateMap();

        var funManager = Template.FunModule.Manager.Instance;
        funManager.ParseTemplateAll(true);
        _FunData = funManager.GetTemplateMap();

        var manualEntrieManager = Template.ManualEntrieModule.Manager.Instance;
        manualEntrieManager.ParseTemplateAll(true);
        _ManualEntrieData = manualEntrieManager.GetTemplateMap();

        var reputationManager = Template.ReputationModule.Manager.Instance;
        reputationManager.ParseTemplateAll(true);
        _ReputationData = reputationManager.GetTemplateMap();

        var skillManager = Template.SkillModule.Manager.Instance;
        skillManager.ParseTemplateAll(true);
        _SkillData = skillManager.GetTemplateMap();

        var actorManager = Template.ActorModule.Manager.Instance;
        actorManager.ParseTemplateAll(true);
        _ActorData = actorManager.GetTemplateMap();

        var stateManager = Template.StateModule.Manager.Instance;
        stateManager.ParseTemplateAll(true);
        _StateData = stateManager.GetTemplateMap();

        var worldBossConfigManager = Template.WorldBossConfigModule.Manager.Instance;
        worldBossConfigManager.ParseTemplateAll(true);
        _WorldBossConfigData = worldBossConfigManager.GetTemplateMap();

        var teamRoomConfigManager = Template.TeamRoomConfigModule.Manager.Instance;
        teamRoomConfigManager.ParseTemplateAll(true);
        _TeamRoomConfigData = teamRoomConfigManager.GetTemplateMap();

        var serviceManager = Template.ServiceModule.Manager.Instance;
        serviceManager.ParseTemplateAll(true);
        _ServiceData = serviceManager.GetTemplateMap();

        var runeManager = Template.RuneModule.Manager.Instance;
        runeManager.ParseTemplateAll(true);
        _RuneData = runeManager.GetTemplateMap();

        var talentManager = Template.TalentModule.Manager.Instance;
        talentManager.ParseTemplateAll(true);
        _TalentData = talentManager.GetTemplateMap();
    }

    public IEnumerable CheckModelAssetCoroutine()
    {
        //
        int count = 0;
        int total = _MonsterData.Values.Count;
        foreach (var monster in _MonsterData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(monster)： {0}", monster.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(monster.ModelAssetPath))
                CheckModelAsset(monster.ModelAssetPath, monster.Id, "ModelAssetPath", LuaDataCheckType.Monster);
            if (!string.IsNullOrEmpty(monster.WeaponAssetPath))
                CheckModelAsset(monster.WeaponAssetPath, monster.Id, "WeaponAssetPath", LuaDataCheckType.Monster);
        }

        //
        count = 0;
        total = _NpcData.Values.Count;
        foreach (var npc in _NpcData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(npc)： {0}", npc.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(npc.ModelAssetPath))
                CheckModelAsset(npc.ModelAssetPath, npc.Id, "ModelAssetPath", LuaDataCheckType.Npc);
            if (!string.IsNullOrEmpty(npc.WeaponAssetPath))
                CheckModelAsset(npc.WeaponAssetPath, npc.Id, "WeaponAssetPath", LuaDataCheckType.Npc);
        }

        //
        count = 0;
        total = _ObstacleData.Values.Count;
        foreach (var obstacle in _ObstacleData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(obstacle)： {0}", obstacle.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(obstacle.ModelAssetPath))
                CheckModelAsset(obstacle.ModelAssetPath, obstacle.Id, "ModelAssetPath", LuaDataCheckType.Obstacle);
        }

        //
        count = 0;
        total = _MineData.Values.Count;
        foreach (var mine in _MineData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(mine)： {0}", mine.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(mine.ModelAssetPath))
                CheckModelAsset(mine.ModelAssetPath, mine.Id, "ModelAssetPath", LuaDataCheckType.Mine);
            if (!string.IsNullOrEmpty(mine.EffectAssetPath))
                CheckModelAsset(mine.EffectAssetPath, mine.Id, "EffectAssetPath", LuaDataCheckType.Mine);
            if (!string.IsNullOrEmpty(mine.AudioAssetPath))
                CheckModelAsset(mine.AudioAssetPath, mine.Id, "AudioAssetPath", LuaDataCheckType.Mine);
            if (!string.IsNullOrEmpty(mine.DisappearAssetPath))
                CheckModelAsset(mine.DisappearAssetPath, mine.Id, "DisappearAssetPath", LuaDataCheckType.Mine);
        }

        //
        count = 0;
        total = _ProfessionData.Values.Count;
        foreach (var profession in _ProfessionData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(profession)： {0}", profession.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(profession.FemaleModelAssetPath))
                CheckModelAsset(profession.FemaleModelAssetPath, profession.Id, "FemaleModelAssetPath", LuaDataCheckType.Profession);
            else
                CheckModelAsset(profession.MaleModelAssetPath, profession.Id, "MaleModelAssetPath", LuaDataCheckType.Profession);
        }

        //
        count = 0;
        total = _ItemData.Values.Count;
        foreach (var item in _ItemData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(item)： {0}", item.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(item.LootModelAssetPath))
                CheckModelAsset(item.LootModelAssetPath, item.Id, "LootModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.MaleLeftHandModelAssetPath))
                CheckModelAsset(item.MaleLeftHandModelAssetPath, item.Id, "MaleLeftHandModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.FemaleLeftHandModelAssetPath))
                CheckModelAsset(item.FemaleLeftHandModelAssetPath, item.Id, "FemaleLeftHandModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.MaleRightHandModelAssetPath))
                CheckModelAsset(item.MaleRightHandModelAssetPath, item.Id, "MaleRightHandModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.FemaleRightHandModelAssetPath))
                CheckModelAsset(item.FemaleRightHandModelAssetPath, item.Id, "FemaleRightHandModelAssetPath", LuaDataCheckType.Item);
        }

        //
        count = 0;
        total = _HorseData.Values.Count;
        foreach (var horse in _HorseData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(horse)： {0}", horse.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(horse.ModelAssetPath))
                CheckModelAsset(horse.ModelAssetPath, horse.Id, "ModelAssetPath", LuaDataCheckType.Horse);
        }

        //
        count = 0;
        total = _PetData.Values.Count;
        foreach (var pet in _PetData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(pet)： {0}", pet.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (pet.AssociatedMonsterId != 0)
            {
                Template.Monster monster;
                if (!_MonsterData.TryGetValue(pet.AssociatedMonsterId, out monster))
                    CheckModelAsset("", pet.Id, "AssociatedMonsterId", LuaDataCheckType.Pet);

                if (!string.IsNullOrEmpty(monster.ModelAssetPath))
                    CheckModelAsset(monster.ModelAssetPath, pet.AssociatedMonsterId, "ModelAssetPath", LuaDataCheckType.Pet);
            }
        }

        //
        count = 0;
        total = _WingData.Values.Count;
        foreach (var wing in _WingData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(wing)： {0}", wing.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(wing.ModelAssetPath))
                CheckModelAsset(wing.ModelAssetPath, wing.Id, "ModelAssetPath", LuaDataCheckType.Wing);
        }

        //
        count = 0;
        total = _WingTalentPageData.Values.Count;
        foreach (var wingTalentPage in _WingTalentPageData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(wingTalentPage)： {0}", wingTalentPage.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(wingTalentPage.ModelAssetPath))
                CheckModelAsset(wingTalentPage.ModelAssetPath, wingTalentPage.Id, "ModelAssetPath", LuaDataCheckType.WingTalentPage);

        }

        //
        count = 0;
        total = _DressData.Values.Count;
        foreach (var dress in _DressData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(dress)： {0}", dress.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(dress.AssetPath1))
                CheckModelAsset(dress.AssetPath1, dress.Id, "AssetPath1", LuaDataCheckType.Dress);

            if (!string.IsNullOrEmpty(dress.AssetPath2))
                CheckModelAsset(dress.AssetPath2, dress.Id, "AssetPath2", LuaDataCheckType.Dress);
        }

        //
        count = 0;
        total = _DyeAndEmbroideryData.Values.Count;
        foreach (var dyeAndEmbroidery in _DyeAndEmbroideryData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(dyeAndEmbroidery)： {0}", dyeAndEmbroidery.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            //             if (dyeAndEmbroidery.EmbroideryAssetId != 0)
            //                 CheckModelAsset(dyeAndEmbroidery.EmbroideryAssetId, dyeAndEmbroidery.Id, "EmbroideryAssetId", LuaDataCheckType.DyeAndEmbroidery);

            //if (!string.IsNullOrEmpty(dyeAndEmbroidery.EmbroideryAssetPath))
            //    CheckModelAsset(dyeAndEmbroidery.EmbroideryAssetPath, dyeAndEmbroidery.Id, "EmbroideryAssetPath", LuaDataCheckType.DyeAndEmbroidery);

        }

        //
        count = 0;
        total = _ExecutionUnitData.Values.Count;
        foreach (var executionUnit in _ExecutionUnitData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(executionUnit)： {0}", executionUnit.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (executionUnit.Event.Transform != null)
            {
                if (!string.IsNullOrEmpty(executionUnit.Event.Transform.BodyAssetPath))
                    CheckModelAsset(executionUnit.Event.Transform.BodyAssetPath, executionUnit.Id, "BodyAssetPath", LuaDataCheckType.ExecutionUnit);
                if (!string.IsNullOrEmpty(executionUnit.Event.Transform.HeadAssetPath))
                    CheckModelAsset(executionUnit.Event.Transform.HeadAssetPath, executionUnit.Id, "HeadAssetPath", LuaDataCheckType.ExecutionUnit);
                if (!string.IsNullOrEmpty(executionUnit.Event.Transform.HairAssetPath))
                    CheckModelAsset(executionUnit.Event.Transform.HairAssetPath, executionUnit.Id, "HairAssetPath", LuaDataCheckType.ExecutionUnit);
                if (!string.IsNullOrEmpty(executionUnit.Event.Transform.WeaponAssetPath))
                    CheckModelAsset(executionUnit.Event.Transform.WeaponAssetPath, executionUnit.Id, "WeaponAssetPath", LuaDataCheckType.ExecutionUnit);
            }
            if (executionUnit.Event.Audio != null)
            {
                if (!string.IsNullOrEmpty(executionUnit.Event.Audio.AssetPath))
                    CheckModelAsset(executionUnit.Event.Audio.AssetPath, executionUnit.Id, "AssetPath", LuaDataCheckType.ExecutionUnit);
            }
        }

        //
        count = 0;
        total = _SceneData.Values.Count;
        foreach (var scene in _SceneData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(scene)： {0}", scene.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(scene.AssetPath))
                CheckModelAsset(scene.AssetPath, scene.Id, "AssetPath", LuaDataCheckType.Scene);
            for (int i = 0; i < scene.RegionRoot.Regions.Count; i++)
            {
                if (!string.IsNullOrEmpty(scene.RegionRoot.Regions[i].CGAssetPath))
                    CheckModelAsset(scene.RegionRoot.Regions[i].CGAssetPath, scene.Id, "CGAssetPath", LuaDataCheckType.Scene);
            }
        }

        //
        count = 0;
        total = _MapData.Values.Count;
        foreach (var map in _MapData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(map)： {0}", map.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(map.CGAssetPath))
                CheckModelAsset(map.CGAssetPath, map.Id, "CGAssetPath", LuaDataCheckType.Map);
        }

        //
        count = 0;
        total = _ActorData.Values.Count;
        foreach (var actor in _ActorData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(actor)： {0}", actor.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(actor.GfxAssetPath))
                CheckModelAsset(actor.GfxAssetPath, actor.Id, "GfxAssetPath", LuaDataCheckType.Actor);
        }

        //
        count = 0;
        total = _WorldBossConfigData.Values.Count;
        foreach (var worldBossConfig in _WorldBossConfigData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(worldBossConfig)： {0}", worldBossConfig.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            //if (!string.IsNullOrEmpty(worldBossConfig.ModelAssetPath))
                //CheckModelAsset(worldBossConfig.ModelAssetPath, worldBossConfig.Id, "ModelAssetPath", LuaDataCheckType.WorldBossConfig);
        }

        //
        count = 0;
        total = _TeamRoomConfigData.Values.Count;
        foreach (var teamRoomConfig in _TeamRoomConfigData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查模型(teamRoomConfig)： {0}", teamRoomConfig.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(teamRoomConfig.Model))
            {
                CheckModelAsset(teamRoomConfig.Model, teamRoomConfig.Id, "Model", LuaDataCheckType.TeamRoomConfig);
            }
        }

        yield return null;
    }

    public IEnumerable CheckIconPathCoroutine()
    {
        int count = 0;
        int total = _MonsterData.Values.Count;
        foreach (var monster in _MonsterData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(monster)： {0}", monster.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(monster.IconAtlasPath))
            {

            }
        }

        //
        count = 0;
        total = _NpcData.Values.Count;
        foreach (var npc in _NpcData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(npc)： {0}", npc.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(npc.IconAtlasPath))
            {
                //TODO:路径不确定
            }
        }

        //
        count = 0;
        total = _SceneData.Values.Count;
        foreach (var scene in _SceneData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(scene)： {0}", scene.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(scene.MiniMapAtlasPath))
                CheckIconPath(scene.MiniMapAtlasPath, scene.Id, "MiniMapAtlasPath", LuaDataCheckType.Scene);
        }

        //
        count = 0;
        total = _InstanceData.Values.Count;
        foreach (var instance in _InstanceData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(instance)： {0}", instance.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(instance.IconPath))
                CheckIconPath(instance.IconPath, instance.Id, "IconPath", LuaDataCheckType.Instance);
        }

        //
        count = 0;
        total = _ItemData.Values.Count;
        foreach (var item in _ItemData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(item)： {0}", item.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(item.IconAtlasPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + item.IconAtlasPath + ".png", item.Id, "IconAtlasPath", LuaDataCheckType.Item);

        }

        //
        count = 0;
        total = _HorseData.Values.Count;
        foreach (var horse in _HorseData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(horse)： {0}", horse.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(horse.IconPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + horse.IconPath + ".png", horse.Id, "IconPath", LuaDataCheckType.Horse);
        }

        //
        count = 0;
        total = _PetData.Values.Count;
        foreach (var pet in _PetData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(pet)： {0}", pet.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(pet.IconPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + pet.IconPath + ".png", pet.Id, "IconPath", LuaDataCheckType.Pet);
        }


        //
        count = 0;
        total = _AchievementData.Values.Count;
        foreach (var achievement in _AchievementData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(achievement)： {0}", achievement.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            //TODO:路径不确定
            //if (achievement.IconPath != string.Empty)
            //    CheckIconPath(Icon_Base_Path_2 + achievement.IconPath+".png", achievement.Id, "", LuaDataCheckType.Achievement);
        }

        //
        count = 0;
        total = _DesignationData.Values.Count;
        foreach (var designation in _DesignationData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(designation)： {0}", designation.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            //TODO:路径不确定
        }

        //
        count = 0;
        total = _FunData.Values.Count;
        foreach (var fun in _FunData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(fun)： {0}", fun.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            //TODO:路径不确定
            //if (fun.IconPath != string.Empty)
            //    CheckIconPath(Icon_Base_Path_2 + "main/" + fun.IconPath + ".png", fun.Id, "IconPath", LuaDataCheckType.Fun);
        }

        //
        count = 0;
        total = _WingData.Values.Count;
        foreach (var wing in _WingData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(wing)： {0}", wing.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(wing.IconPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + wing.IconPath + ".png", wing.Id, "IconPath", LuaDataCheckType.Wing);
        }

        //
        count = 0;
        total = _DressData.Values.Count;
        foreach (var dress in _DressData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(dress)： {0}", dress.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(dress.IconPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + dress.IconPath + ".png", dress.Id, "IconPath", LuaDataCheckType.Dress);
        }

        //
        count = 0;
        total = _ManualEntrieData.Values.Count;
        foreach (var manualEntrie in _ManualEntrieData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(manualEntrie)： {0}", manualEntrie.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            //TODO:路径不确定
            //if (manualEntrie.IconPath != string.Empty)
            //    CheckIconPath(Icon_Base_Path_2 + manualEntrie.IconPath+".png", manualEntrie.Id, "IconPath", LuaDataCheckType.ManualEntrie);
        }

        //
        count = 0;
        total = _MoneyData.Values.Count;
        foreach (var money in _MoneyData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(money)： {0}", money.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(money.IconPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + money.IconPath + ".png", money.Id, "IconPath", LuaDataCheckType.Money);
        }

        //
        count = 0;
        total = _ReputationData.Values.Count;
        foreach (var reputation in _ReputationData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(reputation)： {0}", reputation.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(reputation.IconAtlasPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + reputation.IconAtlasPath, reputation.Id, "IconAtlasPath", LuaDataCheckType.Reputation);
        }

        //
        count = 0;
        total = _SkillData.Values.Count;
        foreach (var skill in _SkillData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(skill)： {0}", skill.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(skill.IconName))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + skill.IconName + ".png", skill.Id, "IconName", LuaDataCheckType.Skill);
        }

        //
        count = 0;
        total = _ExecutionUnitData.Values.Count;
        foreach (var executionUnit in _ExecutionUnitData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(executionUnit)： {0}", executionUnit.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (executionUnit.Event.ChangeSkillIcon != null && !string.IsNullOrEmpty(executionUnit.Event.ChangeSkillIcon.IconPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + executionUnit.Event.ChangeSkillIcon.IconPath + ".png", executionUnit.Id, "IconPath", LuaDataCheckType.ExecutionUnit);

        }

        //
        count = 0;
        total = _StateData.Values.Count;
        foreach (var state in _StateData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(state)： {0}", state.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(state.IconPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + state.IconPath + ".png", state.Id, "IconPath", LuaDataCheckType.State);
        }

        //
        count = 0;
        total = _ProfessionData.Values.Count;
        foreach (var profession in _ProfessionData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(profession)： {0}", profession.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(profession.SymbolAtlasPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + profession.SymbolAtlasPath + ".png", profession.Id, "SymbolAtlasPath", LuaDataCheckType.Profession);
            if (!string.IsNullOrEmpty(profession.MaleIconAtlasPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + profession.MaleIconAtlasPath + ".png", profession.Id, "MaleIconAtlasPath", LuaDataCheckType.Profession);
            if (!string.IsNullOrEmpty(profession.FemaleIconAtlasPath))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + profession.FemaleIconAtlasPath + ".png", profession.Id, "FemaleIconAtlasPath", LuaDataCheckType.Profession);
        }

        //
        count = 0;
        total = _WorldBossConfigData.Values.Count;
        foreach (var worldBossConfig in _WorldBossConfigData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(worldBossConfig)： {0}", worldBossConfig.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            //             if (!string.IsNullOrEmpty(worldBossConfig.Icon))
            //                 CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + "Head/" + worldBossConfig.Icon + ".png", worldBossConfig.Id, "Icon", LuaDataCheckType.WorldBossConfig);
        }

        //
        count = 0;
        total = _ServiceData.Values.Count;
        foreach (var service in _ServiceData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(service)： {0}", service.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            //TODO:路径不确定
            //if (service.OverHeadIcon != string.Empty)
            //    CheckIconPath(Icon_Base_Path_2 + service.OverHeadIcon+".png", service.Id, "OverHeadIcon", LuaDataCheckType.Service);
        }

        //
        count = 0;
        total = _MapData.Values.Count;
        foreach (var map in _MapData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(map)： {0}", map.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(map.MapIcon))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + map.MapIcon + ".png", map.Id, "MapIcon", LuaDataCheckType.Map);
        }

        //
        count = 0;
        total = _RuneData.Values.Count;
        foreach (var rune in _RuneData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(map)： {0}", rune.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(rune.RuneIcon))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + rune.RuneIcon + ".png", rune.Id, "RuneIcon", LuaDataCheckType.Rune);
            if (!string.IsNullOrEmpty(rune.RuneSmallIcon))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + rune.RuneSmallIcon + ".png", rune.Id, "RuneSmallIcon", LuaDataCheckType.Rune);
        }

        //
        count = 0;
        total = _TalentData.Values.Count;
        foreach (var talent in _TalentData.Values)
        {
            ++count;
            GameDataCheckMan.Instance.SetDesc(string.Format("检查图标(map)： {0}", talent.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            if (!string.IsNullOrEmpty(talent.Icon))
                CheckIconPath(LuaDataCheck.Icon_Base_Path_2 + talent.Icon + ".png", talent.Id, "Icon", LuaDataCheckType.Talent);
        }

        yield return null;
    }

    public void ShowDataCheckInfo()
    {
        _ErrorString = "";

        if (_ModelCheckInfo.Count != 0 || _IconCheckInfo.Count != 0)
        {
            _ErrorString += Model_Title + "\n";
            foreach(var kv in _ModelCheckInfo)
            {
                _ErrorString += string.Format("{0} 模型检查错误!\n", GetTypeDes(kv.Key));
                var infoList = kv.Value;

                foreach (var info in infoList)
                {
                    if (info.assetid == 0)
                        _ErrorString += string.Format("\ttid: {0}, property: {1}\n", info.tid, info.property);
                    else
                        _ErrorString += string.Format("\ttid: {0}, property: {1}, assetId: {2}\n", info.tid, info.property, info.assetid);

                    if (string.IsNullOrEmpty(info.errorPath))
                        _ErrorString += string.Format("\t\tlog: {0}\n", info.log);
                    else
                        _ErrorString += string.Format("\t\tlog: {0}, errorPath: {1}\n", info.log, info.errorPath);

                    _ErrorString += "\n";
                }
            }

            foreach (var kv in _IconCheckInfo)
            {
                _ErrorString += string.Format("{0} 图标检查错误!\n", GetTypeDes(kv.Key));
                var infoList = kv.Value;

                foreach (var info in infoList)
                {
                    if (info.assetid == 0)
                        _ErrorString += string.Format("\ttid: {0}, property: {1}\n", info.tid, info.property);
                    else
                        _ErrorString += string.Format("\ttid: {0}, property: {1}, assetId: {2}\n", info.tid, info.property, info.assetid);

                    if (string.IsNullOrEmpty(info.errorPath))
                        _ErrorString += string.Format("\t\tlog: {0}\n", info.log);
                    else
                        _ErrorString += string.Format("\t\tlog: {0}, errorPath: {1}\n", info.log, info.errorPath);
                    _ErrorString += "\n";
                }
            }
        }
    }

    public string _ErrorString = "";
    public void LogReport(string filename)
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
            Debug.Log("AssetBundleCheck检查通过!");
        }
        else
        {
            TextLogger.Instance.WriteLine(_ErrorString);
        }
    }

    private void CheckModelAsset()
    {
        foreach (var monster in _MonsterData.Values)
        {
            if (!string.IsNullOrEmpty(monster.ModelAssetPath))
                CheckModelAsset(monster.ModelAssetPath, monster.Id, "ModelAssetPath", LuaDataCheckType.Monster);
            if (!string.IsNullOrEmpty(monster.WeaponAssetPath))
                CheckModelAsset(monster.WeaponAssetPath, monster.Id, "WeaponAssetPath", LuaDataCheckType.Monster);
        }
        foreach (var npc in _NpcData.Values)
        {
            if (!string.IsNullOrEmpty(npc.ModelAssetPath))
                CheckModelAsset(npc.ModelAssetPath, npc.Id, "ModelAssetPath", LuaDataCheckType.Npc);
            if (!string.IsNullOrEmpty(npc.WeaponAssetPath))
                CheckModelAsset(npc.WeaponAssetPath, npc.Id, "WeaponAssetPath", LuaDataCheckType.Npc);
        }
        foreach (var obstacle in _ObstacleData.Values)
        {
            if (!string.IsNullOrEmpty(obstacle.ModelAssetPath))
                CheckModelAsset(obstacle.ModelAssetPath, obstacle.Id, "ModelAssetPath", LuaDataCheckType.Obstacle);
        }
        foreach (var mine in _MineData.Values)
        {
            if (!string.IsNullOrEmpty(mine.ModelAssetPath))
                CheckModelAsset(mine.ModelAssetPath, mine.Id, "ModelAssetPath", LuaDataCheckType.Mine);
            if (!string.IsNullOrEmpty(mine.EffectAssetPath))
                CheckModelAsset(mine.EffectAssetPath, mine.Id, "EffectAssetPath", LuaDataCheckType.Mine);
            if (!string.IsNullOrEmpty(mine.AudioAssetPath))
                CheckModelAsset(mine.AudioAssetPath, mine.Id, "AudioAssetPath", LuaDataCheckType.Mine);
            if (!string.IsNullOrEmpty(mine.DisappearAssetPath))
                CheckModelAsset(mine.DisappearAssetPath, mine.Id, "DisappearAssetPath", LuaDataCheckType.Mine);
        }
        foreach (var profession in _ProfessionData.Values)
        {
            if (!string.IsNullOrEmpty(profession.FemaleModelAssetPath))
                CheckModelAsset(profession.FemaleModelAssetPath, profession.Id, "FemaleModelAssetPath", LuaDataCheckType.Profession);
            else
                CheckModelAsset(profession.MaleModelAssetPath, profession.Id, "MaleModelAssetPath", LuaDataCheckType.Profession);
//             if (!string.IsNullOrEmpty(profession.FemaleWeaponId))
//                 CheckModelAsset(profession.FemaleWeaponId, profession.Id, "FemaleWeaponId", LuaDataCheckType.Profession);
//             else
//                 CheckModelAsset(profession.MaleWeaponId, profession.Id, "MaleWeaponId", LuaDataCheckType.Profession);
        }
        foreach (var item in _ItemData.Values)
        {
            if (!string.IsNullOrEmpty(item.LootModelAssetPath))
                CheckModelAsset(item.LootModelAssetPath, item.Id, "LootModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.MaleModelAssetPath))
                CheckModelAsset(item.MaleModelAssetPath, item.Id, "MaleModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.FemaleModelAssetPath))
                CheckModelAsset(item.FemaleModelAssetPath, item.Id, "FemaleModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.MaleLeftHandModelAssetPath))
                CheckModelAsset(item.MaleLeftHandModelAssetPath, item.Id, "MaleLeftHandModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.FemaleLeftHandModelAssetPath))
                CheckModelAsset(item.FemaleLeftHandModelAssetPath, item.Id, "FemaleLeftHandModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.MaleRightHandModelAssetPath))
                CheckModelAsset(item.MaleRightHandModelAssetPath, item.Id, "MaleRightHandModelAssetPath", LuaDataCheckType.Item);
            if (!string.IsNullOrEmpty(item.FemaleRightHandModelAssetPath))
                CheckModelAsset(item.FemaleRightHandModelAssetPath, item.Id, "FemaleRightHandModelAssetPath", LuaDataCheckType.Item);
        }
        foreach (var horse in _HorseData.Values)
        {
            if (!string.IsNullOrEmpty(horse.ModelAssetPath))
                CheckModelAsset(horse.ModelAssetPath, horse.Id, "ModelAssetPath", LuaDataCheckType.Horse);
        }
        foreach (var pet in _PetData.Values)
        {
            //if (!string.IsNullOrEmpty(pet.ModelAssetPath))
            //    CheckModelAsset(pet.ModelAssetPath, pet.Id, "ModelAssetPath", LuaDataCheckType.Pet);
        }
        foreach (var wing in _WingData.Values)
        {
            if (!string.IsNullOrEmpty(wing.ModelAssetPath))
                CheckModelAsset(wing.ModelAssetPath, wing.Id, "ModelAssetPath", LuaDataCheckType.Wing);
        }
        foreach (var wingTalentPage in _WingTalentPageData.Values)
        {
            if (!string.IsNullOrEmpty(wingTalentPage.ModelAssetPath))
                CheckModelAsset(wingTalentPage.ModelAssetPath, wingTalentPage.Id, "ModelAssetPath", LuaDataCheckType.WingTalentPage);
        }
        foreach (var dress in _DressData.Values)
        {
            if (!string.IsNullOrEmpty(dress.AssetPath1))
                CheckModelAsset(dress.AssetPath1, dress.Id, "AssetPath1", LuaDataCheckType.Dress);
            if (!string.IsNullOrEmpty(dress.AssetPath2))
                CheckModelAsset(dress.AssetPath2, dress.Id, "AssetPath2", LuaDataCheckType.Dress);
        }
        foreach (var dyeAndEmbroidery in _DyeAndEmbroideryData.Values)
        {
            //if (!string.IsNullOrEmpty(dyeAndEmbroidery.EmbroideryAssetPath))
             //   CheckModelAsset(dyeAndEmbroidery.EmbroideryAssetPath, dyeAndEmbroidery.Id, "EmbroideryAssetPath", LuaDataCheckType.DyeAndEmbroidery);
        }
        foreach (var executionUnit in _ExecutionUnitData.Values)
        {
            if (executionUnit.Event.Transform != null)
            {
                if (!string.IsNullOrEmpty(executionUnit.Event.Transform.BodyAssetPath))
                    CheckModelAsset(executionUnit.Event.Transform.BodyAssetPath, executionUnit.Id, "BodyAssetPath", LuaDataCheckType.ExecutionUnit);
                if (!string.IsNullOrEmpty(executionUnit.Event.Transform.HeadAssetPath))
                    CheckModelAsset(executionUnit.Event.Transform.HeadAssetPath, executionUnit.Id, "HeadAssetPath", LuaDataCheckType.ExecutionUnit);
                if (!string.IsNullOrEmpty(executionUnit.Event.Transform.HairAssetPath))
                    CheckModelAsset(executionUnit.Event.Transform.HairAssetPath, executionUnit.Id, "HairAssetPath", LuaDataCheckType.ExecutionUnit);
            }
            if (executionUnit.Event.Audio != null)
            {
                if (!string.IsNullOrEmpty(executionUnit.Event.Audio.AssetPath))
                    CheckModelAsset(executionUnit.Event.Audio.AssetPath, executionUnit.Id, "AssetPath", LuaDataCheckType.ExecutionUnit);
            }
        }
        foreach (var scene in _SceneData.Values)
        {
            if (!string.IsNullOrEmpty(scene.AssetPath))
                CheckModelAsset(scene.AssetPath, scene.Id, "AssetPath", LuaDataCheckType.Scene);
            for (int i = 0; i < scene.RegionRoot.Regions.Count; i++)
            {
                if (!string.IsNullOrEmpty(scene.RegionRoot.Regions[i].CGAssetPath))
                    CheckModelAsset(scene.RegionRoot.Regions[i].CGAssetPath, scene.Id, "CGAssetPath", LuaDataCheckType.Scene);
            }
        }
        foreach (var map in _MapData.Values)
        {
            if (!string.IsNullOrEmpty(map.CGAssetPath))
                CheckModelAsset(map.CGAssetPath, map.Id, "CGAssetPath", LuaDataCheckType.Map);
        }
        foreach (var actor in _ActorData.Values)
        {
            if (!string.IsNullOrEmpty(actor.GfxAssetPath))
                CheckModelAsset(actor.GfxAssetPath, actor.Id, "GfxAssetPath", LuaDataCheckType.Actor);
        }
        //foreach (var worldBossConfig in _WorldBossConfigData.Values)
        //{
        //    if (!string.IsNullOrEmpty(worldBossConfig.ModelAssetPath))
        //        CheckModelAsset(worldBossConfig.ModelAssetPath, worldBossConfig.Id, "ModelAssetPath", LuaDataCheckType.WorldBossConfig);
        //}
        foreach (var teamRoomConfig in _TeamRoomConfigData.Values)
        {
            if (!string.IsNullOrEmpty(teamRoomConfig.Model))
            {
                CheckModelAsset(teamRoomConfig.Model, teamRoomConfig.Id, "Model", LuaDataCheckType.TeamRoomConfig);
            }
        }
    }

    private void CheckModelAsset(string path, int templateTid, string property, LuaDataCheckType type)
    {
        if (path == "0")
            return;

        if (!AssetBundleCheck.Instance.IsValidPath(path))
        {
            LuaDataCheckInfo ldci = new LuaDataCheckInfo();
            ldci.title = Model_Title;
            ldci.assetid = 0;
            ldci.tid = templateTid;
            ldci.property = property;
            ldci.log = Log_2;
            ldci.errorPath = path;

            if (!_ModelCheckInfo.ContainsKey(type))
                _ModelCheckInfo.Add(type, new List<LuaDataCheckInfo>());
            _ModelCheckInfo[type].Add(ldci);
        }
    }

    /// <summary>
    /// 注意：所有路径都是不确定的
    /// 策划或者程序会随时改动
    /// 此处路径最好划定一致规则
    /// </summary>
    private void CheckIconPath()
    {
        foreach (var monster in _MonsterData.Values)
        {
            if (monster.IconAtlasPath != string.Empty)
            {
                //TODO:路径不确定
            }
        }
        foreach (var npc in _NpcData.Values)
        {
            if (npc.IconAtlasPath != string.Empty)
            {
                //TODO:路径不确定
            }
        }
        foreach (var scene in _SceneData.Values)
        {
            if (!string.IsNullOrEmpty(scene.MiniMapAtlasPath))
                CheckIconPath(scene.MiniMapAtlasPath, scene.Id, "MiniMapAtlasPath", LuaDataCheckType.Scene);
        }
        foreach (var instance in _InstanceData.Values)
        {
            if (!string.IsNullOrEmpty(instance.IconPath))
                CheckIconPath(Icon_Base_Path_0 + instance.IconPath, instance.Id, "IconPath", LuaDataCheckType.Instance);
        }
        foreach (var item in _ItemData.Values)
        {
            if (!string.IsNullOrEmpty(item.IconAtlasPath))
                CheckIconPath(Icon_Base_Path_2 + item.IconAtlasPath + ".png", item.Id, "IconAtlasPath", LuaDataCheckType.Item);

        }
        foreach (var horse in _HorseData.Values)
        {
            if (!string.IsNullOrEmpty(horse.IconPath))
                CheckIconPath(Icon_Base_Path_2 + horse.IconPath + ".png", horse.Id, "IconPath", LuaDataCheckType.Horse);
        }
        foreach (var pet in _PetData.Values)
        {
            if (!string.IsNullOrEmpty(pet.IconPath))
                CheckIconPath(Icon_Base_Path_2 + pet.IconPath + ".png", pet.Id, "IconPath", LuaDataCheckType.Pet);
        }
        foreach (var achievement in _AchievementData.Values)
        {
            //TODO:路径不确定
            //if (achievement.IconPath != string.Empty)
            //    CheckIconPath(Icon_Base_Path_2 + achievement.IconPath+".png", achievement.Id, "", LuaDataCheckType.Achievement);
        }
        foreach (var designation in _DesignationData.Values)
        {
            //TODO:路径不确定
        }
        foreach (var fun in _FunData.Values)
        {
            //TODO:路径不确定
            //if (fun.IconPath != string.Empty)
            //    CheckIconPath(Icon_Base_Path_2 + "main/" + fun.IconPath + ".png", fun.Id, "IconPath", LuaDataCheckType.Fun);
        }
        foreach (var wing in _WingData.Values)
        {
            if (!string.IsNullOrEmpty(wing.IconPath))
                CheckIconPath(Icon_Base_Path_2 + wing.IconPath + ".png", wing.Id, "IconPath", LuaDataCheckType.Wing);
        }
        foreach (var dress in _DressData.Values)
        {
            if (!string.IsNullOrEmpty(dress.IconPath))
                CheckIconPath(Icon_Base_Path_2 + dress.IconPath + ".png", dress.Id, "IconPath", LuaDataCheckType.Dress);
        }
        foreach (var dyeAndEmbroidery in _DyeAndEmbroideryData.Values)
        {
            if (!string.IsNullOrEmpty(dyeAndEmbroidery.IconPath))
                CheckIconPath(Icon_Base_Path_2 + dyeAndEmbroidery.IconPath + ".png", dyeAndEmbroidery.Id, "IconPath", LuaDataCheckType.DyeAndEmbroidery);
        }
        foreach (var manualEntrie in _ManualEntrieData.Values)
        {
            //TODO:路径不确定
            //if (manualEntrie.IconPath != string.Empty)
            //    CheckIconPath(Icon_Base_Path_2 + manualEntrie.IconPath+".png", manualEntrie.Id, "IconPath", LuaDataCheckType.ManualEntrie);
        }
        foreach (var money in _MoneyData.Values)
        {
            if (!string.IsNullOrEmpty(money.IconPath))
                CheckIconPath(Icon_Base_Path_2 + money.IconPath + ".png", money.Id, "IconPath", LuaDataCheckType.Money);
        }
        foreach (var reputation in _ReputationData.Values)
        {
            if (!string.IsNullOrEmpty(reputation.IconAtlasPath))
                CheckIconPath(Icon_Base_Path_2 + reputation.IconAtlasPath + ".png", reputation.Id, "IconAtlasPath", LuaDataCheckType.Reputation);
        }
        foreach (var skill in _SkillData.Values)
        {
            if (!string.IsNullOrEmpty(skill.IconName))
                CheckIconPath(Icon_Base_Path_2 + skill.IconName + ".png", skill.Id, "IconName", LuaDataCheckType.Skill);
        }
        foreach (var executionUnit in _ExecutionUnitData.Values)
        {
            if (executionUnit.Event.ChangeSkillIcon != null)
            {
                if (!string.IsNullOrEmpty(executionUnit.Event.ChangeSkillIcon.IconPath))
                    CheckIconPath(Icon_Base_Path_2 + executionUnit.Event.ChangeSkillIcon.IconPath + ".png", executionUnit.Id, "IconPath", LuaDataCheckType.ExecutionUnit);
            }
        }
        foreach (var state in _StateData.Values)
        {
            if (!string.IsNullOrEmpty(state.IconPath))
                CheckIconPath(Icon_Base_Path_2 + state.IconPath + ".png", state.Id, "IconPath", LuaDataCheckType.State);
        }
        foreach (var profession in _ProfessionData.Values)
        {
            if (!string.IsNullOrEmpty(profession.SymbolAtlasPath))
                CheckIconPath(Icon_Base_Path_2 + profession.SymbolAtlasPath + ".png", profession.Id, "SymbolAtlasPath", LuaDataCheckType.Profession);
            if (!string.IsNullOrEmpty(profession.MaleIconAtlasPath))
                CheckIconPath(Icon_Base_Path_2 + profession.MaleIconAtlasPath + ".png", profession.Id, "MaleIconAtlasPath", LuaDataCheckType.Profession);
            if (!string.IsNullOrEmpty(profession.FemaleIconAtlasPath))
                CheckIconPath(Icon_Base_Path_2 + profession.FemaleIconAtlasPath + ".png", profession.Id, "FemaleIconAtlasPath", LuaDataCheckType.Profession);
        }
        foreach (var worldBossConfig in _WorldBossConfigData.Values)
        {
//             if (!string.IsNullOrEmpty(worldBossConfig.Icon))
//                 CheckIconPath(Icon_Base_Path_2 + "Head/" + worldBossConfig.Icon + ".png", worldBossConfig.Id, "Icon", LuaDataCheckType.WorldBossConfig);
        }
        foreach (var service in _ServiceData.Values)
        {
            //TODO:路径不确定
            //if (service.OverHeadIcon != string.Empty)
            //    CheckIconPath(Icon_Base_Path_2 + service.OverHeadIcon+".png", service.Id, "OverHeadIcon", LuaDataCheckType.Service);
        }
        foreach (var map in _MapData.Values)
        {
            if (!string.IsNullOrEmpty(map.MapIcon))
                CheckIconPath(Icon_Base_Path_2 + map.MapIcon + ".png", map.Id, "MapIcon", LuaDataCheckType.Map);
        }
        foreach (var rune in _RuneData.Values)
        {
            if (!string.IsNullOrEmpty(rune.RuneIcon))
                CheckIconPath(Icon_Base_Path_2 + rune.RuneIcon + ".png", rune.Id, "RuneIcon", LuaDataCheckType.Rune);
            if (!string.IsNullOrEmpty(rune.RuneSmallIcon))
                CheckIconPath(Icon_Base_Path_2 + rune.RuneSmallIcon + ".png", rune.Id, "RuneSmallIcon", LuaDataCheckType.Rune);
        }
        foreach (var talent in _TalentData.Values)
        {
            if (!string.IsNullOrEmpty(talent.Icon))
                CheckIconPath(Icon_Base_Path_2 + talent.Icon + ".png", talent.Id, "Icon", LuaDataCheckType.Talent);
        }
    }

    private void CheckIconPath(string path, int templateTid, string property, LuaDataCheckType type)
    {
        Object asset = AssetBundleCheck.Instance.LoadAsset(path);
        if (asset == null)
        {
            LuaDataCheckInfo ldci = new LuaDataCheckInfo();
            ldci.title = Icon_Title;
            ldci.assetid = 0;
            ldci.tid = templateTid;
            ldci.property = property;
            ldci.log = Log_3;
            ldci.errorPath = path;

            if (!_IconCheckInfo.ContainsKey(type))
                _IconCheckInfo.Add(type, new List<LuaDataCheckInfo>());
            _IconCheckInfo[type].Add(ldci);
        }
    }

    private string GetTypeDes(LuaDataCheckType type)
    {
        string typeDes = string.Empty;
        switch (type)
        {
            case LuaDataCheckType.Monster:
                typeDes = Monster_Type;
                break;
            case LuaDataCheckType.Npc:
                typeDes = Npc_Type;
                break;
            case LuaDataCheckType.Obstacle:
                typeDes = Obstacle_Type;
                break;
            case LuaDataCheckType.Mine:
                typeDes = Mine_Type;
                break;
            case LuaDataCheckType.Profession:
                typeDes = Profession_Type;
                break;
            case LuaDataCheckType.Item:
                typeDes = Item_Type;
                break;
            case LuaDataCheckType.Horse:
                typeDes = Horse_Type;
                break;
            case LuaDataCheckType.Pet:
                typeDes = Pet_Type;
                break;
            case LuaDataCheckType.Wing:
                typeDes = Wing_Type;
                break;
            case LuaDataCheckType.WingTalentPage:
                typeDes = WingTalentPage_Type;
                break;
            case LuaDataCheckType.Money:
                typeDes = Money_Type;
                break;
            case LuaDataCheckType.Dress:
                typeDes = Dress_Type;
                break;
            case LuaDataCheckType.DyeAndEmbroidery:
                typeDes = DyeAndEmbroidery_Type;
                break;
            case LuaDataCheckType.ExecutionUnit:
                typeDes = ExecutionUnit_Type;
                break;
            case LuaDataCheckType.Scene:
                typeDes = Scene_Type;
                break;
            case LuaDataCheckType.Map:
                typeDes = Map_Type;
                break;
            case LuaDataCheckType.Instance:
                typeDes = Instance_Type;
                break;
            case LuaDataCheckType.Achievement:
                typeDes = Achievement_Type;
                break;
            case LuaDataCheckType.Designation:
                typeDes = Designation_Type;
                break;
            case LuaDataCheckType.ActivityContent:
                typeDes = ActivityContent_Type;
                break;
            case LuaDataCheckType.Fun:
                typeDes = Fun_Type;
                break;
            case LuaDataCheckType.ColorConfig:
                typeDes = ColorConfig_Type;
                break;
            case LuaDataCheckType.ManualEntrie:
                typeDes = ManualEntrie_Type;
                break;
            case LuaDataCheckType.Reputation:
                typeDes = Reputation_Type;
                break;
            case LuaDataCheckType.Skill:
                typeDes = Skill_Type;
                break;
            case LuaDataCheckType.Actor:
                typeDes = Actor_Type;
                break;
            case LuaDataCheckType.State:
                typeDes = State_Type;
                break;
            case LuaDataCheckType.WorldBossConfig:
                typeDes = WorldBossConfig_Type;
                break;
            case LuaDataCheckType.TeamRoomConfig:
                typeDes = TeamRoomConfig_Type;
                break;
            case LuaDataCheckType.Service:
                typeDes = Service_Type;
                break;
            case LuaDataCheckType.Rune:
                typeDes = Rune_Type;
                break;
            case LuaDataCheckType.Talent:
                typeDes = Talent_Type;
                break;
        }
        return typeDes;
    }

}

public class LuaDataCheckInfo
{
    public string title = string.Empty;
    public int tid = 0;
    public int assetid = 0;
    public string property = string.Empty;
    public string log = string.Empty;
    public string errorPath = string.Empty;
}

public enum LuaDataCheckType
{
    None = 0,
    Monster = 1,
    Npc = 2,
    Obstacle = 3,
    Mine = 4,
    Profession = 5,
    Item = 6,
    Horse = 7,
    Pet = 8,
    Wing = 9,
    WingTalentPage = 10,
    Money = 11,
    Dress = 12,
    DyeAndEmbroidery = 13,
    ExecutionUnit = 14,
    Scene = 15,
    Map = 16,
    Instance = 17,
    Achievement = 18,
    Designation = 19,
    ActivityContent = 20,
    Fun = 21,
    ColorConfig = 22,
    ManualEntrie = 23,
    Reputation = 24,
    Skill = 25,
    Actor = 26,
    State = 27,
    WorldBossConfig = 28,
    TeamRoomConfig = 29,
    Service = 30,
    Rune = 31,
    Talent = 32,
}


