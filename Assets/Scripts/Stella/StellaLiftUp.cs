using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create LiftUp", fileName = "StellaActionLiftUp")]
    public class StellaLiftUp : StellaActionScriptableObject
    {
        protected enum StateType
        {
            TargetWalk,
            Action,
            BackOff,
        }

        protected StateType state;
        protected float targetX;

        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            base.Init();

            state = StateType.TargetWalk;
            StellaMove.SetAnimState(StellaMove.AnimType.Walk);
            targetX = StellaMove.naePutPosition.x - StellaMove.NaePutDownOffsetX * StellaMove.forwardVector.x;
        }

        public override void UpdateAction()
        {
            switch (state)
            {
                case StateType.TargetWalk:
                    if (StellaMove.AdjustWalk(targetX, StellaWalk.MoveSpeed))
                    {
                        state = StateType.Action;
                        StellaMove.myVelocity = Vector3.zero;
                        ToAction();
                    }
                    break;
                case StateType.Action:
                    StellaMove.myVelocity.x = 0f;
                    StellaMove.instance.Move();
                    break;
            }
        }

        /// <summary>
        /// 行動へ
        /// </summary>
        protected virtual void ToAction()
        {
            StellaMove.RegisterAnimEvent(HoldNae);
            StellaMove.SetAnimState(StellaMove.AnimType.LiftUp);
        }

        void HoldNae()
        {
            StellaMove.RegisterAnimEvent(ToHold);
            ((NaeActable)ActionBox.SelectedActable).Hold();
        }

        void ToHold()
        {
            StellaMove.hasNae = true;
            StellaMove.SetAnimBool("Nae", true);
            StellaMove.RegisterAnimEvent(ToHoldWalk);
        }

        void ToHoldWalk()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.NaeWalk);
        }
    }
}
