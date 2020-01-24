using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class FluffActable : Actable
    {
        [Tooltip("発生してから、これ以上高くなったら消す"), SerializeField]
        float removeHeight = 10;
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
        /// オブジェクトの座標から、ステラの手の座標へのオフセット高さ
        /// </summary>
        float holdOffsetY;

        private void Awake()
        {
            state = StateType.Spawn;
            boxCollider = GetComponent<BoxCollider>();
        }

        public void Init(Vector2 vel, float lf)
        {
            rb = GetComponent<Rigidbody>();
            rb.velocity = vel;
            lifeTime = lf;
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
            Debug.Log($"  Action {state}");

            if (state != StateType.Fly) return false;

            Debug.Log($"  Action 2");

            // ステラがつかむ手の高さ
            Vector3 holdPos = StellaMove.instance.transform.position;
            holdPos.x += stellaStandardHoldOffset.x * StellaMove.forwardVector.x;
            holdPos.y += stellaStandardHoldOffset.y;

            holdOffsetY = holdPos.y - transform.position.y;
            if ((holdOffsetY < stellaOffsetBottom.y) || (holdOffsetY > stellaOffsetTop.y))
            {
                Debug.Log($"  bad pos");
                return false;
            }

            Debug.Log($"  hold");
            state = StateType.Hold;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Fluff);
            return true;
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
