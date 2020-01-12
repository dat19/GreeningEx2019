using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Rock Walk", fileName = "StellaActionRockWalk")]
    public class StellaRockWalk : StellaWalk
    {
        [Tooltip("速度を変える中心からの距離"), SerializeField]
        float changeDistance = 0.2f;
        [Tooltip("速度"), SerializeField]
        float[] rockSpeed = { 0.5f, 1f };
        [Tooltip("降りるチェックをする中心からの距離"), SerializeField]
        float getOffDistance = 0.1f;

        GameObject lastRockObject = null;
        RockActable rockActable;
        SphereCollider rockCollider;

        /// <summary>
        /// 岩が足元にないことを確認するためにチェックする高さ。降りてないのに落ちてしまうなら、この値を増やす
        /// </summary>
        const float CheckFallHeight = 1f;

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
            StellaMove.myVelocity.x = h * StellaMove.MoveSpeed;

            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            // 逆再生設定
            bool back = h * StellaMove.forwardVector.x < -0.5f;
            StellaMove.SetAnimBool("Back", back);

            if (!StellaMove.chrController.isGrounded)
            {
                /// 下に岩があればセーフ
                int hitCount = PhysicsCaster.CharacterControllerCast(
                    StellaMove.chrController, Vector3.down, CheckFallHeight, PhysicsCaster.MapCollisionLayer);
                bool isRock = false;
                for (int i = 0; i < hitCount; i++)
                {
                    if (PhysicsCaster.hits[i].collider.GetComponent<RockActable>())
                    {
                        isRock = true;
                        break;
                    }
                }

                if (!isRock)
                {
                    StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                    FallNextBlock();
                }
            }
            else
            {
                // 真下が岩以外なら、他の行動へ
                if (NotOnRock())
                {
                    StellaMove.instance.ChangeToWalk();
                }

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
                    if (dist < -changeDistance)
                    {
                        StellaMove.myVelocity.x = -rockSpeed[1];
                    }
                    else if (dist > changeDistance)
                    {
                        StellaMove.myVelocity.x = rockSpeed[1];
                    }
                    else
                    {
                        StellaMove.myVelocity.x = rockSpeed[0]*Mathf.Sign(dist);
                    }

                    // ステラが移動できるか確認
                    int hitCount = PhysicsCaster.CharacterControllerCast(
                        StellaMove.chrController,
                        StellaMove.myVelocity.x < 0f ? Vector3.left : Vector3.right,
                        Mathf.Abs(StellaMove.myVelocity.x)*Time.fixedDeltaTime,
                        PhysicsCaster.MapCollisionLayer);
                    bool blocked = false;
                    for (int i=0; i<hitCount;i++)
                    {
                        if (PhysicsCaster.hits[i].collider.gameObject != lastRockObject)
                        {
                            // ぶつかるなら玉乗りキャンセル
                            StellaMove.myVelocity.x = 0f;
                            blocked = true;
                            break;
                        }
                    }

                    if (!blocked)
                    {
                        StellaMove.chrController.enabled = false;
                        rockActable.PushAction();

                        // ステラの座標を修正する
                        Vector3 stellaMove = Vector3.zero;
                        float moved = lastRockObject.transform.position.x - lastPos.x;
                        float rad = moved / rockCollider.radius;
                        stellaMove.Set(
                            moved + Mathf.Sin(rad) * rockCollider.radius,
                            -StellaMove.chrController.stepOffset, 0);
                        StellaMove.chrController.enabled = true;
                        StellaMove.chrController.Move(stellaMove);

                        // 足元に岩が無ければ飛び降りる
                        if (!CheckGetOff())
                        {
                            return;
                        }
                    }
                }

                // 操作していればジャンプチェック
                if (!Mathf.Approximately(h, 0f))
                {
                    StellaMove.instance.CheckMiniJump();
                }
            }
        }

        /// <summary>
        /// 真下が岩以外なら、trueを返します、
        /// </summary>
        bool NotOnRock()
        {
            int hitCount = PhysicsCaster.CharacterControllerCast(StellaMove.chrController, Vector3.down, getOffDistance, PhysicsCaster.MapCollisionLayer);
            for (int i=0;i<hitCount;i++)
            {
                if (PhysicsCaster.hits[i].collider.GetComponent<RockActable>())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 乗っていた岩が下にあるかを確認します。
        /// </summary>
        /// <returns>岩がある時、trueを返します。</returns>
        bool CheckGetOff()
        {
            Vector3 origin = StellaMove.chrController.bounds.center;
            origin.x += getOffDistance * Mathf.Sign(lastRockObject.transform.position.x - origin.x);
            int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hits, float.PositiveInfinity, groundLayer, QueryTriggerInteraction.Collide);

            Collider res = FindRock(hitCount);
            if (res.gameObject == lastRockObject) return true;

            // 列挙が越えていたら、一番上までで再列挙
            while (hitCount >= HitMax)
            {
                hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hits, origin.y-res.bounds.max.y, groundLayer, QueryTriggerInteraction.Collide);
                res = FindRock(hitCount);
                if (res.gameObject == lastRockObject) return true;
            }

            GetOffOutside(StellaMove.chrController.bounds.min.y - res.bounds.max.y);
            return false;
        }

        /// <summary>
        /// 列挙した接触情報から岩を探します。見つからない場合は、一番上のコライダーを返します。
        /// </summary>
        /// <param name="hitCount">検出したオブジェクトの数</param>
        /// <returns>岩があれば岩のコライダー。違う場合は見つけた一番上のコライダー</returns>
        Collider FindRock(int hitCount)
        {
            float top = float.NegativeInfinity;
            Collider res = null;

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.gameObject == lastRockObject)
                {
                    return hits[i].collider;
                }

                float h = hits[i].collider.bounds.max.y;
                if (h > top)
                {
                    res = hits[i].collider;
                    top = h;
                }
            }

            return res;
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