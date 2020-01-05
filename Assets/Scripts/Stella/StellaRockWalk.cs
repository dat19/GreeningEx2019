using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Rock Walk", fileName = "StellaActionRockWalk")]
    public class StellaRockWalk : StellaWalk
    {
        [Tooltip("中心からの距離を、岩に与える速度に変換する係数"), SerializeField]
        float distanceToSpeed = 1f;

        GameObject lastRockObject = null;
        RockActable rockActable;
        SphereCollider rockCollider;

        public override void Init()
        {
            base.Init();

            lastRockObject = StellaMove.stepOnObject;
            rockActable = lastRockObject.GetComponent<RockActable>();
            rockCollider = lastRockObject.GetComponent<SphereCollider>();
        }

        public override void UpdateAction()
        {
            // キーの入力を調べる
            float h = Input.GetAxisRaw("Horizontal");

            // 左右の移動速度(秒速)を求める
            StellaMove.myVelocity.x = h * MoveSpeed;

            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            // 逆再生設定
            bool back = h * StellaMove.forwardVector.x < -0.5f;
            StellaMove.SetAnimBool("Back", back);

            if (!StellaMove.chrController.isGrounded)
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                FallNextBlock();
            }
            else
            {
                // 岩に力を加える
                if (lastRockObject != StellaMove.stepOnObject)
                {
                    lastRockObject = StellaMove.stepOnObject;
                    rockActable = lastRockObject.GetComponent<RockActable>();
                    rockCollider = lastRockObject.GetComponent<SphereCollider>();
                }
                if (rockActable != null)
                {
                    Vector3 lastPos = lastRockObject.transform.position;
                    float dist = StellaMove.instance.transform.position.x - lastPos.x;
                    /*
                    float move = dist * distanceToSpeed;
                    */
                    float move = Mathf.Log10(Mathf.Abs(dist * distanceToSpeed) + 1);
                    move *= Mathf.Sign(dist);
                    StellaMove.myVelocity.x = move / Time.fixedDeltaTime;
                    StellaMove.chrController.enabled = false;
                    Debug.Log($"dist={dist} / move={move} / velx={StellaMove.myVelocity.x}");
                    rockActable.PushAction();
                    // ステラの座標を修正する
                    Vector3 stellaMove = Vector3.zero;
                    float moved = lastRockObject.transform.position.x - lastPos.x;
                    Debug.Log($"  moved={moved} / rockrad={rockCollider.radius}");
                    float rad = moved / rockCollider.radius;
                    stellaMove.Set(
                        moved + Mathf.Sin(rad) * rockCollider.radius,
                        -StellaMove.chrController.stepOffset, 0);
                    StellaMove.chrController.enabled = true;
                    StellaMove.chrController.Move(stellaMove);

                    // 真下に岩が無ければ飛び降りる
                    if (!CheckGetOff()) {
                        return;
                    }
                }

                // 移動しているなら、ジャンプチェック
                if (!Mathf.Approximately(StellaMove.myVelocity.x, 0))
                {
                    StellaMove.instance.CheckMiniJump();
                }
            }
        }

        /// <summary>
        /// 乗っていた岩が下にあるかを確認します。
        /// </summary>
        /// <returns>岩がある時、trueを返します。</returns>
        bool CheckGetOff()
        {
            int hitCount = Physics.RaycastNonAlloc(StellaMove.chrController.bounds.center, Vector3.down, hits, float.PositiveInfinity, groundLayer, QueryTriggerInteraction.Collide);

            float top = float.NegativeInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.gameObject == lastRockObject)
                {
                    return true;
                }

                float h = hits[i].collider.GetComponent<Collider>().bounds.max.y;
                top = Mathf.Max(top, h);
            }

            GetOffOutside(StellaMove.chrController.bounds.min.y - top);
            return false;
        }

        /// <summary>
        /// 岩からステラへの方向に降ります。
        /// </summary>
        /// <param name="top">床までの高さ</param>
        void GetOffOutside(float top)
        {
            float dir = StellaMove.chrController.bounds.center.x - lastRockObject.transform.position.x;
            float offset = StellaMove.chrController.bounds.extents.x + StellaMove.CollisionMargin + rockCollider.bounds.extents.x;
            float t = StellaMove.GetFallTime(top + StellaMove.MiniJumpMargin * 2f);
            float jumpt = StellaMove.GetFallTime(StellaMove.MiniJumpMargin);
            StellaMove.myVelocity.Set(offset * Mathf.Sign(dir) / t, jumpt * StellaMove.GravityAdd, 0f);
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
        }
    }
}