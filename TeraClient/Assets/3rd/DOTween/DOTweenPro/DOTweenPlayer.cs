using UnityEngine;
using System.Collections.Generic;
using DG.Tweening.Core;

namespace DG.Tweening
{
    public enum OnEnableBehaviour_My
    {
        None = 0,
        Play = 1,
        Restart = 2,
        PlayForward = 3,
    }

    public enum OnDisableBehaviour_My
    {
        None = 0,
        Pause = 1,
        Rewind = 2,
        //SmartKill = 3,
        Kill = 3,
        PlayBackward = 4,
        //KillAndComplete,
        //DestroyGameObject,
    }

    [AddComponentMenu("DOTweenPlayer")]
    [DisallowMultipleComponent]
    public class DOTweenPlayer : MonoBehaviour
    {
        [System.Serializable]
        public class DOTGroup
        {
            public string ID;
            public OnEnableBehaviour_My onEnableBehaviour;
            public OnDisableBehaviour_My onDisableBehaviour;

            public DOTweenAnimation[] AllComps;

            //public GameObject[] AllGOs;

            public DOTGroup(string s_id,DOTweenAnimation[] l_dta)
            {
                ID = s_id;
                AllComps = l_dta;
            }

            public DOTGroup(string s_id, int i_cnt)
            {
                ID = s_id;
                AllComps = new DOTweenAnimation[i_cnt];
            }

            public DOTGroup(string s_id, List<DOTweenAnimation> l_dta/*, List<GameObject> l_go*/)
            {
                ID = s_id;
                AllComps = l_dta.ToArray();
                //AllGOs = l_go.ToArray();
            }

            public enum ActionType
            {
                None = 0,
                Play = 1,
                PlayOnce = 2,
                PlayForward = 3,
                PlayBackward = 4,
                Restart = 5,
                Pause = 6,
                Rewind = 7,
                Kill = 8,
                SmartKill = 9,
                KillAndComplete = 10,
                DestroyGameObject = 11
            }

