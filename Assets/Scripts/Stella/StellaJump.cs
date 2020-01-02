using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Jump", fileName = "StellaActionJump")]
    public class StellaJump : StellaActionScriptableObject
    {
        /// <summary>
        /// ジャンプを中断する時、true
        /// </summary>
        bool jumpAbort;

        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            base.Init();

            StellaMove.myVelocity.y = 0f;
            StellaMove.RegisterAnimEvent(StellaMove.instance.StartTargetJump);
            StellaMove.SetAnimState(StellaMove.AnimType.Jump);
            StellaMove.SetAnimFloat("JumpRev", 1f);
            jumpAbort = false;
        }

        public override void UpdateAction()
        {
            if (!jumpAbort)
            {
                // 中断チェック
                if (    ((StellaMove.forwardVector.x < -0.5f) && (Input.GetAxisRaw("Horizontal") >= 0f)
                    ||  ((StellaMove.forwardVector.x > 0.5f) && (Input.GetAxisRaw("Horizontal") <= 0f))))
                {
                    jumpAbort = true;
                    StellaMove.SetAnimFloat("JumpRev", -1f);
                }
            }
            else
            {
                // ジャンプ中断完了確認
                if (StellaMove.AnimTime <= 0f)
                {
                    StellaMove.SetAnimFloat("JumpRev", 1f);
                    jumpAbort = false;
                    StellaMove.SetAnimState(StellaMove.AnimType.Walk);
                    StellaMove.instance.ChangeAction(StellaMove.hasNae ? StellaMove.ActionType.NaeWalk : StellaMove.ActionType.Walk);
                }
            }
        }
    }
}
