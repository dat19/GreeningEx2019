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
        }

        static StateType state;

        /// <summary>
        /// ターン後の状態
        /// </summary>
        StateType afterTurnState;

        /// <summary>
        /// 歩く目的地X
        /// </summary>
        float targetX;

        /// <summary>
        /// 星に捕まった時のHoldPositionからのオフセット座標
        /// </summary>
        public static Vector3 OffsetFromStar { get; private set; }

        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            base.Init();

            StellaMove.SetAnimState(StellaMove.AnimType.Walk);

            targetX = Goal.instance.transform.position.x + Mathf.Sign(Goal.ClearFlyX) * StageManager.GoalToStellaOffset.x;
            state = StateType.ToTarget;
            float dir = targetX - StellaMove.instance.transform.position.x;

            // 向きが逆の時、方向転換から
            if ((dir * StellaMove.forwardVector.x) < 0f)
            {
                StellaMove.instance.StartTurn(dir);
                afterTurnState = StateType.ToTarget;
                state = StateType.Turn;
            }
            else
            {
                state = StateType.ToTarget;
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
                    if (StellaMove.chrController.isGrounded)
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

                case StateType.HoldStar:
                    Vector3 ofs = StellaMove.instance.transform.position - StellaMove.HoldPosition;
                    StellaMove.instance.transform.position = Goal.StarPosition + OffsetFromStar + ofs;
                    break;
            }
        }

        /// <summary>
        /// 星に捕まる
        /// </summary>
        public static void HoldStar()
        {
            StellaMove.RegisterAnimEvent(AttachStar);
            // 左向きの時、Xを逆にする
            Vector3 sc = Vector3.one;
            sc.x = StellaMove.forwardVector.x;
            StellaMove.instance.transform.GetChild(0).localScale = sc;
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
    }
}