            public void Play(ActionType e_type)
            {
                DOTweenAnimation[] a_dtas = AllComps;

                switch (e_type)
                {
                    case ActionType.Restart:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && !a_dtas[i].isFrom)
                            {
                                a_dtas[i].SingleRestart();
                            }
                        }
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && a_dtas[i] != null && a_dtas[i].isFrom)
                            {
                                a_dtas[i].SingleRestart();
                            }
                        }
                        break;
                    case ActionType.Play:       //this is resume
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && !a_dtas[i].isFrom)
                            {
                                a_dtas[i].SinglePlay();
                            }
                        }
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && a_dtas[i].isFrom)
                            {
                                a_dtas[i].SinglePlay();
                            }
                        }
                        break;
                    case ActionType.PlayOnce:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && !a_dtas[i].isFrom)
                            {
                                a_dtas[i].SinglePlayOnce();
                            }
                        }
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && a_dtas[i].isFrom)
                            {
                                a_dtas[i].SinglePlayOnce();
                            }
                        }
                        break;
                    case ActionType.PlayForward:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && !a_dtas[i].isFrom)
                            {
                                a_dtas[i].SingleDirectional(true);
                            }
                        }
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && a_dtas[i].isFrom)
                            {
                                a_dtas[i].SingleDirectional(true);
                            }
                        }
                        break;
                    case ActionType.PlayBackward:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && !a_dtas[i].isFrom)
                            {
                                a_dtas[i].SingleDirectional(false);
                            }
                        }
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && a_dtas[i].isFrom)
                            {
                                a_dtas[i].SingleDirectional(false);
                            }
                        }
                        break;
                    case ActionType.Pause:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null)
                            {
                                a_dtas[i].SinglePause();
                            }
                        }
                        break;
                    case ActionType.Rewind:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && !a_dtas[i].isFrom)
                            {
                                a_dtas[i].SingleRewind();
                            }
                        }
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null && a_dtas[i] != null && a_dtas[i].isFrom)
                            {
                                a_dtas[i].SingleRewind();
                            }
                        }
                        break;
                    case ActionType.Kill:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null)
                            {
                                a_dtas[i].SingleKill();
                            }
                        }
                        break;
                    case ActionType.SmartKill:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null)
                            {
                                a_dtas[i].SingleKill(b_smart: true);
                            }
                        }
                        break;
                    case ActionType.KillAndComplete:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null)
                            {
                                a_dtas[i].SingleKill(true);
                            }
                        }
                        break;
                    case ActionType.DestroyGameObject:
                        for (int i = 0; i < a_dtas.Length; i++)
                        {
                            if (a_dtas[i] != null)
                            {
                                a_dtas[i].SingleDestroyObj();
                            }
                        }
                        break;
                }
            }

            public void PlayDirectional(bool forward)
            {
                DOTweenAnimation[] a_dtas = AllComps;

                for (int i = 0; i < a_dtas.Length; i++)
                {
                    if (a_dtas[i] != null && (!(a_dtas[i].isFrom) == forward))
                    {
                        a_dtas[i].SingleDirectional(forward);
                    }
                }
                for (int i = 0; i < a_dtas.Length; i++)
                {
                    if (a_dtas[i] != null && (a_dtas[i].isFrom == forward))
                    {
                        a_dtas[i].SingleDirectional(forward);
                    }
                }

            }

            public void GoToStartPos()
            {
                DOTweenAnimation[] a_dtas = AllComps;
                for (int i = 0; i < a_dtas.Length; i++)
                {
                    if (a_dtas[i] != null)
                    {
                        a_dtas[i].GoToStartPos();
                    }
                }
            }

            public void GoToEndPos()
            {
                DOTweenAnimation[] a_dtas = AllComps;
                for (int i = 0; i < a_dtas.Length; i++)
                {
                    if (a_dtas[i] != null)
                    {
                        a_dtas[i].GoToEndPos();
                    }
                }
            }
        }

        public DOTGroup[] AllGroups = new DOTGroup[0];
        //private bool _IsReady = false;

        public int FindGroup(string s_id)
        {
            for (int i = 0; i < AllGroups.Length; i++)
            {
                if (AllGroups[i].ID == s_id)
                {
                    return i;
                }
            }
            return -1;
        }

        private void OnEnable()
        {
            //Debug.Log("OnEnable "+name);
            for (int i = 0; i < AllGroups.Length; i++)
            {
                switch (AllGroups[i].onEnableBehaviour)
                {
                    case OnEnableBehaviour_My.Play:
                        {
                            AllGroups[i].Play(DOTGroup.ActionType.Play);
                            break;
                        }
                    case OnEnableBehaviour_My.Restart:
                        {
                            //if(_IsReady)
                            {
                                AllGroups[i].Play(DOTGroup.ActionType.Restart);
                            }
                            break;
                        }
                    case OnEnableBehaviour_My.PlayForward:
                        {
                            //if(_IsReady)
                            {
                                AllGroups[i].Play(DOTGroup.ActionType.PlayForward);
                            }
                            break;
                        }
                    //case OnEnableBehaviour_My.RestartFromSpawnPoint:
                    //    //this._requiresRestartFromSpawnPoint = true;
                    //    break;
                }
            }
        }

        //private void Start()
        //{
            //_IsReady = true;
        //}

        private void OnDisable()
        {
            //Debug.Log("OnDisable");
            //_IsReady = true;

            //this._requiresRestartFromSpawnPoint = false;
            for (int i = 0; i < AllGroups.Length; i++)
            {
                switch (AllGroups[i].onDisableBehaviour)
                {
                    case OnDisableBehaviour_My.Pause:
                        {
                            AllGroups[i].Play(DOTGroup.ActionType.Pause);
                            break;
                        }
                    case OnDisableBehaviour_My.Rewind:
                        {
                            AllGroups[i].Play(DOTGroup.ActionType.Rewind);
                            break;
                        }
                    //case OnDisableBehaviour_My.SmartKill:
                    //    {
                    //        AllGroups[i].Play(DOTGroup.ActionType.SmartKill);
                    //        break;
                    //    }
                    case OnDisableBehaviour_My.Kill:
                        {
                            AllGroups[i].Play(DOTGroup.ActionType.Kill);
                            break;
                        }
                    //case OnDisableBehaviour_My.KillAndComplete:
                    //    {
                    //        //ABSAnimationComponent component = base.GetComponent<ABSAnimationComponent>();
                    //        //if ((Object)component != (Object)null)
                    //        //{
                    //        //    component.DOComplete();
                    //        //    component.DOKill();
                    //        //}
                    //        AllComps[i].Play(DOTGroup.ActionType.KillAndComplete);
                    //        break;
                    //    }
                    //case OnDisableBehaviour_My.DestroyGameObject:
                    //    {
                    //        //ABSAnimationComponent component = base.GetComponent<ABSAnimationComponent>();
                    //        //if ((Object)component != (Object)null)
                    //        //{
                    //        //    component.DOKill();
                    //        //}
                    //        //Object.Destroy(base.gameObject);
                    //        AllComps[i].Play(DOTGroup.ActionType.DestroyGameObject);
                    //        break;
                    //    }
                    case OnDisableBehaviour_My.PlayBackward:
                        {
                            //if(_IsReady)
                            {
                                AllGroups[i].Play(DOTGroup.ActionType.PlayBackward);
                            }
                            break;
                        }
                }
            }
        }

        private void OnDestroy()
        {
            AllGroups = null;
        }

        public void Play(string s_id)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].Play(DOTGroup.ActionType.Play);
            }
        }

        public void PlayOnce(string s_id)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].Play(DOTGroup.ActionType.PlayOnce);
            }
        }

        public void Pause(string s_id)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].Play(DOTGroup.ActionType.Pause);
            }
        }

        public void Rewind(string s_id)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].Play(DOTGroup.ActionType.Rewind);
            }
        }

        public void Restart(string s_id)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].Play(DOTGroup.ActionType.Restart);
            }
        }

        public void Stop(string s_id)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].Play(DOTGroup.ActionType.Kill);
            }
        }

        public void PlayDirectional(string s_id, bool forward)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].PlayDirectional(forward);
            }
        }

        //public DOTweenAnimation[] GetTweenList(string id)
        //{
        //    int i_id=FindGroup(id);
        //    if (i_id > -1)
        //        return AllGroups[i_id].AllComps;
        //    return null;
        //}

        public void GoToStartPos(string s_id)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].GoToStartPos();
            }
        }

        public void GoToEndPos(string s_id)
        {
            int i_pos = FindGroup(s_id);

            if (i_pos > -1)
            {
                AllGroups[i_pos].GoToEndPos();
            }
        }

        [NoToLua]
        public void FindAndDo(string s_id, DOTGroup.ActionType play_mode)
        {
            DOTweenAnimation[] dotas = GetComponentsInChildren<DOTweenAnimation>(true);
            DOTGroup group = new DOTGroup(s_id, dotas);
            group.Play(play_mode);
        }

        public void FindAndDoRestart(string s_id)
        {
            FindAndDo(s_id, DOTGroup.ActionType.Restart);
        }

        public void FindAndDoKill(string s_id)
        {
            FindAndDo(s_id, DOTGroup.ActionType.Kill);
        }

    }

}
