using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019 {
    public class MushroomStepOn : MonoBehaviour, IStepOn
    {
        Grow grow;

        void Awake()
        {
            grow = GetComponent<Grow>();
        }

        /// <summary>
        /// 踏んだ時に呼び出すメソッド
        /// </summary>
        public void StepOn()
        {
            // 完成していない時は動作しない
            if (grow.state != Grow.StateType.Growed) return;
            
            StellaMove.instance.ChangeAction(StellaMove.ActionType.MushroomJump);
            GetComponent<Animator>().SetTrigger("Jump");
        }
    }
}