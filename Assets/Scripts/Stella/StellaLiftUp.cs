using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create LiftUp", fileName = "StellaActionLiftUp")]
    public class StellaLiftUp : StellaLiftUpPutDownBase
    {
        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            base.Init();

            state = StateType.TargetWalk;
            StellaMove.SetAnimState(StellaMove.AnimType.Walk);
            StellaMove.naeActable = ((NaeActable)ActionBox.SelectedActable);
            targetX = StellaMove.naePutPosition.x - (StellaMove.NaePutDownOffsetX + StellaMove.naeActable.NaeOffsetX) * StellaMove.forwardVector.x;
        }

        /// <summary>
        /// 行動へ
        /// </summary>
        protected override void ToAction()
        {
            StellaMove.RegisterAnimEvent(HoldNae);
            StellaMove.SetAnimState(StellaMove.AnimType.LiftUp);
        }

        void HoldNae()
        {
            StellaMove.RegisterAnimEvent(ToHold);
            StellaMove.naeActable.Hold();
        }

        void ToHold()
        {
            StellaMove.SetAnimBool("Nae", true);
            StellaMove.RegisterAnimEvent(ToHoldWalk);
        }

        void ToHoldWalk()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.NaeWalk);
        }
    }
}
