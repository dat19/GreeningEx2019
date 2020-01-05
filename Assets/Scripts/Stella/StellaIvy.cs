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
            // 上下移動設定
            float v = Input.GetAxisRaw("Vertical");
            StellaMove.myVelocity.y = v * upDownSpeed;

            // 左右補正設定
            float ivyx = StellaMove.IvyInstance.transform.position.x;
            float stx = ivyx - (StellaMove.HoldPosition.x- StellaMove.instance.transform.position.x);
            float movex = (stx - StellaMove.instance.transform.position.x);
            float step = sideSpeed * Time.fixedDeltaTime;
            float sign = Mathf.Sign(movex);
            movex = Mathf.Min(Mathf.Abs(movex), step);
            StellaMove.myVelocity.x = movex * sign / Time.fixedDeltaTime;

            // 移動
            float lastY = StellaMove.instance.transform.position.y;
            CollisionFlags flags = StellaMove.chrController.Move(StellaMove.myVelocity * Time.fixedDeltaTime);
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