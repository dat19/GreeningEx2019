using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019 {
    public class RockActable :Actable
    {
        CharacterController chrController = null;
        CharacterController ChrController
        {
            get
            {
                if (chrController == null)
                {
                    chrController = GetComponent<CharacterController>();
                }
                return chrController;
            }
        }

        Vector3 myVelocity = Vector3.zero;
        SphereCollider sphereCollider;

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
            StellaMove.targetJumpGround.y = ChrController.bounds.max.y;
            StellaMove.myVelocity.x = 0f;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Jump);
        }

        public override void PushAction()
        {
            if (!CanAction)
            {
                return;
            }

            // 重力加速
            Vector3 move = Vector3.zero;
            move.x = StellaMove.myVelocity.x * Time.fixedDeltaTime;
            Vector3 lastPos = transform.position;
            if (!Mathf.Approximately(move.x, 0))
            {
                Debug.Log($"  push {move.x} frame={Time.frameCount}");
            }
            ChrController.Move(move);
            if (ChrController.collisionFlags != CollisionFlags.Below)
            {
                Debug.Log($"  flag={ChrController.collisionFlags}");
            }

            // 移動した分、回転
            float zrot = (transform.position.x - lastPos.x) / ChrController.radius;
            transform.Rotate(0, 0, -zrot * Mathf.Rad2Deg);
        }

        private void FixedUpdate()
        {
            // 当たり判定を一致させる
            if (sphereCollider == null)
            {
                sphereCollider = GetComponent<SphereCollider>();
            }
            if (ChrController.enabled)
            {
                sphereCollider.radius = ChrController.radius;
                sphereCollider.center = ChrController.center;
                sphereCollider.enabled = true;
            }

            // 苗の時は何もしない
            if (!CanAction) return;

            // 重力加速
            myVelocity.y -= StellaMove.GravityAdd * Time.fixedDeltaTime;
            ChrController.Move(myVelocity);

            // 着地チェック
            if (ChrController.isGrounded && myVelocity.y <= 0f)
            {
                myVelocity.y = 0f;
            }
        }
    }
}