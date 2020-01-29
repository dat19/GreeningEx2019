using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Ivy", fileName = "StellaActionIvy")]
    public class StellaIvy : StellaActionScriptableObject
    {
        [Tooltip("ツタの昇り降り速度"), SerializeField]
        float upDownSpeed = 2.5f;
        [Tooltip("左右方向移動速度"), SerializeField]
        float sideSpeed = 2f;
        [Tooltip("ツタ飛び降り時のY速度"), SerializeField]
        float putDownSpeedY = 6f;

        public override void Init()
        {
            base.Init();

            StellaMove.myVelocity = Vector3.zero;
            StellaMove.SetAnimState(StellaMove.AnimType.Ivy);
        }


        /// <summary>
        /// ツタの昇り降り処理
        /// </summary>
        public override void UpdateAction()
        {
            // 左右が押されていたら飛び降りる
            if (SideMove()) return;

            // 左右補正
            AdjustSide();

            // 上下移動設定
            VerticalMove();
        }

        /// <summary>
        /// 横方向の移動を制御します。
        /// </summary>
        /// <returns>これ以降の処理が不要な場合、trueを返します。</returns>
        bool SideMove()
        {
            float h = Input.GetAxisRaw("Horizontal");
            // 操作なし
            if (Mathf.Approximately(h, 0f)) return false;

            StellaMove.myVelocity.x = h * StellaMove.MiniJumpSpeedMax;
            StellaMove.myVelocity.y = putDownSpeedY;
            StellaMove.instance.Gravity();
            StellaMove.instance.Move();
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
            return true;
        }

        /// <summary>
        /// ツタとの左右位置の補正
        /// </summary>
        void AdjustSide()
        {
            // 左右補正設定
            float ivyx = StellaMove.IvyInstance.transform.position.x;
            float stx = ivyx - (StellaMove.HoldPosition.x - StellaMove.instance.transform.position.x);
            float movex = (stx - StellaMove.instance.transform.position.x);
            float step = sideSpeed * Time.fixedDeltaTime;
            float sign = Mathf.Sign(movex);
            movex = Mathf.Min(Mathf.Abs(movex), step);
            StellaMove.myVelocity.x = movex * sign / Time.fixedDeltaTime;
        }

        /// <summary>
        /// 上下移動
        /// </summary>
        void VerticalMove()
        {
            float v = Input.GetAxisRaw("Vertical");
            StellaMove.myVelocity.y = v * upDownSpeed;

            // 登り切ったチェック
            if (v > 0.5f)
            {
                if (!StellaMove.IsIvyUp())
                {
                    // 前方に飛び降り
                    StellaMove.myVelocity.x = StellaMove.MiniJumpSpeedMax * StellaMove.forwardVector.x;
                    StellaMove.myVelocity.y = putDownSpeedY;
                    StellaMove.instance.Gravity();
                    StellaMove.instance.Move();
                    StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                    return;
                }
            }

            // 移動
            float lastY = StellaMove.instance.transform.position.y;
            CollisionFlags flags = StellaMove.ChrController.Move(StellaMove.myVelocity * Time.fixedDeltaTime);
            if (flags.HasFlag(CollisionFlags.Below) && StellaMove.myVelocity.y < 0f)
            {
                // 下にぶつかっていたらツタを離す
                StellaMove.myVelocity = Vector3.zero;
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
                return;
            }

            float vely = (lastY - StellaMove.instance.transform.position.y) / Time.fixedDeltaTime;
            if (Mathf.Abs(vely) < Mathf.Abs(StellaMove.myVelocity.x))
            {
                vely = StellaMove.myVelocity.x;
            }
            StellaMove.SetAnimFloat("VelY", vely);
        }
    }
}