using System;
using Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Template;
using Object = UnityEngine.Object;


public enum QuestNpcType
{
    Provide = 0,
    Deliver,
    Objectives_Conversation,
    Objectives_FinishDungeon,
    Objectives_UseItem,
    Objectives_HoldItem,
    Objectives_EnterDungeon,
    Events_AssistNpc,
    Events_ChatBubble,
    AppearHide,
}

public enum QuestMonsterType
{
    Objectives_KillMonster = 0,
    Objectives_FinishDungeon,
    Objectives_UseItem,
    Objectives_HoldItem,
    Objectives_EnterDungeon,
    Events_RefMonster,
    Events_RegionRefMonster,
}

public enum QuestMineType
{
    Objectives_Gather = 0,
    Objectives_FinishDungeon,
    Objectives_UseItem,
    Objectives_HoldItem,
    Objectives_EnterDungeon,
}

public enum ServiceType
{
	ServiceType_Conversation,	//NPC对话
}

public enum EntityType
{
    Role = 0,
    Monster = 1,
    Npc = 2,
    Subobject = 3,
    Obstacle = 4,
}

public partial class LuaQuestCheck : Singleton<LuaQuestCheck>
{
    public string _ErrorString = "";

    public Dictionary<int, Quest> _QuestData = new Dictionary<int, Quest>();
    public Dictionary<int, Npc> _NpcData = new Dictionary<int, Npc>();
    public Dictionary<int, Monster> _MonsterData = new Dictionary<int, Monster>();
    public Dictionary<int, Mine> _MineData = new Dictionary<int, Mine>();
    public Dictionary<int, Scene> _SceneData = new Dictionary<int, Scene>();
	public Dictionary<int, Service> _ServiceData = new Dictionary<int, Service>();

    public void Init()
    {
        //预先设置路径
        Template.Path.BasePath = System.IO.Path.Combine(Application.dataPath, "../../GameRes/");
        Template.Path.BinPath = "Data/";
        //初始化数据存储

        var questManager = Template.QuestModule.Manager.Instance;
        questManager.ParseTemplateAll(true);
        _QuestData = questManager.GetTemplateMap();

        var npcManager = Template.NpcModule.Manager.Instance;
        npcManager.ParseTemplateAll(true);
        _NpcData = npcManager.GetTemplateMap();

        var monsterManager = Template.MonsterModule.Manager.Instance;
        monsterManager.ParseTemplateAll(true);
        _MonsterData = monsterManager.GetTemplateMap();

        var mineManager = Template.MineModule.Manager.Instance;
        mineManager.ParseTemplateAll(true);
        _MineData = mineManager.GetTemplateMap();

		var serviceManager = Template.ServiceModule.Manager.Instance;
		serviceManager.ParseTemplateAll(true);
		_ServiceData = serviceManager.GetTemplateMap();
	}

