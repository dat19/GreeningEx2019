using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class FluffActable : Actable
    {
        [Tooltip("ステラへのオフセット上端"), SerializeField]
        Vector3 stellaOffsetTop = new Vector3(0, 0.85f, 0);
        [Tooltip("ステラへのオフセット下端"), SerializeField]
        Vector3 stellaOffsetBottom = new Vector3(0, 0.3f, 0);
        [Tooltip("ステラが綿毛を持つ基準座標"), SerializeField]
        Vector3 stellaStandardHoldOffset = new Vector3(0.35f, 0.47f, -0.17f);
        [Tooltip("落下速度"), SerializeField]
        float fallSpeed = -1.0f;

        /// <summary>
        /// 状態
        /// </summary>
        enum StateType
        {
            Spawn,
            Fly,
            Hold,
            Fall,
        }

        /// <summary>
        /// ステラが持つ座標の高さ
        /// </summary>
        public Vector3 HoldPosition
        {
            get
            {
                return transform.position + Vector3.up * holdOffsetY;
            }
        }

        /// <summary>
        /// ステラの暫定の手の位置
        /// </summary>
        public Vector3 StellaStandardHoldOffset
        {
            get
            {
                return stellaStandardHoldOffset;
            }
        }

        StateType state = StateType.Spawn;

        Rigidbody rb;
        float lifeTime;
        float startY;
        BoxCollider boxCollider = null;
        /// <summary>
        /// 発生してから、これ以上高くなったら消す
        /// </summary>
        float removeHeight = 10;

        /// <summary>
        /// オブジェクトの座標から、ステラの手の座標へのオフセット高さ
        /// </summary>
        float holdOffsetY;

        private void Awake()
        {
            state = StateType.Spawn;
            boxCollider = GetComponent<BoxCollider>();
        }

        public void Init(Vector2 vel, float lf, float maxh = 10)
        {
            rb = GetComponent<Rigidbody>();
            rb.velocity = vel;
            lifeTime = lf;
            removeHeight = maxh;
            startY = transform.position.y;
        }

        void FixedUpdate()
        {
            if (state == StateType.Spawn) return;

            switch (state)
            {
                case StateType.Fly:
                    if (transform.position.y - startY > removeHeight)
                    {
                        Destroy(gameObject);
                    }
                    break;

                case StateType.Hold:
                    lifeTime -= Time.fixedDeltaTime;
                    if (lifeTime <= 0f)
                    {
                        state = StateType.Fall;
                        Vector3 v = rb.velocity;
                        v.Set(0f, fallSpeed, 0);
                        rb.velocity = v;
                    }
                    break;

                case StateType.Fall:
                    // 消す確認
                    Vector3 origin = boxCollider.bounds.center;
                    origin.y = boxCollider.bounds.max.y;
                    GameObject go = PhysicsCaster.GetGroundWater(origin, Vector3.up, 1f);
                    if (go != null)
                    {
                        Destroy(gameObject);
                    }

                    break;
            }
        }

        public override bool Action()
        {
            if (state != StateType.Fly) return false;

            // ステラがつかむ手の高さ
            Vector3 holdPos = StellaMove.instance.transform.position;
            holdPos.x += stellaStandardHoldOffset.x * StellaMove.forwardVector.x;
            holdPos.y += stellaStandardHoldOffset.y;

            holdOffsetY = holdPos.y - transform.position.y;
            if ((holdOffsetY < stellaOffsetBottom.y) || (holdOffsetY > stellaOffsetTop.y))
            {
                return false;
            }

            state = StateType.Hold;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Fluff);
            return true;
        }

        /// <summary>
        /// 渡されたステラの座標(transform.position)と向きに合わせた場所に綿毛を移動させて、
        /// 持ち状態にします。
        /// </summary>
        public void SetPositionAndHold(Vector3 pos)
        {
            Vector3 holdOffset = stellaStandardHoldOffset;
            holdOffset.x = holdOffset.x * StellaMove.forwardVector.x;
            transform.position = pos - holdOffset;

            state = StateType.Hold;
        }

        /// <summary>
        /// 落下にします。
        /// </summary>
        public void SetFall()
        {
            state = StateType.Fall;
        }

        /// <summary>
        /// Spawnアニメが終わったら、アニメからこのメソッドを呼び出します。
        /// </summary>
        public void ToFly()
        {
            CanAction = true;
            state = StateType.Fly;
        }

        /// <summary>
        /// 離す
        /// </summary>
        public void Release()
        {
            if (state == StateType.Hold)
            {
                state = StateType.Fly;
            }
        }
    }
}
