using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// ステラおぼれ
    /// </summary>
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Obore", fileName = "StellaActionObore")]
    public class StellaObore : StellaActionScriptableObject
    {
        [Tooltip("おぼれてからステージ選択までの秒数"), SerializeField]
        float toRestartSeconds = 2f;
        [Tooltip("落下速度"), SerializeField]
        float oboreFall = -0.25f;

        public override void Init()
        {
            base.Init();
            StellaMove.SetAnimState(StellaMove.AnimType.Obore);
            StellaMove.myVelocity.Set(0, oboreFall, 0);
            StellaMove.instance.Invoke("Restart", toRestartSeconds);
            StellaMove.instance.Splash();
        }

        public override void UpdateAction()
        {
            StellaMove.instance.Move();
        }
    }
}