    public IEnumerable CheckQuestNpcCoroutine()
    {
        int count = 0;
        int total = _QuestData.Count;
        foreach (var quest in _QuestData.Values)
        {
            if (quest.Name.Contains("作废") || quest.Name.Contains("废弃"))
                continue;

            ++count;

            GameDataCheckMan.Instance.SetDesc(string.Format("检查任务NPC: {0}, 任务: {1}", quest.Id, quest.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            int id = quest.Id;
            if (quest.ProvideRelated != null &&
                quest.ProvideRelated.ProvideMode != null &&
                quest.ProvideRelated.ProvideMode.ViaNpc != null)
            {
                CheckNPC(quest, quest.ProvideRelated.ProvideMode.ViaNpc.NpcId, QuestNpcType.Provide, 0, quest.Type == Template.Quest.QuestType.Main);
            }

            for (int i = 0; i < quest.ObjectiveRelated.QuestObjectives.Count; i++)
            {
                if (quest.ObjectiveRelated.QuestObjectives[i].Conversation != null)
                {
                    CheckNPC(quest, quest.ObjectiveRelated.QuestObjectives[i].Conversation.NpcId, QuestNpcType.Objectives_Conversation, i, false);
                    CheckNpcService(quest, quest.ObjectiveRelated.QuestObjectives[i].Conversation.NpcId, ServiceType.ServiceType_Conversation, quest.ObjectiveRelated.QuestObjectives[i].Conversation.DialogueId);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon.PathType == 1)
                {
                    CheckNPC(quest, quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon.PathID, QuestNpcType.Objectives_FinishDungeon, i, false);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].UseItem != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].UseItem.PathType == 1)
                {
                    CheckNPC(quest, quest.ObjectiveRelated.QuestObjectives[i].UseItem.PathID, QuestNpcType.Objectives_UseItem, i, false);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].HoldItem != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].HoldItem.PathType == 1)
                {
                    CheckNPC(quest, quest.ObjectiveRelated.QuestObjectives[i].HoldItem.PathID, QuestNpcType.Objectives_HoldItem, i, false);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon.PathType == 1)
                {
                    CheckNPC(quest, quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon.PathID, QuestNpcType.Objectives_EnterDungeon, i, false);
                }
            }
            if (quest.DeliverRelated != null &&
                quest.DeliverRelated.ViaNpc != null)
            {
                CheckNPC(quest, quest.DeliverRelated.ViaNpc.NpcId, QuestNpcType.Deliver, 0, quest.Type == Template.Quest.QuestType.Main);
            }
            for (int i = 0; i < quest.EventRelated.QuestEvents.Count; i++)
            {
                if (quest.EventRelated.QuestEvents[i].AssistNpc != null)
                {
                    CheckNPC(quest, quest.EventRelated.QuestEvents[i].AssistNpc.NpcTID, QuestNpcType.Events_AssistNpc, 0, false);
                }
                if (quest.EventRelated.QuestEvents[i].ChatBubble != null)
                {
                    CheckNPC(quest, quest.EventRelated.QuestEvents[i].ChatBubble.NpcTID, QuestNpcType.Events_ChatBubble, 0, false);
                }
            }
            for (int i = 0; i < quest.NpcAppearHideRelated.QuestAppearHideNpcs.Count; i++)
            {
                for (int j = 0; j < quest.NpcAppearHideRelated.QuestAppearHideNpcs[i].NpcIds.Count; j++)
                {
                    CheckNPC(quest, quest.NpcAppearHideRelated.QuestAppearHideNpcs[i].NpcIds[j].Id, QuestNpcType.AppearHide, i, false);
                }
            }
        }

        yield return null;
    }

    public IEnumerable CheckQuestMineCoroutine()
    {
        int count = 0;
        int total = LuaQuestCheck.Instance._QuestData.Count;
        foreach (var quest in LuaQuestCheck.Instance._QuestData.Values)
        {
            if (quest.Name.Contains("作废") || quest.Name.Contains("废弃"))
                continue;

            ++count;

            GameDataCheckMan.Instance.SetDesc(string.Format("检查任务Mine: {0}, 任务: {1}", quest.Id, quest.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            for (int i = 0; i < quest.ObjectiveRelated.QuestObjectives.Count; i++)
            {
                if (quest.ObjectiveRelated.QuestObjectives[i].Gather != null)
                {
                    CheckMine(quest, quest.ObjectiveRelated.QuestObjectives[i].Gather.MineId, QuestMineType.Objectives_Gather, i);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon.PathType == 2)
                {
                    CheckMine(quest, quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon.PathID, QuestMineType.Objectives_FinishDungeon, i);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].UseItem != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].UseItem.PathType == 2)
                {
                    CheckMine(quest, quest.ObjectiveRelated.QuestObjectives[i].UseItem.PathID, QuestMineType.Objectives_UseItem, i);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].HoldItem != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].HoldItem.PathType == 2)
                {
                    CheckMine(quest, quest.ObjectiveRelated.QuestObjectives[i].HoldItem.PathID, QuestMineType.Objectives_HoldItem, i);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon.PathType == 2)
                {
                    CheckMine(quest, quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon.PathID, QuestMineType.Objectives_EnterDungeon, i);
                }
            }
        }

        yield return null;
    }

    public IEnumerable CheckQuestMonsterCoroutine()
    {
        int count = 0;
        int total = LuaQuestCheck.Instance._QuestData.Count;
        foreach (var quest in LuaQuestCheck.Instance._QuestData.Values)
        {
            if (quest.Name.Contains("作废") || quest.Name.Contains("废弃"))
                continue;

            ++count;

            GameDataCheckMan.Instance.SetDesc(string.Format("检查任务Monster: {0}, 任务: {1}", quest.Id, quest.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            for (int i = 0; i < quest.ObjectiveRelated.QuestObjectives.Count; i++)
            {
                if (quest.ObjectiveRelated.QuestObjectives[i].KillMonster != null)
                {
                    CheckMonster(quest, quest.ObjectiveRelated.QuestObjectives[i].KillMonster.MonsterId, QuestMonsterType.Objectives_KillMonster, i);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon.PathType == 0)
                {
                    CheckMonster(quest, quest.ObjectiveRelated.QuestObjectives[i].FinishDungeon.PathID, QuestMonsterType.Objectives_FinishDungeon, i);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].UseItem != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].UseItem.PathType == 0)
                {
                    CheckMonster(quest, quest.ObjectiveRelated.QuestObjectives[i].UseItem.PathID, QuestMonsterType.Objectives_UseItem, i);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].HoldItem != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].HoldItem.PathType == 0)
                {
                    CheckMonster(quest, quest.ObjectiveRelated.QuestObjectives[i].HoldItem.PathID, QuestMonsterType.Objectives_HoldItem, i);
                }
                if (quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon != null &&
                    quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon.PathType == 0)
                {
                    CheckMonster(quest, quest.ObjectiveRelated.QuestObjectives[i].EnterDungeon.PathID, QuestMonsterType.Objectives_EnterDungeon, i);
                }
            }

            for (int i = 0; i < quest.EventRelated.QuestEvents.Count; i++)
            {
                if (quest.EventRelated.QuestEvents[i].RefMonster != null)
                {
                    CheckEntityGenerator(quest, quest.EventRelated.QuestEvents[i].RefMonster.MapID, quest.EventRelated.QuestEvents[i].RefMonster.GeneratorID);
                }
                if (quest.EventRelated.QuestEvents[i].RegionRefMonster != null)
                {
                    CheckEntityGenerator(quest, quest.EventRelated.QuestEvents[i].RegionRefMonster.MapID, quest.EventRelated.QuestEvents[i].RegionRefMonster.GeneratorID);
                }
            }
        }

        yield return null;
    }

    public IEnumerable CheckQuestMainCoroutine()
    {
        int count = 0;
        int total = LuaQuestCheck.Instance._QuestData.Count;
        foreach (var quest in LuaQuestCheck.Instance._QuestData.Values)
        {
            if (quest.Name.Contains("作废") || quest.Name.Contains("废弃"))
                continue;

            ++count;

            GameDataCheckMan.Instance.SetDesc(string.Format("检查主任务: {0}, 任务: {1}", quest.Id, quest.Name));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            LuaQuestCheck.Instance.CheckMainQuest(quest);
        }

        yield return null;
    }

    //现在客户端逻辑是只有接主任务和交主任务的npc才会用Npc摄像机
    private void CheckNPC(Quest quest, int npcTid, QuestNpcType questNpcType, int index, bool bCheckNpcCamera)
    {
        int questTid = quest.Id;
        if (npcTid == 0)
        {
            if (questNpcType == QuestNpcType.Provide || questNpcType == QuestNpcType.Deliver)
                _ErrorString += string.Format("任务npc不存在! questTid: {0}, questName: {1}, npcTid: {2}, 类型: {3}, 索引: {4}\n", questTid, quest.Name, npcTid, questNpcType, index);
            else
                return;             //其他类型npc的id为0时暂时跳过
        }

        if (_NpcData.ContainsKey(npcTid))
        {
            Npc npc = _NpcData[npcTid];
            GameObject go = AssetBundleCheck.Instance.LoadAsset(npc.ModelAssetPath) as GameObject;
            if (go == null)
            {
                _ErrorString += string.Format("模型为空! questTid: {0}, questName: {1}, npcTid: {2}, npcPath: {3}, 类型: {4}, 索引: {5}\n", questTid, quest.Name, npcTid, npc.ModelAssetPath, questNpcType, index);
            }
            else
            {
                if (bCheckNpcCamera)
                {
                    HangPointHolder hangHolder = go.GetComponentInChildren<HangPointHolder>();
                    if (hangHolder != null)
                    {
                        GameObject g_camPos = hangHolder.GetHangPoint(HangPointHolder.HangPointType.DCamPos);
                        GameObject g_camLookAt = hangHolder.GetHangPoint(HangPointHolder.HangPointType.DCamPosLookAt);

                        if (g_camPos == null)
                        {
                            _ErrorString += string.Format("缺少摄像机挂点 HangPoint_DCamPos! questTid: {0}, questName: {1}, npcTid: {2}, npcPath: {3}, 类型: {4}, 索引: {5}\n", questTid, quest.Name, npcTid, npc.ModelAssetPath, questNpcType, index);
                        }
                        if (g_camLookAt == null)
                        {
                            _ErrorString += string.Format("缺少摄像机挂点 HangPoint_DCamPosLookAt! questTid: {0}, questName: {1}, npcTid: {2}, npcPath: {3}, 类型: {4}, 索引: {5}\n", questTid, quest.Name, npcTid, npc.ModelAssetPath, questNpcType, index);
                        }
                    }
                    else
                    {
                        _ErrorString += string.Format("缺少挂载点 HangPointHolder! questTid: {0}, questName: {1}, npcTid: {2}, npcPath: {3}, 类型: {4}, 索引: {5}\n", questTid, quest.Name, npcTid, npc.ModelAssetPath, questNpcType, index);
                    }
                }
            }
        }
        else
        {
            _ErrorString += string.Format("任务npc不存在! questTid: {0}, questName: {1}, npcTid: {2}, 类型: {3}, 索引: {4}\n", questTid, quest.Name, npcTid, questNpcType, index);
        }

        //检查生成点
        foreach (var scene in _SceneData.Values)
        {
            string navmesh = scene.NavMeshName;
            foreach(var generator in scene.EntityGeneratorRoot.EntityGenerators)
            {
                if (generator.EntityType != (int)EntityType.Npc)
                    continue;

                foreach(var entityInfo in generator.EntityInfos)
                {
                    if (npcTid != entityInfo.EntityId)
                        continue;
                    Vector3 pos = new Vector3();
                    pos.x = generator.PositionX;
                    pos.y = generator.PositionY;
                    pos.z = generator.PositionZ;

                    if (!NavMeshManager.Instance.IsValidPositionStrict(Template.Path.BasePath, navmesh, pos))
                    {
                        _ErrorString += string.Format("npc生成点不在navmesh可达点: questTid: {0}, questName: {1}, npcTid: {2}, 类型: {3}, 索引: {4}, 场景Id: {5}, 生成器: {6}\n", questTid, quest.Name, npcTid, questNpcType, index, scene.Id, generator.Id);
                    }
                }
            }
        }
    }

    private void CheckMonster(Quest quest, int monsterTid, QuestMonsterType questMonsterType, int index)
    {
        int questTid = quest.Id;
        if (monsterTid == 0)
        {
            if (questMonsterType == QuestMonsterType.Objectives_KillMonster)
                _ErrorString += string.Format("任务monster不存在! questTid: {0}, questName: {1}, monsterTid: {2}, 类型: {3}, 索引: {4}\n", questTid, quest.Name, monsterTid, questMonsterType, index);
            else
                return;             //其他类型npc的id为0时暂时跳过
        }

        if (_MonsterData.ContainsKey(monsterTid))
        {
            Monster monster = _MonsterData[monsterTid];
            GameObject go = AssetBundleCheck.Instance.LoadAsset(monster.ModelAssetPath) as GameObject;
            if (go == null)
            {
                _ErrorString += string.Format("模型为空! questTid: {0}, questName: {1}, monsterTid: {2}, monsterPath: {3}, 类型: {4}, 索引: {5}\n", questTid, quest.Name, monsterTid, monster.ModelAssetPath, questMonsterType, index);
            }
            else
            {
                GameObject hurtobj = null;
                var holder = go.GetComponent<HangPointHolder>();
                if (holder != null)
                    hurtobj = holder.GetHangPoint(HangPointHolder.HangPointType.Hurt);
                else
                    hurtobj = go.FindChildRecursively("HangPoint_Hurt");

                if (hurtobj == null)
                {
                    _ErrorString += string.Format("任务monster没有HangPoint_Hurt! questTid: {0}, questName: {1}, monsterTid: {2}, monsterPath: {3}, 类型: {4}, 索引: {5}\n", questTid, quest.Name, monsterTid, monster.ModelAssetPath, questMonsterType, index);
                }
            }
        }
        else
        {
            _ErrorString += string.Format("任务monster不存在! questTid: {0}, questName: {1}, monsterTid: {2}, 类型: {3}, 索引: {4}\n", questTid, quest.Name, monsterTid, questMonsterType, index);
        }

        //检查生成点
        foreach (var scene in _SceneData.Values)
        {
            string navmesh = scene.NavMeshName;
            foreach (var generator in scene.EntityGeneratorRoot.EntityGenerators)
            {
                if (generator.EntityType != (int)EntityType.Monster)
                    continue;

                foreach (var entityInfo in generator.EntityInfos)
                {
                    if (monsterTid != entityInfo.EntityId)
                        continue;
                    Vector3 pos = new Vector3();
                    pos.x = generator.PositionX;
                    pos.y = generator.PositionY;
                    pos.z = generator.PositionZ;

                    if (!NavMeshManager.Instance.IsValidPositionStrict(Template.Path.BasePath, navmesh, pos))
                    {
                        _ErrorString += string.Format("monster生成点不在navmesh可达点: questTid: {0}, questName: {1}, npcTid: {2}, 类型: {3}, 索引: {4}, 场景Id: {5}, 生成器: {6}\n", questTid, quest.Name, monsterTid, questMonsterType, index, scene.Id, generator.Id);
                    }
                }
            }
        }
    }

    private void CheckMine(Quest quest, int mineTid, QuestMineType questMineType, int index)
    {
        int questTid = quest.Id;
        if (mineTid == 0)
        {
            if (questMineType == QuestMineType.Objectives_Gather)
                _ErrorString += string.Format("任务mine不存在! questTid: {0}, questName: {1}, mineTid: {2}, 类型: {3}, 索引: {4}\n", questTid, quest.Name, mineTid, questMineType, index);
            else
                return;             //其他类型npc的id为0时暂时跳过
        }

        if (_MineData.ContainsKey(mineTid))
        {
            Mine mine = _MineData[mineTid];
            GameObject go = AssetBundleCheck.Instance.LoadAsset(mine.ModelAssetPath) as GameObject;
            if (go == null)
            {
                _ErrorString += string.Format("模型为空! questTid: {0}, questName: {1}, mineTid: {2}, minePath: {3}, 类型: {4}, 索引: {5}\n", questTid, quest.Name, mineTid, mine.ModelAssetPath, questMineType, index);
            }

			//检查任务专属矿
			//if (mine.QuestId > 0 && mine.QuestId != questTid)
			//{
			//	_ErrorString += string.Format("矿物关联任务ID不正确! questTid: {0}, questName: {1}, mineTid: {2},  mine.QuestId : {3}, 类型: {4}, 索引: {5}\n", questTid, quest.Name, mineTid, mine.QuestId, questMineType, index);

			//}

		}
        else
        {
            _ErrorString += string.Format("任务monster不存在! questTid: {0}, questName: {1}, mineTid: {2}, 类型: {3}, 索引: {4}\n", questTid, quest.Name, mineTid, questMineType, index);
        }

        //检查生成点
        foreach (var scene in _SceneData.Values)
        {
            string navmesh = scene.NavMeshName;
            foreach (var generator in scene.EntityGeneratorRoot.EntityGenerators)
            {
                if (generator.EntityType != (int)EntityType.Monster)
                    continue;

                foreach (var entityInfo in generator.EntityInfos)
                {
                    if (mineTid != entityInfo.EntityId)
                        continue;
                    Vector3 pos = new Vector3();
                    pos.x = generator.PositionX;
                    pos.y = generator.PositionY;
                    pos.z = generator.PositionZ;

                    if (!NavMeshManager.Instance.IsValidPositionStrict(Template.Path.BasePath, navmesh, pos, true, 2.0f))
                    {
                        _ErrorString += string.Format("mine生成点不在navmesh可达点: questTid: {0}, questName: {1}, mineTid: {2}, 类型: {3}, 索引: {4}, 场景Id: {5}, 生成器: {6}\n", questTid, quest.Name, mineTid, questMineType, index, scene.Id, generator.Id);
                    }
                }
            }
        }

	
    }

	//检查NPC是否有某项服务
    private bool CheckNpcService(Quest quest, int npcTid, ServiceType serviceType, int serviceID)
	{
        if (npcTid == 0)
            return true;

        if (!_NpcData.ContainsKey(npcTid))
        {
            _ErrorString += string.Format("NPC服务不存在! questTid: {0}, questName: {1}, npcTid: {2}, 服务器类型: {3}, 服务ID: {4}\n", quest.Id, quest.Name, npcTid, serviceType, serviceID);
            return false;
        }

        Npc npc = _NpcData[npcTid];
		foreach(var service in npc.Services)
		{
			Service tempService = _ServiceData[service.Id];

			switch(serviceType)
			{
				case ServiceType.ServiceType_Conversation:
					{
						if (tempService.Conversation != null)
						{
							if (tempService.Conversation.DialogueId == serviceID)
								return true;
						}
						break;
					}
				default:
					break;
			}
		}

		_ErrorString += string.Format("NPC服务不存在! questTid: {0}, questName: {1}, npcTid: {2}, 服务器类型: {3}, 服务ID: {4}\n", quest.Id, quest.Name, npcTid, serviceType, serviceID);
		return false;
	}

    private void CheckEntityGenerator(Quest quest, int sceneTid, int generatorTid)
    {
        int questTid = quest.Id;
        if (generatorTid == 0)
            return;

        if (_SceneData.ContainsKey(sceneTid))
        {
            bool bFind = false;
            for (int i = 0; i < _SceneData[sceneTid].EntityGeneratorRoot.EntityGenerators.Count; i++)
            {
                if (_SceneData[sceneTid].EntityGeneratorRoot.EntityGenerators[i].EntityType == (int)EntityType.Monster)
                {
                    if (generatorTid == _SceneData[sceneTid].EntityGeneratorRoot.EntityGenerators[i].Id)
                    {
                        bFind = true;
                        break;
                    }
                }
            }

            if (!bFind)
                _ErrorString += string.Format("monster生成器不存在: questTid: {0}, questName: {1}, 场景Id: {2}, 生成器: {3}\n", questTid, quest.Name, sceneTid, generatorTid);
        }
    }

    private void CheckMainQuest(Quest quest)
    {
        if (quest.Type != Quest.QuestType.Main || quest.IsSubQuest)         //只检查主线任务，不检查SubQuest
            return;

        int questTid = quest.Id;

        for (int i = 0; i < quest.ProvideRelated.PredecessorQuest.PreQuests.Count; i++)
        {
            int preTid = quest.ProvideRelated.PredecessorQuest.PreQuests[i].Id;

            if (!_QuestData.ContainsKey(preTid))
            {
                _ErrorString += string.Format("任务前置不存在! questTid: {0}, questName: {1}， preQuestTid: {2}\n", questTid, quest.Name, preTid);
                break;
            }
            if (_QuestData[preTid].DeliverRelated.ViaNpc != null &&
                _QuestData[preTid].DeliverRelated.NextQuestId != questTid)
            {
                _ErrorString += string.Format("前置任务的后续错误! questTid: {0}, questName: {1}， preQuestTid: {2}，preQuest's NextQuest: {3}\n", questTid, quest.Name, preTid, _QuestData[preTid].DeliverRelated.NextQuestId);
                break;
            }
        }
    }
}
