using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Walk", fileName = "StellaActionWalk")]
    public class StellaWalk : StellaActionScriptableObject
    {
        const int RaycastHitMax = 8;
        RaycastHit[] raycastHits = new RaycastHit[RaycastHitMax];

        public override void Init()
        {
            StellaMove.instance.SetAnimState(StellaMove.AnimType.Walk);
        }

        public override void UpdateAction()
        {
            // 水まきチェック
            if (Input.GetButton("Water"))
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Water);

            }
            else
            {
                StellaMove.instance.Walk();
            }

            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            if (!StellaMove.chrController.isGrounded)
            {
                StellaMove.instance.SetAnimState(StellaMove.AnimType.Jump);
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
                FallNextBlock();
            }
            else
            {
                StellaMove.instance.CheckMiniJump();
            }
        }

        /// <summary>
        /// 歩きから落下する時に、隣のブロックの中心辺りに着地できるようなX速度を設定
        /// </summary>
        public void FallNextBlock()
        {
            // 着地目標のX座標を求める
            Vector3 origin = Vector3.zero;
            origin.x = Mathf.Round(StellaMove.instance.transform.position.x);

            // 着地目標のX座標と自分の足元のYから下方向にレイを飛ばして、着地点を見つける
            origin.y = StellaMove.chrController.bounds.min.y;

            int cnt = Physics.RaycastNonAlloc(origin, Vector3.down, raycastHits, float.PositiveInfinity, StellaMove.MapCollisionLayerMask);
            if (cnt == 0)
            {
#if UNITY_EDITOR
                // 本来ないはずだが念のため
                Debug.Log($"地面無し");
#endif
                StellaMove.myVelocity.x = 0;
                return;
            }

            // 一番上を探す
            float top = raycastHits[0].collider.bounds.max.y;
            for (int i = 1; i < cnt; i++)
            {
                if (raycastHits[i].collider.bounds.max.y > top)
                {
                    top = raycastHits[i].collider.bounds.max.y;
                }
            }
            float h = StellaMove.chrController.bounds.min.y - top;
            float t = Mathf.Sqrt(2f * h / StellaMove.GravityAdd);
            StellaMove.myVelocity.x = (origin.x - StellaMove.instance.transform.position.x) / t;
        }


    }
}
