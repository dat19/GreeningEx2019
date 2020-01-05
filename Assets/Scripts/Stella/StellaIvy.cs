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

            StellaMove.SetAnimFloat("VelY", (lastY - StellaMove.instance.transform.position.y)/Time.fixedDeltaTime);
        }
    }
}