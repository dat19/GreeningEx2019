using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Nae Walk", fileName = "StellaActionNaeWalk")]
    public class StellaNaeWalk : StellaWalk
    {
        [Tooltip("苗を置ける場所の単位距離"), SerializeField]
        float naeUnit = 1f;

        NaeActable naeActable = null;

        public override void Init()
        {
            base.Init();

            naeActable = (NaeActable)ActionBox.SelectedActable;
        }

        public override void UpdateAction()
        {
            // ターン中処理
            if (state == StateType.Turn)
            {
                Turn();
                return;
            }

            // 置けるかチェック
            Vector3 naepos = StellaMove.instance.transform.position;
            float absOffset = StellaMove.ActionBoxInstance.colliderCenter.x
                + StellaMove.ActionBoxInstance.halfExtents.x
                + naeActable.ColliderExtentsX;
            float baseX = naepos.x + absOffset * StellaMove.forwardVector.x;

            // 単位変換
            float unitX = Mathf.Round(baseX / naeUnit) * naeUnit;
            if (absOffset < (Mathf.Abs(unitX-naepos.x)))
            {
                // 遠くなっているので、1単位近づける
                naepos.x = unitX - naeUnit * StellaMove.forwardVector.x;
            }
            else
            {
                naepos.x = unitX;
            }

            NaeActable.MarkerObject.transform.position = naepos;


            // 水まきチェック
            if (Input.GetButton("Water"))
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Water);
            }
            else
            {
                // 行動ボタンチェック
                if (Input.GetButton("Action"))
                {
                    Actable act = StellaMove.ActionBoxInstance.GetActableInstance();
                    if (act != null)
                    {
                        act.Action();
                        return;
                    }
                }

                Walk();
            }

            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            if (!StellaMove.chrController.isGrounded)
            {
                StellaMove.SetAnimState(StellaMove.AnimType.Jump);
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                FallNextBlock();
            }
            else
            {
                StellaMove.instance.CheckMiniJump();
            }
        }
    }
}