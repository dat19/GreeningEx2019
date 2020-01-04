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

        RockActable rockActable;

        public override void Init()
        {
            base.Init();

            rockActable = (RockActable)ActionBox.SelectedActable;
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
            float back = h * StellaMove.forwardVector.x > 0.5f ? 1f : -1f;
            StellaMove.SetAnimFloat("Back", back);

            if (!StellaMove.chrController.isGrounded)
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                FallNextBlock();
            }
            else
            {
                // 岩に力を加える
                Debug.Log($"  down force");

                // 移動しているなら、ジャンプチェック
                if (!Mathf.Approximately(StellaMove.myVelocity.x, 0))
                {
                    StellaMove.instance.CheckMiniJump();
                }
            }
        }
    }
}