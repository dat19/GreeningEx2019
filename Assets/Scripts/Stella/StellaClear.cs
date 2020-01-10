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
            Jump,
            HoldStar,
        }

        StateType state;

        // ターン後の状態
        StateType afterTurnState;

        float targetX;

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

                case StateType.Wait:
                    break;
            }
        }
    }
}
