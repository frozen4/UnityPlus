#if BakeReflection
using CinemaSuite.Common;
using System.Reflection;
#endif
using System;
using System.Collections.Generic;

namespace CinemaDirector
{
    /// <summary>
    /// A helper class for getting useful data from Director Runtime objects.
    /// </summary>
    public static class DirectorRuntimeHelper
    {
        static Dictionary<Type, List<Type>> _DicAllowedTrackTypes = new Dictionary<Type, List<Type>>();
        static Dictionary<Type, List<Type>> _DicAllowedItemTypes = new Dictionary<Type, List<Type>>();

#if BakeReflection
        static Dictionary<Type, List<Type>> _CacheSubTypes = new Dictionary<Type, List<Type>>();
        static Dictionary<Type, List<TimelineTrackAttribute>> _CacheTLTAttribute = new Dictionary<Type, List<TimelineTrackAttribute>>();
        static Dictionary<Type, List<CutsceneItemAttribute>> _CacheCIAttribute = new Dictionary<Type, List<CutsceneItemAttribute>>();
        static List<Assembly> _CacheAssembies = null;
        static Dictionary<Assembly, List<Type>> _CacheAssembiesType = new Dictionary<Assembly, List<Type>>();
#endif
        static DirectorRuntimeHelper()
        {
            #region 完全去掉反射，提前bake。 请勿删除这段代码，输出的内容是用来Bake的

            // 经工具统计现在使用中的Class有
            // ActorItemTrack TrackGroup ActorTrackGroup CurveTrack ShotTrack DirectorGroup CinemaShot MultiActorTrackGroup
            // PlayAnimationEvent DisableGameObject EnableGameObject GlobalItemTrack CinemaActorClipCurve
            // SetStopEvent PlayParticleSystemEvent FadeFromBlack TextDegenerationEvent TextGenerationEvent
            // FadeToBlack SetSkipEvent StopParticleSystemEvent SetParent TextBubbleEvent SetPositionEvent

            _DicAllowedTrackTypes.Add(typeof(TrackGroup), new List<Type> { typeof(GlobalItemTrack), typeof(ShotTrack) });
            _DicAllowedTrackTypes.Add(typeof(DirectorGroup), new List<Type> { typeof(GlobalItemTrack), typeof(ShotTrack) });
            _DicAllowedTrackTypes.Add(typeof(ActorTrackGroup), new List<Type> { typeof(ActorItemTrack), typeof(CurveTrack) });
            _DicAllowedTrackTypes.Add(typeof(MultiActorTrackGroup), new List<Type> { typeof(ActorItemTrack), typeof(MultiCurveTrack) });
            //_DicAllowedTrackTypes.Add(typeof(CharacterTrackGroup), new List<Type> { typeof(MecanimTrack), typeof(TransformTrack) });

            _DicAllowedItemTypes.Add(typeof(GlobalItemTrack), new List<Type>
            {
                typeof(FadeFromBlack),
                typeof(FadeToBlack),

                //typeof(PauseCutscene),
                //typeof(PlayCutscene),
                //typeof(StopCutscene),
                //typeof(FadeTexture),
                //typeof(DisableGameObjectGlobal),
                //typeof(EnableGameObjectGlobal),
                //typeof(ColorTransition),
                //typeof(FadeFromWhite),
                //typeof(FadeToWhite),
                //typeof(MassDisabler),
                //typeof(StoryboardEvent)
            });
            _DicAllowedItemTypes.Add(typeof(ShotTrack), new List<Type> { typeof(CinemaShot) });
            _DicAllowedItemTypes.Add(typeof(ActorItemTrack), new List<Type>
            {
                typeof(PlayAnimationEvent),
                typeof(EnableGameObject),
                typeof(DisableGameObject),
                typeof(PlayParticleSystemEvent),
                typeof(StopParticleSystemEvent),
                typeof(SetParent),
                typeof(SetPositionEvent),
                typeof(SetSkipEvent),
                typeof(SetStopEvent),
                typeof(TextBubbleEvent),
                typeof(TextDegenerationEvent),
                typeof(TextGenerationEvent)

                //typeof(BlendAnimationEvent),
                //typeof(CrossFadeAnimationEvent),
                //typeof(RewindAnimationEvent),
                //typeof(SampleAnimationEvent),
                //typeof(StopAnimationEvent),
                //typeof(CrossFadeAnimatorEvent),
                //typeof(LookAtTarget),
                //typeof(PlayAnimatorEvent),
                //typeof(SetBoolAnimatorEvent),
                //typeof(SetFloatAnimatorEvent),
                //typeof(SetIKPositionAnimatorEvent),
                //typeof(SetIKPositionWeightAnimatorEvent),
                //typeof(SetIKRotationAnimatorEvent),
                //typeof(SetIKRotationWeightAnimatorEvent),
                //typeof(SetIntegerAnimatorEvent),
                //typeof(SetLayerWeightAnimatorEvent),
                //typeof(SetLookAtPositionAnimatorEvent),
                //typeof(SetLookAtWeightAnimatorEvent),
                //typeof(SetTargetAnimatorEvent),
                //typeof(SetTriggerAnimatorEvent),
                //typeof(PauseAudioEvent),
                //typeof(PlayAudioEvent),
                //typeof(PlayOneShotAudioEvent),
                //typeof(StopAudioEvent),
                //typeof(DoShake),
                //typeof(DisableBehaviour),
                //typeof(EnableBehaviour),
                //typeof(EnableGameObjectAction),
                //typeof(SendMessageGameObject),
                //typeof(SetLightColour),
                //typeof(SetIntensityLight),
                //typeof(SetDestinationEvent),
                //typeof(PauseParticleSystemEvent),
                //typeof(ApplyForceEvent),
                //typeof(ApplyTorqueEvent),
                //typeof(RigidbodySleepEvent),
                //typeof(RigidbodyWakeUpEvent),
                //typeof(SetMassEvent),
                //typeof(ToggleGravityEvent),
                //typeof(AttachChildrenEvent),
                //typeof(DetachChildrenEvent),
                //typeof(RotateFollow),
                //typeof(SetRotationEvent),
                //typeof(SetScaleEvent),
                //typeof(SetTransformEvent),
                //typeof(TransformLookAtAction),
                //typeof(CanvasCameraSwitchEvent),
                //typeof(ColorChange),
                //typeof(ColorChangeSelectable),
                //typeof(SetIsInteractable),
            });
            _DicAllowedItemTypes.Add(typeof(CurveTrack), new List<Type> { typeof(CinemaActorClipCurve) });
            _DicAllowedItemTypes.Add(typeof(MultiCurveTrack), new List<Type> { typeof(CinemaMultiActorCurveClip) });

//             _DicAllowedItemTypes.Add(typeof(MecanimTrack), new List<Type>
//             {
//                 //typeof(CrossFadeAnimatorEvent),
//                 //typeof(LookAtTarget),
//                 //typeof(MatchTargetEvent),
//                 //typeof(PlayAnimatorEvent),
//                 //typeof(SetBoolAnimatorEvent),
//                 //typeof(SetFloatAnimatorEvent),
//                 //typeof(SetIntegerAnimatorEvent),
//                 //typeof(SetLayerWeightAnimatorEvent),
//                 //typeof(SetLookAtPositionAnimatorEvent),
//                 //typeof(SetLookAtWeightAnimatorEvent),
//                 //typeof(SetTriggerAnimatorEvent)
//             });
//             _DicAllowedItemTypes.Add(typeof(TransformTrack), new List<Type>
//             {
//                 //typeof(RotateFollow),
//                 //typeof(SetTransformEvent),
//                 //typeof(TransformLookAtAction)
//             });
#if BakeReflection
            //GetAllowedTrackTypes(typeof(ActorTrackGroup));
            //GetAllowedTrackTypes(typeof(CharacterTrackGroup));
            //GetAllowedTrackTypes(typeof(DirectorGroup));
            //GetAllowedTrackTypes(typeof(MultiActorTrackGroup));
            //GetAllowedTrackTypes(typeof(TrackGroup));

            //GetAllowedItemTypes(typeof(GlobalItemTrack));
            //GetAllowedItemTypes(typeof(MecanimTrack));
            //GetAllowedItemTypes(typeof(MultiCurveTrack));
            //GetAllowedItemTypes(typeof(ShotTrack));
            //GetAllowedItemTypes(typeof(TransformTrack));
            //GetAllowedItemTypes(typeof(TimelineTrack));
            //GetAllowedItemTypes(typeof(ActorItemTrack));
            //GetAllowedItemTypes(typeof(AudioTrack));
            //GetAllowedItemTypes(typeof(CurveTrack));


            string res = "";
            foreach (var dic in _DicAllowedTrackTypes)
            {
                res += "_DicAllowedTrackTypes.Add(typeof(" + dic.Key.Name + "), new List<Type> {";
                var v = dic.Value;
                for (int i = 0; i < v.Count; i++)
                {
                    var track = v[i];
                    res += string.Format("typeof({0})", track.Name);
                    if (i < v.Count - 1)
                    {
                        res += ", ";
                    }
                }
                res += "});\n";
            }
            foreach (var dic in _DicAllowedItemTypes)
            {
                res += "_DicAllowedItemTypes.Add(typeof(" + dic.Key.Name + "), new List<Type> {";

                var v = dic.Value;
                for (int i = 0; i < v.Count; i++)
                {
                    var track = v[i];
                    res += string.Format("typeof({0})", track.Name);
                    if (i < v.Count - 1)
                    {
                        res += ", ";
                    }
                }
                res += "});\n";
            }
            Debug.Log(res);
#endif
#endregion
        }

