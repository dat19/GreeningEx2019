//#define DEBUG_STELLA_POSITION

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
            Log($"A: Stella={StellaMove.instance.transform.position.x}, {StellaMove.instance.transform.position.y} {StellaMove.myVelocity.x}, {StellaMove.myVelocity.y} rock={rockActable.transform.position.x}, {rockActable.transform.position.y}");
            StellaMove.instance.Move();
            Log($"B: Stella={StellaMove.instance.transform.position.x}, {StellaMove.instance.transform.position.y} isGrounded={StellaMove.ChrController.isGrounded}");

            // 逆再生設定
            bool back = h * StellaMove.forwardVector.x < -0.5f;
            StellaMove.SetAnimBool("Back", back);

            if (!StellaMove.ChrController.isGrounded)
            {
                /// 下に岩があればセーフ
                int hitCount = PhysicsCaster.CharacterControllerCast(
                    StellaMove.ChrController, Vector3.down, CheckFallHeight, PhysicsCaster.MapCollisionLayer);
                bool isRock = false;
                for (int i = 0; i < hitCount; i++)
                {
                    if (PhysicsCaster.hits[i].collider.CompareTag("Rock"))
                    {
                        isRock = true;
                        break;
                    }
                }

                if (!isRock)
                {
                    StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                    FallNextBlock();
                    Log($"C: Stella={StellaMove.instance.transform.position.x}, {StellaMove.instance.transform.position.y}");
                }
            }
            else
            {
                // 真下が岩以外なら、他の行動へ
                if (NotOnRock())
                {
                    StellaMove.instance.ChangeToWalk();
                    Log($"D: Stella={StellaMove.instance.transform.position.x}, {StellaMove.instance.transform.position.y}");
                }

                // 乗り換えチェック
                if (lastRockObject != StellaMove.stepOnObject)
                {
                    Log($"  乗り換え");
                    lastRockObject = StellaMove.stepOnObject;
                    rockActable = lastRockObject.GetComponent<RockActable>();
                    rockCollider = lastRockObject.GetComponent<SphereCollider>();
                }

                // 岩に力を加える
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
                        StellaMove.ChrController,
                        StellaMove.myVelocity.x < 0f ? Vector3.left : Vector3.right,
                        Mathf.Abs(StellaMove.myVelocity.x)*Time.fixedDeltaTime,
                        PhysicsCaster.MapCollisionLayer);
                    bool blocked = false;
                    for (int i=0; i<hitCount;i++)
                    {
                        if (PhysicsCaster.hits[i].collider.gameObject != lastRockObject)
                        {
                            // ぶつかるなら移動キャンセル
                            StellaMove.myVelocity.x = 0f;
                            blocked = true;
                            break;
                        }
                    }

                    // 岩を移動させる
                    if (!blocked)
                    {
                        StellaMove.ChrController.enabled = false;
                        rockActable.PushAction();
                        Log($"F: Stella={StellaMove.instance.transform.position.x}, {StellaMove.instance.transform.position.y}");

                        // ステラの座標を修正する
                        Vector3 stellaMove = Vector3.zero;
                        float moved = lastRockObject.transform.position.x - lastPos.x;
                        float rad = moved / rockCollider.radius;
                        stellaMove.Set(
                            moved + Mathf.Sin(rad) * rockCollider.radius,
                            StellaMove.ChrController.skinWidth * 1.5f,
                            0f);
                        Log($"  moved={moved}={lastRockObject.transform.position.x}-{lastPos.x} / rad={rad} / sin={Mathf.Sin(rad)} / rockrad={rockCollider.radius} / Stella={StellaMove.instance.transform.position.x}, {StellaMove.instance.transform.position.y}");
                        StellaMove.ChrController.enabled = true;
                        StellaMove.ChrController.Move(stellaMove);
                        Log($"  moved2 Stella={StellaMove.instance.transform.position.x}, {StellaMove.instance.transform.position.y}");
                        // 着地
                        stellaMove.Set(0, -StellaMove.ChrController.stepOffset, 0);
                        StellaMove.ChrController.Move(stellaMove);
                        Log($"G: Stella={StellaMove.instance.transform.position.x}, {StellaMove.instance.transform.position.y} move={stellaMove.x}, {stellaMove.y} isGrounded={StellaMove.ChrController.isGrounded} / rock={rockActable.transform.position.x}, {rockActable.transform.position.y}");

                        // 足元に岩が無ければ飛び降りる
                        if (!CheckGetOff())
                        {
                            Log($"H: {StellaMove.instance.transform.position}");
                            return;
                        }
                    }
                }

                // 操作していればジャンプチェック
                if (!Mathf.Approximately(h, 0f))
                {
                    StellaMove.instance.CheckMiniJump();
                    Log($"I: {StellaMove.instance.transform.position}");
                }
            }
        }

        /// <summary>
        /// 真下が岩以外なら、trueを返します、
        /// </summary>
        bool NotOnRock()
        {
            int hitCount = PhysicsCaster.CharacterControllerCast(StellaMove.ChrController, Vector3.down, getOffDistance, PhysicsCaster.MapCollisionLayer);
            for (int i=0;i<hitCount;i++)
            {
                if (PhysicsCaster.hits[i].collider.CompareTag("Rock"))
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
            Vector3 origin = StellaMove.ChrController.bounds.center;
            origin.x += getOffDistance * Mathf.Sign(lastRockObject.transform.position.x - origin.x);
            int hitCount = PhysicsCaster.Raycast(origin, Vector3.down, float.PositiveInfinity, PhysicsCaster.MapCollisionLayer, QueryTriggerInteraction.Collide);
            Collider res = FindRock(hitCount);
            if (res.gameObject == lastRockObject) return true;

            // 列挙が越えていたら、一番上までで再列挙
            while (hitCount >= PhysicsCaster.hits.Length)
            {
                hitCount = PhysicsCaster.Raycast(origin, Vector3.down, origin.y - res.bounds.max.y, PhysicsCaster.MapCollisionLayer, QueryTriggerInteraction.Collide);
                res = FindRock(hitCount);
                if (res.gameObject == lastRockObject) return true;
            }

            GetOffOutside(StellaMove.ChrController.bounds.min.y - res.bounds.max.y);
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
                if (PhysicsCaster.hits[i].collider.gameObject == lastRockObject)
                {
                    return PhysicsCaster.hits[i].collider;
                }

                float h = PhysicsCaster.hits[i].collider.bounds.max.y;
                if (h > top)
                {
                    res = PhysicsCaster.hits[i].collider;
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
            float dir = StellaMove.ChrController.bounds.center.x - lastRockObject.transform.position.x;
            float offset = StellaMove.ChrController.bounds.extents.x + StellaMove.CollisionMargin + rockCollider.bounds.extents.x;
            float t = StellaMove.GetFallTime(top + StellaMove.MiniJumpMargin * 2f);
            float jumpt = StellaMove.GetFallTime(StellaMove.MiniJumpMargin);
            StellaMove.myVelocity.Set(offset * Mathf.Sign(dir) / t, jumpt * StellaMove.GravityAdd, 0f);
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
        }

        [System.Diagnostics.Conditional("DEBUG_STELLA_POSITION")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}