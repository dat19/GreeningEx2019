using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create PutDown", fileName = "StellaActionPutDown")]
    public class StellaPutDown : StellaLiftUp
    {
        public override void UpdateAction()
        {
            if (state != StateType.BackOff)
            {
                base.UpdateAction();
            }
            else
            {
                if (StellaMove.AdjustWalk(targetX, StellaWalk.MoveSpeed))
                {
                    ((NaeActable)ActionBox.SelectedActable).SetCollider(true);
                    StellaMove.myVelocity.x = 0;
                    StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
                }
            }
        }

        protected override void ToAction()
        {
            StellaMove.RegisterAnimEvent(PutDownNae);
            StellaMove.RegisterAnimEvent(NaeOff);
            StellaMove.SetAnimState(StellaMove.AnimType.PutDown);
        }

        void NaeOff()
        {
            StellaMove.SetAnimBool("Nae", false);
            StellaMove.hasNae = false;
            StellaMove.RegisterAnimEvent(PutDownNae);
        }

        void PutDownNae()
        {
            StellaMove.RegisterAnimEvent(BackOff);
            ((NaeActable)ActionBox.SelectedActable).PutDown();
        }

        void BackOff()
        {
            state = StateType.BackOff;
            targetX = StellaMove.naePutPosition.x - (StellaMove.chrController.radius + ((NaeActable)ActionBox.SelectedActable).ColliderExtentsX) * StellaMove.forwardVector.x + StellaMove.CollisionMargin;
            // 前進はしない
            if ((targetX-StellaMove.instance.transform.position.x) * StellaMove.forwardVector.x > 0)
            {
                targetX = StellaMove.instance.transform.position.x;
            }
            StellaMove.SetAnimState(StellaMove.AnimType.Walk);
        }
    }
}