        /// <summary>
        /// Returns a list of Track types that are associated with the given Track Group.
        /// </summary>
        /// <param name="trackGroup">The track group to be inspected</param>
        /// <returns>A list of track types that meet the genre criteria of the given track group.</returns>
        public static List<Type> GetAllowedTrackTypes(TrackGroup trackGroup)
        {
            var trackGroupType = trackGroup.GetType();
            List<Type> allowedTrackTypes;
            if (_DicAllowedTrackTypes.TryGetValue(trackGroupType, out allowedTrackTypes))
                return allowedTrackTypes;
#if BakeReflection
            // Get all the allowed Genres for this track group
            TimelineTrackGenre[] genres = new TimelineTrackGenre[0];

            TrackGroupAttribute[] tga = ReflectionHelper.GetCustomAttributes<TrackGroupAttribute>(trackGroupType, true);
            for (int i = 0; i < tga.Length; i++)
            {
                if (tga[i] != null)
                {
                    genres = tga[i].AllowedTrackGenres;
                    break;
                }
            }

            Type[] subTypes = GetAllSubTypes(typeof(TimelineTrack));
            allowedTrackTypes = new List<Type>();
            for (int i = 0; i < subTypes.Length; i++)
            {
                TimelineTrackAttribute[] customAttributes;

                var subType = subTypes[i];
                List<TimelineTrackAttribute> cache = null;
                if (_CacheTLTAttribute.TryGetValue(subType, out cache))
                {
                    //Debug.Log("++++" + trackGroupType.Name);
                    customAttributes = cache.ToArray();
                }
                else
                {
                    //Debug.Log(trackGroupType.Name);
                    customAttributes = ReflectionHelper.GetCustomAttributes<TimelineTrackAttribute>(subType, true);
                    _CacheTLTAttribute.Add(subType, new List<TimelineTrackAttribute>(customAttributes));
                }

                for (int j = 0; j < customAttributes.Length; j++)
                {
                    if (customAttributes[j] != null)
                    {
                        for (int k = 0; k < customAttributes[j].TrackGenres.Length; k++)
                        {
                            TimelineTrackGenre genre = customAttributes[j].TrackGenres[k];
                            for (int l = 0; l < genres.Length; l++)
                            {
                                if (genre == genres[l])
                                {
                                    allowedTrackTypes.Add(subType);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            _DicAllowedTrackTypes.Add(trackGroupType, allowedTrackTypes);
#endif
            return allowedTrackTypes;
        }

        /// <summary>
        /// Returns a list of Cutscene Item types that are associated with the given Track.
        /// </summary>
        /// <param name="timelineTrack">The track to look up.</param>
        /// <returns>A list of valid cutscene item types.</returns>
        public static List<Type> GetAllowedItemTypes(TimelineTrack timelineTrack)
        {
            var timelineTrackType = timelineTrack.GetType();
            List<Type> allowedItemTypes;
            if (_DicAllowedItemTypes.TryGetValue(timelineTrackType, out allowedItemTypes))
                return allowedItemTypes;
#if BakeReflection
            // Get all the allowed Genres for this track group
            CutsceneItemGenre[] genres = new CutsceneItemGenre[0];

            TimelineTrackAttribute[] tta = ReflectionHelper.GetCustomAttributes<TimelineTrackAttribute>(timelineTrackType, true);
            for (int i = 0; i < tta.Length; i++)
            {
                if (tta[i] != null)
                {
                    genres = tta[i].AllowedItemGenres;
                    break;
                }
            }

            Type[] subTypes = GetAllSubTypes(typeof(TimelineItem));
            allowedItemTypes = new List<Type>();
            for (int i = 0; i < subTypes.Length; i++)
            {
                CutsceneItemAttribute[] customAttributes;

                var subType = subTypes[i];
                List<CutsceneItemAttribute> cache = null;
                if (_CacheCIAttribute.TryGetValue(subType, out cache))
                {
                    customAttributes = cache.ToArray();
                }
                else
                {
                    customAttributes = ReflectionHelper.GetCustomAttributes<CutsceneItemAttribute>(subType, true);
                    _CacheCIAttribute.Add(subType, new List<CutsceneItemAttribute>(customAttributes));
                }

                for (int j = 0; j < customAttributes.Length; j++)
                {
                    if (customAttributes[j] != null)
                    {
                        for (int k = 0; k < customAttributes[j].Genres.Length; k++)
                        {
                            CutsceneItemGenre genre = customAttributes[j].Genres[k];
                            for (int l = 0; l < genres.Length; l++)
                            {
                                if (genre == genres[l])
                                {
                                    allowedItemTypes.Add(subType);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            _DicAllowedItemTypes.Add(timelineTrackType, allowedItemTypes);
#endif
            return allowedItemTypes;
        }
#if BakeReflection
        /// <summary>
        /// Get all Sub types from the given parent type.
        /// </summary>
        /// <param name="ParentType">The parent type</param>
        /// <returns>all children types of the parent.</returns>
        private static Type[] GetAllSubTypes(System.Type ParentType)
        {
            List<System.Type> list;
            if (_CacheSubTypes.TryGetValue(ParentType, out list))
            {
                return list.ToArray();
            }
            list = new List<System.Type>();
            Assembly[] assemblies;
            if(_CacheAssembies != null && _CacheAssembies.Count > 0)
            {
                assemblies = _CacheAssembies.ToArray();
            }
            else
            {
                assemblies = ReflectionHelper.GetAssemblies();
                _CacheAssembies = new List<Assembly>(assemblies);
            }
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types;

                List<Type> cache;
                var assemblie = assemblies[i];
                if (_CacheAssembiesType.TryGetValue(assemblie, out cache))
                {
                    types = cache.ToArray();
                }
                else
                {
                    types = ReflectionHelper.GetTypes(assemblie);
                    _CacheAssembiesType.Add(assemblie, new List<Type>(types));
                }

                for (int j = 0; j < types.Length; j++)
                {
                    if (types[j] != null && ReflectionHelper.IsSubclassOf(types[j], ParentType))
                    {
                        list.Add(types[j]);
                    }
                }
            }
            _CacheSubTypes.Add(ParentType, list);
            return list.ToArray();
        }
#endif
    }
}
