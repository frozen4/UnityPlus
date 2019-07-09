using Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public enum WeatherType
{
    None = 0,
    Rain = 4,
    Snow = 5
}

public enum DynamicEffectType
{
    None = 99,
    Morning = 0,
    Day = 1,
    Dusk = 2,
    Night = 3
}

public class ScenesManager : MonoBehaviourSingleton<ScenesManager>, GameLogic.ITickLogic
{
    private readonly TerrainsManager _TerrainsManager = new TerrainsManager();
    private readonly ScenesBlocksManager _BlocksManager = new ScenesBlocksManager();
    private readonly ScenesColliderManager _ColliderManager = new ScenesColliderManager();


    private int _CurrentMapID = 0;
    private int _CurrentSceneTid = 0;
    private string _CurNavmeshName = null;

    private GameObject _SceneRoot = null;
    private SceneConfig _ConfigData = null;

    private Rain _Rain = null;
    private Snow _Snow = null;

    public bool IsUseWeatherEffect = true;

    private float _LastTime1 = 0;
    private float _LastTime2 = 0;

    public int GetCurrentMapID()
    {
        return _CurrentMapID;
    }
    public int GetCurrentSceneTid()
    {
        return _CurrentSceneTid;
    }
    public string GetCurrentNavmeshName()
    {
        return _CurNavmeshName;
    }

    public void Tick(float dt) {}

    public IEnumerable TickCoroutine()
    {
        if (null != Main.Main3DCamera && Main.HostPalyer != null && _ConfigData != null)
        {
            Vector3 camPos = Main.Main3DCamera.transform.position;
            var position = Main.HostPalyer.transform.position;

            //float now = Time.time;
            //if (now - _LastTime1 > 2)
            {
                //_LastTime1 = now;
                _TerrainsManager.Update(camPos);
                _ColliderManager.Update(position);
                yield return null;

                foreach (var item in _BlocksManager.TickCoroutine(position))
                    yield return item;
            }
        }
    }

    public void UpdataSceneInfo(int sceneTid, int mapID, string navmeshName)
    {
        _CurrentSceneTid = sceneTid;
        _CurrentMapID = mapID;
        _CurNavmeshName = navmeshName;
    }

    public SceneConfig GetSceneConfig()
    {
        return _ConfigData;
    }

    public void OnEnterScene(GameObject sceneRoot)
    {
        _SceneRoot = sceneRoot;
        if (null == sceneRoot)
        {
            HobaDebuger.Log("scene prefab seems  instantiate  failed . ");
            return;
        }

        _ConfigData = sceneRoot.GetComponent<SceneConfig>();
        if (null == _ConfigData)
        {
            HobaDebuger.Log(" Is not a scene prefab.");
            return;
        }

        ScenesRegionManager.Instance.Init(_ConfigData._LightRegionName);
        DynamicEffectManager.Instance.Init(_ConfigData);
        _TerrainsManager.Init(_ConfigData._LightmapConfig._TerrainLightmapInfos);
        _BlocksManager.Init(_ConfigData);
        _ColliderManager.Init(_ConfigData);
    }

    /// 根据角色位置，预加载周围的分块后回调
    /// 在刚刚进入新地图后调用
    public void LoadObjectsAtPos(float posx, float posz, Action callback)
    {
        ///提前加载一次BOXColider
        _ColliderManager.Preload(posx, posz);
        _BlocksManager.Preload(posx, posz, callback);
    }

    /// 玩家同图传送，位置发生变化时调用 OnHostPlayerPosChange
    /// 根据角色位置，加载周围的分块和阻挡
    public void UpdateObjects(Vector3 position)
    {
        _ColliderManager.Preload(position.x, position.z);
        _BlocksManager.UpdateBlocks(position);
    }

    public void ChangeDetailLevel(int sceneQuality)
    {
        _BlocksManager.SceneQuality = sceneQuality;
    }

    public int GetSceneQuality()
    {
        return _BlocksManager.SceneQuality;
    }

    public Dictionary<GameObject, StaticObjectAudio> GetStaticObjectAudioList()
    {
        return _BlocksManager.GetAudioObjectDic();
    }

    public void UpdateWaterReflection()
    {
        _BlocksManager.UpdateWaterReflection();
    }

