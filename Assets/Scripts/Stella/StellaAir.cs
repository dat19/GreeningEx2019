using System.Collections;
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
            isLanding = false;
            StellaMove.instance.SetAnimState(StellaMove.AnimType.Air);
        }

        public override void UpdateAction()
        {
            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            if (!isLanding && StellaMove.chrController.isGrounded)
            {
                StellaMove.myVelocity.x = 0;
                StellaMove.RegisterAnimEvent(Grounded);
                isLanding = true;
            }
        }

        void Grounded()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
            isLanding = false;
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (!StageManager.CanMove) return;

            if (other.CompareTag("DeadZone"))
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Obore);
            }
        }
    }
}
