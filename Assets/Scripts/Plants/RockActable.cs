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
        Rigidbody rb = null;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
        /// <summary>
        /// 押された距離
        /// </summary>
        float pushX = 0f;

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
            if (!CanAction) return;

            Debug.Log($"  pushed");

            pushX += StellaMove.myVelocity.x * Time.fixedDeltaTime;
        }

        private void FixedUpdate()
        {
            if (Mathf.Approximately(pushX, 0f))
            {
                // 押していなければZ回転なし
                rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
                return;
            }
            else
            {
                rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
            }

            float zrot = pushX / SphereColliderInstance.radius;
            transform.Rotate(0, 0, -zrot * Mathf.Rad2Deg);
            Vector3 v = rb.velocity;
            v.x = pushX;
            rb.velocity = v;
            pushX = 0f;
        }
    }
}