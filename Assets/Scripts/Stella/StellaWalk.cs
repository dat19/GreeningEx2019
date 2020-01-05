using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Walk", fileName = "StellaActionWalk")]
    public class StellaWalk : StellaActionScriptableObject
    {
        [Tooltip("ステラの横向きの角度"), SerializeField]
        float rotateY = 40f;
        [Tooltip("方向転換秒数"), SerializeField]
        float turnSeconds = 0.15f;

        /// <summary>
        /// 移動速度(秒速)
        /// </summary>
        public const float MoveSpeed = 3.5f;

        protected enum StateType
        {
            Walk,
            Turn,
        }

        protected StateType state = StateType.Walk;
        float stateStartTime;

        public override void Init()
        {
            base.Init();
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
                return;
            }
            else if (StellaMove.CheckIvyHold())
            {
                return;
            }
            else
            {
                // 行動ボタンチェック
                if (Input.GetButton("Action"))
                {
                    Actable act = StellaMove.ActionBoxInstance.GetActableInstance();
                    if (act != null)
                    {
                        if (act.Action())
                        {
                            return;
                        }
                    }
                }

                Walk();
            }

            bool isBack = PushCheck();
            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            if (!StellaMove.chrController.isGrounded)
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                FallNextBlock();
            }
            else
            {
                // 乗っかりチェック
                StellaMove.CheckStepOn();

                // 移動しているなら、ジャンプチェック
                if (!isBack && !Mathf.Approximately(StellaMove.myVelocity.x, 0))
                {
                    StellaMove.instance.CheckMiniJump();
                }
            }
        }

        /// <summary>
        /// 歩いたり止まったりします。継続時はtrue、ターンに移行するのでこれ以降の処理が不要な時、falseを返します。
        /// </summary>
        /// <returns>歩き継続の時、true</returns>
        protected bool Walk()
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
                return false;
            }

            // 左右の移動速度(秒速)を求める
            StellaMove.myVelocity.x = h * MoveSpeed;

            return true;
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

            int cnt = Physics.RaycastNonAlloc(origin, Vector3.down, hits, float.PositiveInfinity, StellaMove.MapCollisionLayerMask);
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
            float top = hits[0].collider.bounds.max.y;
            for (int i = 1; i < cnt; i++)
            {
                // 地面と水以外は対象外
                if (!hits[i].collider.CompareTag("Ground")
                    && !hits[i].collider.CompareTag("Water")) continue;
                if (hits[i].collider.bounds.max.y > top)
                {
                    top = hits[i].collider.bounds.max.y;
                }
            }
            float h = StellaMove.chrController.bounds.min.y - top;
            // 高さがないか、負の値の時は、1段分で算出
            if (h <= 0f)
            {
                h = 1f;
            }

            float t = Mathf.Sqrt(2f * h / StellaMove.GravityAdd);
            StellaMove.myVelocity.x = (origin.x - StellaMove.instance.transform.position.x) / t;
            if (StellaMove.myVelocity.x*StellaMove.forwardVector.x < 0f)
            {
                StellaMove.myVelocity.x = 0;
            }
        }

        /// <summary>
        /// 移動先のチェックを行ってPushActionを呼び出します。
        /// また、岩にめり込む対策のため、移動方向で埋まっていたら、埋まらない場所まで戻します。
        /// </summary>
        /// <returns>戻す処理をしていたらtrueを返します。</returns>
        protected bool PushCheck()
        {
            float h = StellaMove.chrController.height * 0.5f - StellaMove.chrController.radius;
            Vector3 p1 = StellaMove.chrController.bounds.center + Vector3.up * h;
            Vector3 p2 = StellaMove.chrController.bounds.center + Vector3.down * h;
            int hitCount = Physics.CapsuleCastNonAlloc(p1,p2,
                StellaMove.chrController.radius,
                StellaMove.forwardVector,
                hits,
                Mathf.Abs(StellaMove.myVelocity.x*Time.fixedDeltaTime),
                groundLayer
                );
            bool isBack = false;

            for (int i=0; i<hitCount;i++)
            {
                Actable[] acts = hits[i].collider.GetComponents<Actable>();
                for (int j=0;j<acts.Length;j++)
                {
                    if (!acts[j].CanAction) continue;

                    // 方向が左右か
                    float ydif = hits[i].point.y - StellaMove.chrController.bounds.center.y;
                    float sidecheck = h + StellaMove.chrController.radius * 0.5f;
                    if (Mathf.Abs(ydif) > sidecheck)
                    {
                        continue;
                    }

                    // 移動していて、移動先にオブジェクトがある場合、押す
                    float to = hits[i].collider.bounds.center.x - StellaMove.instance.transform.position.x;
                    if (StellaMove.myVelocity.x * to > 0f)
                    {
                        if (!acts[j].PushAction()) continue;
                    }

                    // 向いている方向に対象物がある時、対象物に触れていたらステラを下げる
                    float range = StellaMove.chrController.bounds.extents.x + hits[i].collider.bounds.extents.x + StellaMove.CollisionMargin;
                    if (((to * StellaMove.forwardVector.x) > 0f) && (Mathf.Abs(to) < range))
                    {
                        float posx = hits[i].collider.bounds.center.x - range * StellaMove.forwardVector.x;
                        StellaMove.myVelocity.x = (posx - StellaMove.instance.transform.position.x) / Time.fixedDeltaTime;
                        isBack = true;
                    }
                }
            }

            return isBack;
        }
    }
}
