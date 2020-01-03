using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019 {
    public class MushroomActable :Actable
    {
        CapsuleCollider capsuleCollider = null;
        CapsuleCollider CapsuleColliderInstance
        {
            get
            {
                if (capsuleCollider == null)
                {
                    capsuleCollider = GetComponent<CapsuleCollider>();
                }
                return capsuleCollider;
            }
        }

        /// <summary>
        /// 生長後に有効になる
        /// </summary>
        public override bool CanAction { 
            get
            {
                return GrowInstance.state == Grow.StateType.Growed;
            }
            protected set => base.CanAction = value; 
        }

        public override void Action()
        {
            if (!CanAction) return;

            StellaMove.targetJumpGround = transform.position;
            StellaMove.targetJumpGround.y = CapsuleColliderInstance.bounds.max.y;
            StellaMove.myVelocity.x = 0f;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Jump);
        }

        public override void PushAction()
        {
            Action();
        }
    }
}