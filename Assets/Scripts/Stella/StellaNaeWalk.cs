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
            Vector3 naepos = GetPutPosition(StellaMove.instance.transform.position);
            naepos.y = StellaMove.chrController.bounds.min.y + naeActable.HeightFromGround;
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

        /// <summary>
        /// 苗を置く候補座標を返します。
        /// </summary>
        /// <param name="stellaPosition">ステラの座標</param>
        /// <returns>求めた苗の座標</returns>
        Vector3 GetPutPosition(Vector3 stellaPosition)
        {
            Vector3 naepos = stellaPosition;
            float absOffset = StellaMove.ActionBoxInstance.colliderCenter.x
                + StellaMove.ActionBoxInstance.halfExtents.x
                + naeActable.ColliderExtentsX;
            float baseX = naepos.x + absOffset * StellaMove.forwardVector.x;

            // 単位変換
            naepos.x = Mathf.Round(baseX / naeUnit) * naeUnit;
            if (absOffset < (Mathf.Abs(naepos.x - stellaPosition.x)))
            {
                // 遠くなっているので、1単位近づける
                naepos.x -= naeUnit * StellaMove.forwardVector.x;
                return naepos;
            }

            return naepos;
        }
    }
}