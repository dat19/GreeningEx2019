using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Walk", fileName = "StellaActionWalk")]
    public class StellaWalk : StellaActionScriptableObject
    {
        [Tooltip("移動速度(秒速)"), SerializeField]
        float moveSpeed = 3.5f;
        [Tooltip("ステラの横向きの角度"), SerializeField]
        float rotateY = 40f;
        [Tooltip("方向転換秒数"), SerializeField]
        float turnSeconds = 0.15f;

        protected enum StateType
        {
            Walk,
            Turn,
        }

        protected StateType state = StateType.Walk;
        const int RaycastHitMax = 8;
        RaycastHit[] raycastHits = new RaycastHit[RaycastHitMax];
        float stateStartTime;

        public override void Init()
        {
            StellaMove.SetAnimState(StellaMove.AnimType.Walk);
            state = StateType.Walk;
        }

        public override void UpdateAction()
        {
            // ターン中処理
            if (state == StateType.Turn)
            {
                Turn();
                return;
            }

            // 水まきチェック
            if (Input.GetButton("Water"))
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Water);
            }
            else
            {
                // 行動ボタンチェック
                if (Input.GetButton("Action"))
                {
                    Actable act = StellaMove.ActionBoxInstance.GetActableInstance();
                    if (act != null)
                    {
                        act.Action();
                        return;
                    }
                }

                Walk();
            }

            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            if (!StellaMove.chrController.isGrounded)
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                FallNextBlock();
            }
            else
            {
                StellaMove.instance.CheckMiniJump();
            }
        }

        /// <summary>
        /// 歩いたり止まったりします。
        /// </summary>
        protected void Walk()
        {
            // キーの入力を調べる
            float h = Input.GetAxisRaw("Horizontal");

            // 方向転換チェック
            if ((h * StellaMove.forwardVector.x) < -0.5f)
            {
                state = StateType.Turn;
                stateStartTime = Time.time - Time.fixedDeltaTime;
                StellaMove.myVelocity.x = 0f;
                Turn();
                return;
            }

            // 左右の移動速度(秒速)を求める
            float vx = h * moveSpeed;

            // 動かす
            StellaMove.myVelocity.x = vx;

            Vector3 e = StellaMove.Pivot.eulerAngles;
            if (h < -0.5f)
            {
                e.y = rotateY;
                StellaMove.forwardVector = Vector3.left;
            }
            else if (h > 0.5f)
            {
                e.y = -rotateY;
                StellaMove.forwardVector = Vector3.right;
            }
            StellaMove.Pivot.eulerAngles = e;
        }

        /// <summary>
        /// ターン処理
        /// </summary>
        protected void Turn()
        {
            Vector3 e = StellaMove.Pivot.eulerAngles;
            if (StellaMove.forwardVector.x > -0.5f)
            {
                e.y = rotateY;
            }
            else
            {
                e.y = -rotateY;
            }

            float delta = (Time.time - stateStartTime) / turnSeconds;

            if (delta >= 1f)
            {
                delta = 1f;
                StellaMove.forwardVector.x = -Mathf.Sign(e.y);
                state = StateType.Walk;
            }

            e.y = Mathf.LerpAngle(-e.y, e.y, delta);
            StellaMove.Pivot.eulerAngles = e;
        }

        /// <summary>
        /// 歩きから落下する時に、隣のブロックの中心辺りに着地できるようなX速度を設定
        /// </summary>
        public void FallNextBlock()
        {
            // 着地目標のX座標を求める
            Vector3 origin = Vector3.zero;
            origin.x = Mathf.Round(StellaMove.instance.transform.position.x);

            // 着地目標のX座標と自分の足元のYから下方向にレイを飛ばして、着地点を見つける
            origin.y = StellaMove.chrController.bounds.min.y;

            int cnt = Physics.RaycastNonAlloc(origin, Vector3.down, raycastHits, float.PositiveInfinity, StellaMove.MapCollisionLayerMask);
            if (cnt == 0)
            {
#if UNITY_EDITOR
                // 本来ないはずだが念のため
                Debug.Log($"地面無し");
#endif
                StellaMove.myVelocity.x = 0;
                return;
            }

            // 一番上を探す
            float top = raycastHits[0].collider.bounds.max.y;
            for (int i = 1; i < cnt; i++)
            {
                if (raycastHits[i].collider.bounds.max.y > top)
                {
                    top = raycastHits[i].collider.bounds.max.y;
                }
            }
            float h = StellaMove.chrController.bounds.min.y - top;
            float t = Mathf.Sqrt(2f * h / StellaMove.GravityAdd);
            StellaMove.myVelocity.x = (origin.x - StellaMove.instance.transform.position.x) / t;
        }
    }
}
