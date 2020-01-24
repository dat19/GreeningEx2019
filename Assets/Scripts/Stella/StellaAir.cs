﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// ステラの空中制御。
    /// targetPositionのx座標に達するまでは、現在の移動を続けます。
    /// </summary>
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Air", fileName = "StellaActionAir")]
    public class StellaAir : StellaActionScriptableObject
    {
        /// <summary>
        /// 着地中
        /// </summary>
        bool isLanding = false;

        public override void Init()
        {
            base.Init();
            isLanding = false;
            StellaMove.SetAnimState(StellaMove.AnimType.Air);
        }

        public override void UpdateAction()
        {
            // つた掴みチェック
            if (StellaMove.CheckIvyHold())
            {
                return;
            }

            // 行動ボタンチェック。着地時は何もしない
            if (Input.GetButton("Action") && !isLanding)
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

            StellaMove.instance.Gravity();
            CollisionFlags flag = StellaMove.instance.Move();

            // 着地チェック
            if (!isLanding && StellaMove.chrController.isGrounded && StellaMove.myVelocity.y < 0f)
            {
                SoundController.Play(SoundController.SeType.Landing);

                StellaMove.myVelocity.x = 0;
                StellaMove.RegisterAnimEvent(Grounded);
                isLanding = true;
                StellaMove.CheckStepOn();
            }
            // 頭ぶつけチェック
            else if ((StellaMove.myVelocity.y > 0f) && flag.HasFlag(CollisionFlags.Above))
            {
                SoundController.Play(SoundController.SeType.HitHead);
                StellaMove.myVelocity.y = 0f;
            }
        }

        void Grounded()
        {
            isLanding = false;
            StellaMove.instance.ChangeToWalk();
        }
    }
}
