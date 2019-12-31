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

        void HoldNae()
        {
            Debug.Log($"hold nae");
            StellaMove.RegisterAnimEvent(ToHoldWalk);
            ((NaeActable)ActionBox.SelectedActable).Hold(StellaMove.ZyouroPivot);
        }

        void ToHoldWalk()
        {
            Debug.Log($"hold walk");
            StellaMove.instance.ChangeAction(StellaMove.ActionType.NaeWalk);
            StellaMove.SetAnimBool("Nae", true);
        }
    }
}
