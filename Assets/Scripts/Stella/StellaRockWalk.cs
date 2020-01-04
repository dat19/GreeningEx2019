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
                    float move = dist * distanceToSpeed;
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
                }

                // 移動しているなら、ジャンプチェック
                if (!Mathf.Approximately(StellaMove.myVelocity.x, 0))
                {
                    StellaMove.instance.CheckMiniJump();
                }
            }
        }
    }
}