    // 服务器通知天气变化 S2CWeatherChange
    public void ChangeWeather(WeatherType weatherType)
    {
        if (!IsUseWeatherEffect) return;
        if (weatherType == WeatherType.Rain)
        {
            if (null != _Snow) _Snow.enabled = false;
            if (null == _Rain)
            {
                _Rain = this.gameObject.AddComponent<Rain>();
            }
            if (null != _Rain) _Rain.enabled = true;
        }
        else if (weatherType == WeatherType.Snow)
        {
            if (null == _Snow)
            {
                _Snow = this.gameObject.AddComponent<Snow>();
            }
            if (null != _Snow) _Snow.enabled = true;
            if (null != _Rain) _Rain.enabled = false;
        }
        else if (weatherType == WeatherType.None)
        {
            if (null != _Snow) _Snow.enabled = false;
            if (null != _Rain) _Rain.enabled = false;
        }

        DynamicEffectManager.Instance.EnterWeatherDynamicEffect(weatherType);
    }


    [ContextMenu("Close")]
    public void CloseAnimaion()
    {
        ScenesAnimationManager.Instance.CloseAnimaiontion();
    }
    [ContextMenu("Open")]
    public void OpenAnimaion()
    {
        ScenesAnimationManager.Instance.OpenLightAnimaiotn();
    }

    public void ChangeWeatherByEffectID(int effectID)
    {
        DynamicEffectManager.Instance.ChangeWeatherByEffectID(effectID);
    }


    public void ChangeWeatherByMemory(int effectID)
    {
        DynamicEffectManager.Instance.ChangeWeatherByMemory(effectID);
    }
    public void Cleanup()
    {
        _TerrainsManager.Clear();
        _BlocksManager.Clear();
        _ColliderManager.Clear();

        DynamicEffectManager.Instance.Cleanup();
        ScenesAnimationManager.Instance.Clear();
        ScenesRegionManager.Instance.Clear();

        if (null != _Rain) _Rain.enabled = false;
        if (null != _Snow) _Snow.enabled = false;
    }

    public void Release()
    {
        Cleanup();
    }


    public void OnDebugCmd(string cmd)
    {
        string[] result = cmd.Split(' ');
        if (result.Length == 0 || result.GetLength(0) < 2) return;

        if (result[0].Equals("dn"))
        {
            int cmdid = int.Parse(result[1]);
            if (cmdid == 9)
            {
                ChangeWeather(WeatherType.Rain);
            }
            else if (cmdid == 10)
            {
                ChangeWeather(WeatherType.Snow);
            }
            else if (cmdid == 11)
            {
                ChangeWeather(WeatherType.None);
            }
            else if (cmdid == 1)
            {
                DynamicEffectManager.Instance.OnDebugCmd(DynamicEffectType.Morning);
            }
            else if (cmdid == 2)
            {
                DynamicEffectManager.Instance.OnDebugCmd(DynamicEffectType.Day);
            }
            else if (cmdid == 3)
            {
                DynamicEffectManager.Instance.OnDebugCmd(DynamicEffectType.Dusk);
            }
            else if (cmdid == 4)
            {
                DynamicEffectManager.Instance.OnDebugCmd(DynamicEffectType.Night);
            }
            else if (cmdid == 801)
            {
                HobaDebuger.LogError("InitializeSingularSDK");
                SingularSDK.InitializeSingularSDK();
            }
            else if (cmdid == 802)
            {
                // An example login event with no arguments
                HobaDebuger.LogError("SingularSDK.Event");
                SingularSDK.Event("Login");
            }
            else if (cmdid == 803)
            {
                // An example login event passing two key value pairs
                SingularSDK.Event("Login", "Key1", "Value1", "Key2", 1234);
            }
            else if (cmdid == 804)
            {
                // An example JSONEvent passing a dictionary
                SingularSDK.Event(new Dictionary<string, object>() {
                {"Key1", "Value1"},
                {"Key2", new Dictionary<string, object>() {
                {"SubKey1", "SubValue1"},
                {"SubKey2", "SubValue2"}
                }
                }
                }, "JSONEvent");
            }
            else if (cmdid == 805)
            {
                // Revenue with no product details
                SingularSDK.Revenue("USD", 9.99);
            }
            else if (cmdid == 806)
            {
                // Revenue with product details
                SingularSDK.Revenue("USD", 50.50, "abc123", "myProductName", "myProductCategory", 2, 25.50);
            }
            else if (cmdid == 807)
            {
                // Set a Custom User ID
                SingularSDK.SetCustomUserId("custom_user_id");

                // Unset a Custom User ID
                SingularSDK.UnsetCustomUserId();
            }
            else if (cmdid == 700)
            {
                //SendDebugData();
            }
            else if (cmdid == 808)
            {
                _BlocksManager.OnDebugCmd(true);
                _ColliderManager.OnDebugCmd(false);
            }
            else if (cmdid == 809)
            {
                _BlocksManager.OnDebugCmd(false);
                _ColliderManager.OnDebugCmd(true);
            }

        }
    }
}