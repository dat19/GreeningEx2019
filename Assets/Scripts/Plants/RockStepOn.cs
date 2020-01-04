using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class RockStepOn : MonoBehaviour, IStepOn
    {
        Grow grow;

        void Awake()
        {
            grow = GetComponent<Grow>();
        }

        public void StepOn()
        {
            if (grow.state != Grow.StateType.Growed) return;

            // 着地したオブジェクトとしてステラに報告
            StellaMove.stepOnObject = gameObject;

            // 着地済みの時は切り替え不要
            if (StellaMove.NowAction == StellaMove.ActionType.Tamanori) return;

            StellaMove.instance.ChangeAction(StellaMove.ActionType.Tamanori);
        }
    }
}