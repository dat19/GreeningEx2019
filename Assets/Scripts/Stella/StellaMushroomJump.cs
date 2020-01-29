using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create MushroomJump", fileName = "StellaActionMushroomJump")]
    public class StellaMushroomJump : StellaActionScriptableObject
    {
        [Tooltip("ジャンプの高さ"), SerializeField]
        float jumpHeight = 5.5f;
        [Tooltip("入力なしの時に、最高点の時点のX距離"), SerializeField]
        float standardJump = 1f;
        [Tooltip("入力ありの時に、最高点の時点のX距離"), SerializeField]
        float horizontalJump = 3f;

        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            base.Init();

            StellaMove.myVelocity = Vector3.zero;
            StellaMove.RegisterAnimEvent(Jump);
            StellaMove.SetAnimState(StellaMove.AnimType.Jump);
            StellaMove.SetAnimFloat("JumpRev", 1f);
        }

        public override void UpdateAction()
        {
            StellaMove.instance.Gravity();
            StellaMove.instance.Move();
        }

        /// <summary>
        /// ジャンプ実行
        /// </summary>
        void Jump()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float t = StellaMove.GetFallTime(jumpHeight);
            StellaMove.myVelocity.y = t * StellaMove.GravityAdd;

            if (Mathf.Approximately(h, 0f))
            {
                // 入力がない場合、前にstandardJump
                StellaMove.myVelocity.x = standardJump * StellaMove.forwardVector.x / t;
            }
            else
            {
                // 入力がある場合は、horizontalJump
                StellaMove.myVelocity.x = horizontalJump * h / t;
            }

            SoundController.Play(SoundController.SeType.MushroomJump);
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);
        }
    }
}
