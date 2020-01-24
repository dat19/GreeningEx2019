using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create PutDown", fileName = "StellaActionPutDown")]
    public class StellaPutDown : StellaLiftUpPutDownBase
    {
        public override void Init()
        {
            base.Init();

            state = StateType.TargetWalk;
            StellaMove.SetAnimState(StellaMove.AnimType.Walk);
            targetX = StellaMove.naePutPosition.x - (StellaMove.NaePutDownOffsetX + StellaMove.naeActable.NaeOffsetX) * StellaMove.forwardVector.x;
        }

        public override void UpdateAction()
        {
            if (state != StateType.BackOff)
            {
                base.UpdateAction();
            }
            else
            {
                // 調整を終えて、苗を下す
                if (StellaMove.AdjustWalk(targetX, StellaMove.MoveSpeed) != StellaMove.AdjustWalkResult.Continue)
                {
                    ToWalk();
                }
            }
        }

        protected override void ToAction()
        {
            StellaMove.RegisterAnimEvent(PutDownNae);
            StellaMove.RegisterAnimEvent(NaeOff);
            StellaMove.SetAnimState(StellaMove.AnimType.PutDown);
        }

        // 苗を離す
        void NaeOff()
        {
            StellaMove.SetAnimBool("Nae", false);
            StellaMove.RegisterAnimEvent(PutDownNae);
        }

        void PutDownNae()
        {
            SoundController.Play(SoundController.SeType.PutDown);
            StellaMove.RegisterAnimEvent(BackOff);
            StellaMove.naeActable.PutDown();
        }

        void BackOff()
        {
            state = StateType.BackOff;
            targetX = StellaMove.naePutPosition.x - (StellaMove.chrController.radius + StellaMove.naeActable.ColliderExtentsX) * StellaMove.forwardVector.x + StellaMove.CollisionMargin;
            // 前進はしない
            if ((targetX-StellaMove.instance.transform.position.x) * StellaMove.forwardVector.x > 0)
            {
                targetX = StellaMove.instance.transform.position.x;
            }
            StellaMove.SetAnimState(StellaMove.AnimType.Walk);
        }
    }
}
