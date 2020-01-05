using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019 {
    public class RockActable :Actable
    {
        [Tooltip("真下に地面がない時の移動速度"), SerializeField]
        float rollSpeed = 0.75f;

        /// <summary>
        /// ステラの速度より少し速く押して、ひっかかりをなくす
        /// </summary>
        const float PushRate = 1.1f;

        /// <summary>
        /// 地面を調べる時の距離
        /// </summary>
        const float GroundCheckDistance = 0.01f;

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

        public override bool PushAction()
        {
            if (!CanAction)
            {
                return false;
            }

            // 重力加速
            Vector3 move = Vector3.zero;
            move.x = StellaMove.myVelocity.x * Time.fixedDeltaTime * PushRate;
            Vector3 lastPos = transform.position;
            ChrController.Move(move);
            if (transform.position.y > lastPos.y)
            {
                // 持ちあがる時は押しキャンセル
                transform.position = lastPos;
            }

            // 移動した分、回転
            setRotate(lastPos.x);
            return true;
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

            // 生長中、地面のめり込みを防ぐ
            if (GrowInstance.state == Grow.StateType.Growing)
            {
                GameObject go = PhysicsCaster.GetGround(chrController.bounds.center, chrController.bounds.extents.y);
                if (go != null)
                {
                    float targetY = go.GetComponent<Collider>().bounds.max.y + chrController.bounds.extents.y;
                    if (transform.position.y < targetY)
                    {
                        transform.Translate(0, targetY - transform.position.y, 0);
                    }
                }
            }

            // 苗の時はここまで
            if (!CanAction) return;

            // 真下に地面がない場合、自動的に転がす
            Vector3 origin = chrController.bounds.center;
            float dist = chrController.bounds.extents.y + GroundCheckDistance;
            GameObject ground = PhysicsCaster.GetGroundWater(origin, dist);
            if (ground == null)
            {
                origin.x = chrController.bounds.min.x;
                ground = PhysicsCaster.GetGroundWater(origin, dist);
                myVelocity.x = 0;
                if (ground != null)
                {
                    myVelocity.x += rollSpeed;
                }

                origin.x = chrController.bounds.max.x;
                ground = PhysicsCaster.GetGroundWater(origin, dist);
                if (ground != null)
                {
                    myVelocity.x -= rollSpeed;
                }
            }

            // 重力加速
            myVelocity.y -= StellaMove.GravityAdd * Time.fixedDeltaTime;
            Vector3 lastPos = transform.position;
            ChrController.Move(myVelocity * Time.fixedDeltaTime);
            setRotate(lastPos.x);

            // 着地チェック
            if (ChrController.isGrounded && myVelocity.y <= 0f)
            {
                myVelocity.y = 0f;
            }
        }

        /// <summary>
        /// 前回のX座標を指定して、現在のX座標に移動するのに必要な回転をさせます。
        /// </summary>
        /// <param name="lastPosX">前回のX座標</param>
        void setRotate(float lastPosX)
        {
            float zrot = (transform.position.x - lastPosX) / ChrController.radius;
            transform.Rotate(0, 0, -zrot * Mathf.Rad2Deg);
        }

    }
}