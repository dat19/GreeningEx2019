//#define DEBUG_DISP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Clear", fileName = "StellaActionClear")]
    public class StellaClear : StellaActionScriptableObject
    {
        enum StateType
        {
            Turn,
            ToTarget,
            Wait,
            HoldStar,
            ToTargetAir,
            WaitAir,
        }

        /// <summary>
        /// 空中キャッチの時、定位置に移動するまでの秒数
        /// </summary>
        const float ToCatchAirSeconds = 0.5f;

        /// <summary>
        /// 星に捕まった時のHoldPositionからのオフセット座標
        /// </summary>
        public static Vector3 OffsetFromStar { get; private set; }

        static StateType state;

        /// <summary>
        /// ターン後の状態
        /// </summary>
        StateType afterTurnState;

        /// <summary>
        /// 歩く目的地X
        /// </summary>
        float targetX;
        float startTime;
        bool isTurn;

        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            base.Init();

            targetX = Goal.StarPosition.x + Mathf.Sign(Goal.ClearFlyX) * StageManager.GoalToStellaOffset.x;
            float dir = targetX - StellaMove.instance.transform.position.x;

            if (StellaMove.ChrController.isGrounded)
            {
                StellaMove.SetAnimState(StellaMove.AnimType.Walk);
                state = StateType.ToTarget;

                // 向きが逆の時、方向転換から
                if ((dir * StellaMove.forwardVector.x) < 0f)
                {
                    StellaMove.instance.StartTurn(dir);
                    afterTurnState = StateType.ToTarget;
                    state = StateType.Turn;
                }
            }
            else
            {
                // 空中
                StellaMove.SetAnimState(StellaMove.AnimType.ClearWait);
                state = StateType.ToTargetAir;
                startTime = 0;

                // ターンチェック
                isTurn = false;
                if (Goal.ClearFlyX * StellaMove.forwardVector.x < 0) {
                    isTurn = true;
                    StellaMove.instance.StartTurn(Mathf.Sign(Goal.ClearFlyX));
                }
            }
        }

        /// <summary>
        /// 行動
        /// </summary>
        public override void UpdateAction()
        {
            switch(state)
            {
                case StateType.Turn:
                    if (StellaMove.ChrController.isGrounded)
                    {
                        // 方向を確認
                        if (StellaMove.instance.Turn())
                        {
                            state = afterTurnState;
                        }
                    }
                    else
                    {
                        // 重力確認
                        StellaMove.instance.Gravity();
                        StellaMove.instance.Move();
                    }

                    break;

                case StateType.ToTarget:
                    // 移動
                    if (StellaMove.AdjustWalk(targetX, StellaMove.MoveSpeed) == StellaMove.AdjustWalkResult.Reach) {
                        state = StateType.Wait;

                        // 星が飛び去る方向と向きが逆の時、方向転換
                        if (StellaMove.forwardVector.x * Mathf.Sign(Goal.ClearFlyX) < 0f)
                        {
                            StellaMove.instance.StartTurn(-StellaMove.forwardVector.x);
                            afterTurnState = StateType.Wait;
                            state = StateType.Turn;
                        }
                    }
                    break;

                case StateType.ToTargetAir:
                    if (isTurn)
                    {
                        isTurn = !StellaMove.instance.Turn();
                    }

                    Vector3 targetPosition = Goal.StarPosition + StageManager.GoalToStellaOffset;
                    targetPosition.x = Goal.StarPosition.x + Mathf.Sign(Goal.ClearFlyX) * StageManager.GoalToStellaOffset.x;
                    targetPosition.z = 0f;

                    startTime += Time.fixedDeltaTime;
                    float t = Mathf.Clamp01(startTime / ToCatchAirSeconds);
                    StellaMove.instance.transform.position = Vector3.Lerp(
                        StellaMove.instance.transform.position,
                        targetPosition,
                        t
                        );
                    if (t >= 1f)
                    {
                        OffsetFromStar = StellaMove.HoldPosition - Goal.StarPosition;
                        state = StateType.WaitAir;
                    }
                    Log($"  target={targetPosition} / StarPos={Goal.StarPosition} / goalToStellaOffset={StageManager.GoalToStellaOffset}");
                    break;
            }
        }

        public override void LateUpdate()
        {
            if ((state == StateType.HoldStar) || (state == StateType.WaitAir))
            {
                Vector3 ofs = StellaMove.instance.transform.position - StellaMove.HoldPosition;
                StellaMove.instance.transform.position = Goal.StarPosition + OffsetFromStar + ofs;
            }
        }

        /// <summary>
        /// 星に捕まる
        /// </summary>
        public static void HoldStar()
        {
            // 左向きの時、Xを逆にする
            Vector3 sc = Vector3.one;
            sc.x = StellaMove.forwardVector.x;
            StellaMove.instance.transform.GetChild(0).localScale = sc;

            if (state == StateType.WaitAir)
            {
                // 空中なので、すぐに飛び去りへ
                FollowStar();
                return;
            }

            StellaMove.RegisterAnimEvent(AttachStar);
            StellaMove.SetAnimState(StellaMove.AnimType.Clear);
        }

        /// <summary>
        /// 星にぶら下がる。このアニメが完了するまでは、ステラに星をくっつける
        /// </summary>
        static void AttachStar()
        {
            OffsetFromStar = StellaMove.HoldPosition - Goal.StarPosition;
            Goal.FollowStella();
            StellaMove.RegisterAnimEvent(FollowStar);
            state = StateType.Wait;
        }

        /// <summary>
        /// 飛び立つ。ステラを星にくっつける
        /// </summary>
        static void FollowStar()
        {
            // 星が飛び立つ前段階
            Goal.FlyWait();
            state = StateType.HoldStar;
        }

        [System.Diagnostics.Conditional("DEBUG_DISP")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}
