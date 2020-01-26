﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Nae Walk", fileName = "StellaActionNaeWalk")]
    public class StellaNaeWalk : StellaWalk
    {
        [Tooltip("苗を置ける場所の単位距離"), SerializeField]
        float naeUnit = 1f;
        [Tooltip("苗を置ける下方向の高さ"), SerializeField]
        float naePutUnderHeight = 0.5f;
        [Tooltip("苗を置ける上方向の高さ"), SerializeField]
        float naePutTopHeight = 0.5f;
        [Tooltip("移動チェックの時の足元からの高さ"), SerializeField]
        float naeWalkCollideHeight = 1f;

        public override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// 指定の座標に苗が置けるかを確認して、フラグで返します。
        /// </summary>
        /// <param name="pos">苗を置きたい地面の座標</param>
        /// <returns>true=置ける / false=置けない</returns>
        bool CheckPut(Vector3 pos)
        {
            // 置く先が、ステラの足元より一定以上低い場合は置けない
            if (pos.y < (StellaMove.chrController.bounds.min.y-naePutUnderHeight)) return false;

            // 置く先が、ステラの足元より一定以上高い場所は置けない
            if (pos.y > (StellaMove.chrController.bounds.min.y + naePutTopHeight)) return false;

            // 重なっているオブジェクトを探査
            int hitCount = StellaMove.naeActable.FetchOverlapObjects(pos, PhysicsCaster.MapLayer);
            return (hitCount == 0);
        }

        public override void UpdateAction()
        {
            // ターン中処理
            if (state == StateType.Turn)
            {
                if (StellaMove.instance.Turn())
                {
                    state = StateType.Walk;
                }
                return;
            }

            // 置く候補の地面の座標
            Vector3 naepos = GetPutPosition(StellaMove.instance.transform.position);
            bool canPut = CheckPut(naepos);

            if (canPut)
            {
                // 置けるならボタンによって苗を置く
                if (Input.GetButton("Water") || Input.GetButton("Action"))
                {
                    StellaMove.naePutPosition = naepos;
                    StellaMove.instance.ChangeAction(StellaMove.ActionType.PutDown);
                    return;
                }

                // 今置いたらここという場所に苗マーカーを表示
                NaeActable.MarkerObject.SetActive(true);
                naepos.y += StellaMove.naeActable.HeightFromGround;
                NaeActable.MarkerObject.transform.position = naepos;
            }
            else
            {
                // 置けない時はマーカーを非表示
                NaeActable.MarkerObject.SetActive(false);
            }

            // 行動
            if (!Walk())
            {
                // ターン時はこれ以降の処理不要
                return;
            }

            bool isBack = PushCheck();
            StellaMove.instance.Gravity();

            // 移動先の候補を調べる
            Vector3 nextNaePos = GetPutPosition(StellaMove.instance.transform.position + StellaMove.myVelocity * Time.fixedDeltaTime);
            if (!Mathf.Approximately(Vector3.Distance(naepos, nextNaePos), 0f)) {
                // 苗の候補場所が変わるので、移動キャンセル調査
                // 苗の高さは、地面より1マス分上を確認
                nextNaePos.y = StellaMove.chrController.bounds.min.y + naeWalkCollideHeight;
                int hitCount = StellaMove.naeActable.FetchOverlapObjects(nextNaePos, PhysicsCaster.MapCollisionLayer);
                for (int i=0; i<hitCount;i++)
                {
                    if (PhysicsCaster.hits[i].collider.CompareTag(GroundTag))
                    {
                        Debug.Log($"  hit nextNaeY={nextNaePos.y}");
                        // ぶつかるので移動をキャンセル
                        StellaMove.myVelocity.x = 0f;
                        break;
                    }
                }
            }

            StellaMove.instance.Move();

            if (!StellaMove.chrController.isGrounded)
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                FallNextBlock();
            }
            else
            {
                // 乗っかりチェック
                StellaMove.CheckStepOn();

                // 戻しじゃなく移動しているなら、ジャンプチェック
                if (!isBack && !Mathf.Approximately(StellaMove.myVelocity.x, 0))
                {
                    StellaMove.instance.CheckMiniJump();
                }
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
                + StellaMove.naeActable.ColliderExtentsX;
            float baseX = naepos.x + absOffset * StellaMove.forwardVector.x;

            // 単位変換
            naepos.x = Mathf.Round(baseX / naeUnit) * naeUnit;
            if (absOffset < (Mathf.Abs(naepos.x - stellaPosition.x)))
            {
                // 遠くなっているので、1単位近づける
                naepos.x -= naeUnit * StellaMove.forwardVector.x;
            }

            // 床の位置を調べる
            naepos.y = StellaMove.chrController.bounds.center.y;
            int grobj = PhysicsCaster.GetGround(naepos, float.PositiveInfinity);
            if (grobj == -1)
            {
                // 置けない高さを設定
                naepos.y = (StellaMove.chrController.bounds.min.y - naePutUnderHeight * 2f);
                return naepos;
            }

            naepos.y = PhysicsCaster.hits[grobj].collider.bounds.max.y;
            return naepos;
        }
    }
}