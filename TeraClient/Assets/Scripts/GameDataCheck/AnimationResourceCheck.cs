using System;
using Common;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Template;
using Object = UnityEngine.Object;


public class AnimationResourceCheck : Singleton<AnimationResourceCheck>
{
    public class AnimationCheckEntry
    {
        public int index = 0;
        public string childPath = string.Empty;
        public string animationClip = string.Empty;
    }

    public List<string> _ModelAssetPathList = new List<string>();

    public List<AnimationCheckEntry> _AnimationCheckList_CreateRole = new List<AnimationCheckEntry>();
    public readonly string _SelectRole = "Assets/Outputs/Scenes/SelectChar.prefab";
    public readonly string _CreateRole = "Assets/Outputs/Scenes/CreatCharacter.prefab";
    
    private readonly string[] _CreateRoleModelPaths = new string[]{
        "MainAnimator/Character/CharacterRoot/humwarrior_m_create",
		"MainAnimator/Character/CharacterRoot/alipriest_f_create",	
		"MainAnimator/Character/CharacterRoot/casassassin_m_create",	
		"MainAnimator/Character/CharacterRoot/sprarcher_f_create"
    };

    private readonly string[] _CreateRoleModelClips = new string[] {
        "create_stand",
        "create_idle1",
    };

    public void Init()
    {
        //select role

        //create role
        for(int i = 0; i < _CreateRoleModelPaths.Length; ++i)
        {
            foreach(string clip in _CreateRoleModelClips)
            {
                _AnimationCheckList_CreateRole.Add(new AnimationCheckEntry() {
                    childPath = _CreateRoleModelPaths[i],
                    animationClip = clip,
                    index = i,
                });
            }
        }

        //
        _ModelAssetPathList.Clear();
        _ModelAssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("characters"));
        _ModelAssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("monsters"));
    }

    public string _ErrorString = "";

}


