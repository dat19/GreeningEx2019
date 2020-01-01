using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Jump", fileName = "StellaActionJump")]
    public class StellaJump : StellaActionScriptableObject
    {
        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            StellaMove.myVelocity.y = 0f;
            StellaMove.RegisterAnimEvent(StellaMove.instance.StartTargetJump);
            StellaMove.SetAnimState(StellaMove.AnimType.Jump);
        }
    }
}
