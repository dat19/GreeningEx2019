using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
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
        }

        public override void UpdateAction(float tick)
        {
            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            if (!isLanding && StellaMove.chrController.isGrounded)
            {
                StellaMove.myVelocity.x = 0;
                StellaMove.RegisterAnimEvent(Grounded);
                StellaMove.instance.SetAnimState(StellaMove.AnimType.OnGround);
                isLanding = true;
            }
        }

        void Grounded()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
            isLanding = false;
        }
    }
}
