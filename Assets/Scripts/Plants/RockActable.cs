using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019 {
    public class RockActable :Actable
    {
        SphereCollider sphereCollider = null;
        SphereCollider SphereColliderInstance
        {
            get
            {
                if (sphereCollider == null)
                {
                    sphereCollider = GetComponent<SphereCollider>();
                }
                return sphereCollider;
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
            StellaMove.targetJumpGround.y = SphereColliderInstance.bounds.max.y;
            StellaMove.myVelocity.x = 0f;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Jump);
        }

        public override void PushAction()
        {
            Debug.Log($"押す");
        }
    }
}