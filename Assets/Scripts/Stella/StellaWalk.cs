﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Walk", fileName = "StellaActionWalk")]
    public class StellaWalk : StellaActionScriptableObject
    {
        protected enum StateType
        {
            Walk,
            Turn,
        }

        protected StateType state = StateType.Walk;

        public override void Init()
        {
            base.Init();
            StellaMove.SetAnimState(StellaMove.AnimType.Walk);
            StellaMove.SetAnimBool("Back", false);
            state = StateType.Walk;
        }

        public override void UpdateAction()
        {
            // ターン中処理
            if (state == StateType.Turn)
            {
                if (StellaMove.instance.Turn())
                {
                    state = StateType.Walk;
                }
                return;
            }

            // 水まきチェック
            if (Input.GetButton("Water"))
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Water);
                return;
            }
            else if (StellaMove.CheckIvyHold())
            {
                return;
            }
            else
            {
                // 行動ボタンチェック
                if (Input.GetButton("Action"))
                {
                    Actable act = StellaMove.ActionBoxInstance.GetActableInstance();
                    if (act != null)
                    {
                        if (act.Action())
                        {
                            return;
                        }
                    }
                }

                Walk();
            }

            bool isBack = PushCheck();
            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            if (!StellaMove.ChrController.isGrounded)
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                FallNextBlock();
            }
            else
            {
                // 乗っかりチェック
                StellaMove.CheckStepOn();

                // 移動しているなら、ジャンプチェック
                if (!isBack && !Mathf.Approximately(StellaMove.myVelocity.x, 0))
                {
                    StellaMove.instance.CheckMiniJump();
                }
            }
        }

        /// <summary>
        /// 歩いたり止まったりします。継続時はtrue、ターンに移行するのでこれ以降の処理が不要な時、falseを返します。
        /// </summary>
        /// <returns>歩き継続の時、true</returns>
        protected bool Walk()
        {
            // キーの入力を調べる
            float h = Input.GetAxisRaw("Horizontal");

            // 方向転換チェック
            if ((h * StellaMove.forwardVector.x) < -0.5f)
            {
                state = StateType.Turn;
                StellaMove.instance.StartTurn(-StellaMove.forwardVector.x);
                return false;
            }

            // 左右の移動速度(秒速)を求める
            StellaMove.myVelocity.x = h * StellaMove.MoveSpeed;

            return true;
        }

        /// <summary>
        /// 歩きから落下する時に、隣のブロックの中心辺りに着地できるようなX速度を設定
        /// </summary>
        public void FallNextBlock()
        {
            // 着地目標のX座標を求める
            Vector3 origin = Vector3.zero;
            origin.x = Mathf.Round(StellaMove.instance.transform.position.x);

            // 着地目標のX座標と自分の足元のYから下方向にレイを飛ばして、着地点を見つける
            origin.y = StellaMove.ChrController.bounds.min.y;

            int cnt = PhysicsCaster.Raycast(origin, Vector3.down, float.PositiveInfinity, PhysicsCaster.MapCollisionPlayerOnlyLayer);
            if (cnt == 0)
            {
#if UNITY_EDITOR
                // 本来ないはずだが念のため
                Debug.Log($"地面無し {cnt}");
#endif
                StellaMove.myVelocity.x = 0;
                return;
            }

            // 一番上を探す
            float top = PhysicsCaster.hits[0].collider.bounds.max.y;
            for (int i = 1; i < cnt; i++)
            {
                // 地面と水以外は対象外
                if (!PhysicsCaster.hits[i].collider.CompareTag("Ground")
                    && !PhysicsCaster.hits[i].collider.CompareTag("Water")) continue;
                if (PhysicsCaster.hits[i].collider.bounds.max.y > top)
                {
                    top = PhysicsCaster.hits[i].collider.bounds.max.y;
                }
            }
            float h = StellaMove.ChrController.bounds.min.y - top;
            // 高さがないか、負の値の時は、1段分で算出
            if (h <= 0f)
            {
                h = 1f;
            }

            float t = Mathf.Sqrt(2f * h / StellaMove.GravityAdd);
            StellaMove.myVelocity.x = (origin.x - StellaMove.instance.transform.position.x) / t;
            if (StellaMove.myVelocity.x*StellaMove.forwardVector.x < 0f)
            {
                StellaMove.myVelocity.x = 0;
            }
        }

        /// <summary>
        /// 移動先のチェックを行ってPushActionを呼び出します。
        /// また、岩にめり込む対策のため、移動方向で埋まっていたら、埋まらない場所まで戻します。
        /// </summary>
        /// <returns>戻す処理をしていたらtrueを返します。</returns>
        protected bool PushCheck()
        {
            float h = StellaMove.ChrController.height * 0.5f - StellaMove.ChrController.radius;
            int hitCount = PhysicsCaster.CharacterControllerCast(
                StellaMove.ChrController,
                StellaMove.forwardVector,
                Mathf.Abs(StellaMove.myVelocity.x * Time.fixedDeltaTime),
                PhysicsCaster.MapCollisionLayer);
            bool isBack = false;

            for (int i=0; i<hitCount;i++)
            {
                Actable[] acts = PhysicsCaster.hits[i].collider.GetComponents<Actable>();
                for (int j=0;j<acts.Length;j++)
                {
                    if (!acts[j].CanAction) continue;

                    // 方向が左右か
                    float ydif = PhysicsCaster.hits[i].point.y - StellaMove.ChrController.bounds.center.y;
                    float sidecheck = h + StellaMove.ChrController.radius * 0.5f;
                    if (Mathf.Abs(ydif) > sidecheck)
                    {
                        continue;
                    }

                    // 移動していて、移動先にオブジェクトがある場合、押す
                    float to = PhysicsCaster.hits[i].collider.bounds.center.x - StellaMove.instance.transform.position.x;
                    if (StellaMove.myVelocity.x * to > 0f)
                    {
                        if (!acts[j].PushAction()) continue;
                    }

                    // 向いている方向に対象物がある時、対象物に触れていたらステラを下げる
                    float range = StellaMove.ChrController.bounds.extents.x + PhysicsCaster.hits[i].collider.bounds.extents.x + StellaMove.CollisionMargin;
                    if (((to * StellaMove.forwardVector.x) > 0f) && (Mathf.Abs(to) < range))
                    {
                        float posx = PhysicsCaster.hits[i].collider.bounds.center.x - range * StellaMove.forwardVector.x;
                        StellaMove.myVelocity.x = (posx - StellaMove.instance.transform.position.x) / Time.fixedDeltaTime;
                        isBack = true;
                    }
                }
            }

            return isBack;
        }
    }
}
