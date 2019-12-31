using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create LiftUp", fileName = "StellaActionLiftUp")]
    public class StellaLiftUp : StellaActionScriptableObject
    {
        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            StellaMove.RegisterAnimEvent(HoldNae);
            StellaMove.myVelocity = Vector3.zero;
            StellaMove.SetAnimState(StellaMove.AnimType.Walk);
            StellaMove.SetAnimTrigger("LiftUp");
        }

        public override void UpdateAction()
        {
            StellaMove.myVelocity.x = 0f;
            StellaMove.instance.Move();
        }

        void HoldNae()
        {
            StellaMove.RegisterAnimEvent(ToHold);
            ((NaeActable)ActionBox.SelectedActable).Hold(StellaMove.ZyouroPivot);
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
