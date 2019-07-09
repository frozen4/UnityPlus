//----------------------------------------------
//            Xffect Editor
// Copyright © 2012- Shallway Studio
// http://shallway.net
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Xft
{
    public class EffectNode : IComparable<EffectNode>
    {
        public RenderObject RenderObj;

        //constructor
        protected ERenderType Type;  //1 sprite, 2 ribbon trail, 3 cone, 4, custom mesh.
        public int Index;
        public ulong TotalIndex;
        public Transform ClientTrans;
        public bool SyncClient;
        public EffectLayer Owner;

        //internal using
        protected Vector3 CurDirection;
        protected Vector3 LastWorldPos = Vector3.zero;
        public Vector3 CurWorldPos;
        protected float ElapsedTime;

        //affect by affector
        public Vector3 Position;
        public Vector2 LowerLeftUV;
        public Vector2 UVDimensions;
        public Vector3 Velocity;
        public Vector2 Scale;
        public float RotateAngle;
        public Color Color;

        public XffectComponent SubEmitter = null;


        public Camera MyCamera
        {
            get
            {
                if (Owner == null)
                {
                    Debug.LogError("something wrong with camera init!");
                    return null;
                }
                return Owner.MyCamera;
            }
        }

        //reset
        public List<Affector> AffectorList;
        public Vector3 OriDirection;
        public float LifeTime;
        public int OriRotateAngle;
        public float OriScaleX;
        public float OriScaleY;

        public bool SimpleSprite = false;
        public Color StartColor;

        //public bool mIsFade = false;
        //protected float mOriSpeed;
        //protected float mSplineOffset;

        public int CompareTo(EffectNode other)
        {
            return this.TotalIndex.CompareTo(other.TotalIndex);
        }

        public EffectNode(int index, Transform clienttrans, bool sync, EffectLayer owner)
        {
            Index = index;
            ClientTrans = clienttrans;
            SyncClient = sync;
            Owner = owner;
            LowerLeftUV = Vector2.zero;
            UVDimensions = Vector2.one;
            Scale = Vector2.one;
            RotateAngle = 0;
            Color = Color.white;
        }

        public void SetAffectorList(List<Affector> afts)
        {
            AffectorList = afts;
        }

        public void Init(Vector3 oriDir, float speed, float life, int oriRot, float oriScaleX, float oriScaleY, Color oriColor, Vector2 oriLowerUv, Vector2 oriUVDimension)
        {
            OriDirection = oriDir;
            LifeTime = life;
            OriRotateAngle = oriRot;
            OriScaleX = oriScaleX;
            OriScaleY = oriScaleY;

            StartColor = oriColor;

            Color = oriColor;
            ElapsedTime = 0f;
            // direction is synced to client rotation, except sphere dir
            if (Owner.AlwaysSyncRotation)
                Velocity = Owner.ClientTransform.rotation * OriDirection * speed;
            else
                Velocity = OriDirection * speed;
            LowerLeftUV = oriLowerUv;
            UVDimensions = oriUVDimension;
            RenderObj.Initialize(this);
        }

        public float GetElapsedTime()
        {
            return ElapsedTime;
        }

        public float GetLifeTime()
        {
            return LifeTime;
        }

        public void SetLocalPosition(Vector3 pos)
        {
            //ribbon trail needs to reset the head.
            if (Type == ERenderType.Ribbon)
            {
                RibbonTrail rt = RenderObj as RibbonTrail;
                if (!SyncClient)
                    rt.OriHeadPos = pos;
                else
                    rt.OriHeadPos = GetRealClientPos() + pos;
            }

            Position = pos;
        }

        public Vector3 GetRealClientPos()
        {
            Vector3 mscale = Vector3.one * Owner.Owner.Scale;
            Vector3 clientPos = Vector3.zero;
            clientPos.x = ClientTrans.position.x / mscale.x;
            clientPos.y = ClientTrans.position.y / mscale.y;
            clientPos.z = ClientTrans.position.z / mscale.z;
            return clientPos;
        }

        //back to original scale pos.
        public Vector3 GetOriginalPos()
        {
            Vector3 ret;
            Vector3 clientPos = GetRealClientPos();
            if (!SyncClient)
                ret = Position - clientPos + ClientTrans.position;
            else
                ret = Position + ClientTrans.position;
            return ret;
        }


        //added to optimize grid effect..if simpe no Transform() every circle.
        protected bool IsSimpleSprite()
        {
            return (Owner.SpriteType == (int) STYPE.XY
                    && Owner.OriVelocityAxis == Vector3.zero
                    && !Owner.ScaleAffectorEnable
                    && !Owner.RotAffectorEnable
                    && Owner.OriSpeed < 1e-04
                    && !Owner.GravityAffectorEnable
                    && !Owner.AirAffectorEnable
                    && !Owner.TurbulenceAffectorEnable
                    && !Owner.BombAffectorEnable
                    && !Owner.UVRotAffectorEnable
                    && !Owner.UVScaleAffectorEnable
                    && Mathf.Abs(Owner.OriRotationMax - Owner.OriRotationMin) < 1e-04
                    && Mathf.Abs(Owner.OriScaleXMin - Owner.OriScaleXMax) < 1e-04
                    && Mathf.Abs(Owner.OriScaleYMin - Owner.OriScaleYMax) < 1e-04
                    && Owner.SpeedMin < 1e-04);
        }

        public void SetRenderType(ERenderType type)
        {
            Type = type;
            if (type == ERenderType.Sprite)
            {
                RenderObj = Owner.GetVertexPool().AddSprite(Owner.SpriteWidth, Owner.SpriteHeight,
                    (STYPE)Owner.SpriteType, (ORIPOINT)Owner.OriPoint, 60f, IsSimpleSprite());
            }
            else if (type == ERenderType.Ribbon)
            {
                float rwidth = Owner.RibbonWidth;
                float rlen = Owner.RibbonLen;
                if (Owner.UseRandomRibbon)
                {
                    rwidth = UnityEngine.Random.Range(Owner.RibbonWidthMin, Owner.RibbonWidthMax);
                    rlen = UnityEngine.Random.Range(Owner.RibbonLenMin, Owner.RibbonLenMax);
                }
                RenderObj = Owner.GetVertexPool().AddRibbonTrail(Owner.FaceToObject, Owner.FaceObject,
                    rwidth, Owner.MaxRibbonElements, rlen, Owner.ClientTransform.position + Owner.EmitPoint, 60f);
            }
            else if (type == ERenderType.Cone)
            {
                RenderObj = Owner.GetVertexPool().AddCone(Owner.ConeSize, Owner.ConeSegment,
                    Owner.ConeAngle, Owner.OriVelocityAxis, 0, 60f, Owner.UseConeAngleChange, Owner.ConeDeltaAngle);
            }
            else if (type == ERenderType.CustomMesh)
            {
                if (Owner.CMesh == null)
                    Debug.LogError("custom mesh layer has no mesh to display!", Owner.gameObject);
                if (Owner.OriVelocityAxis == Vector3.zero)
                    Owner.OriVelocityAxis = Vector3.up;

                var dir = Owner.OriVelocityAxis;
                RenderObj = Owner.GetVertexPool().AddCustomMesh(Owner.CMesh, dir, 60f);
            }

            RenderObj.Node = this;
        }

        public void Reset()
        {
            Position = Owner.ClientTransform.position;
            Velocity = Vector3.zero;
            ElapsedTime = 0f;
            CurWorldPos = Owner.transform.position;
            LastWorldPos = CurWorldPos;

            if (Owner.IsRandomStartColor)
                StartColor = Owner.RandomColorGradient.Evaluate(UnityEngine.Random.Range(0f, 1f));

            foreach (var v in AffectorList)
            {
                v.Reset();
            }

            Scale = Vector3.one;
            //mIsFade = false;
            RenderObj.Reset();
        }

        public void Remove()
        {
            Owner.RemoveActiveNode(this);
        }

        public void Stop()
        {
            Reset();
            Remove();
        }

        public void Update(float deltaTime)
        {
            ElapsedTime += deltaTime;
            for (int i = 0; i < AffectorList.Count; i++)
            {
                Affector aft = AffectorList[i];
                aft.Update(deltaTime);
            }

            Position += Velocity * deltaTime;
            if (SyncClient)
                CurWorldPos = Position + GetRealClientPos();
            else
                CurWorldPos = Position;

            RenderObj.Update(deltaTime);
            if (Owner.UseShaderCurve2 || Owner.UseShaderCurve1)
            {
                float x = Owner.UseShaderCurve1 ? Owner.ShaderCurveX1.Evaluate(GetElapsedTime(), this) : 1f;
                float y = Owner.UseShaderCurve2 ? Owner.ShaderCurveX2.Evaluate(GetElapsedTime(), this) : 0f;
                RenderObj.ApplyShaderParam(x, y);
            }
            else if (Owner.UseFlowControl)
            {
                float x = Owner.FlowSpeed * GetElapsedTime();
                float y = Owner.FlowStretchLength;
                RenderObj.ApplyShaderParam(x, y);
            }

            LastWorldPos = CurWorldPos;

            if (ElapsedTime > LifeTime && LifeTime > 0)
            {
                Reset();
                Remove();
            }
        }
    }
}