﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Fluff", fileName = "StellaActionFluff")]
    public class StellaFluff : StellaActionScriptableObject
    {
        FluffActable fluffActable = null;
        bool holded = false;

        public override void Init()
        {
            base.Init();
            fluffActable = ((FluffActable)ActionBox.SelectedActable);
            holded = false;
            StellaMove.RegisterAnimEvent(HoldStart);
            StellaMove.SetAnimState(StellaMove.AnimType.Dandelion);
        }

        public override void UpdateAction()
        {
            // 離すか、綿毛が無効化していたら落下へ
            if (!Input.GetButton("Action") || (fluffActable == null))
            {
                StellaMove.myVelocity = Vector3.zero;
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                return;
            }

            // 降りて着地したら、歩きへ

            // 手の位置と綿毛の位置に合わせて移動
            Vector3 next = fluffActable.HoldPosition;
            if (!holded)
            {
                Vector3 ofs = fluffActable.StellaStandardHoldOffset;
                if (StellaMove.forwardVector.x < -0.5f)
                {
                    ofs.x = -ofs.x;
                }
                next -= ofs;
            }
            else
            {
                next -= (StellaMove.HoldPosition-StellaMove.instance.transform.position);
            }

            Vector3 move = next - StellaMove.instance.transform.position;

            // 画面端から出た時の補正
            float adjustedX = StellaMove.instance.StageOverCheck(move.x);
            bool adjusted = move.x != adjustedX;
            move.x = adjustedX;

            // 移動
            CollisionFlags flags = StellaMove.ChrController.Move(move);

            // 衝突があったら離す
            if ((flags != CollisionFlags.None) || adjusted)
            {
                holded = false;
                if ((flags & CollisionFlags.Below) != 0)
                {
                    StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
                }
                else
                {
                    StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                }
            }
        }

        void HoldStart()
        {
            holded = true;
        }

        public override void End()
        {
            base.End();
            fluffActable.Release();
        }
    }
}