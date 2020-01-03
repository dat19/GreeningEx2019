using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019 {
    public class MushroomStepOn : MonoBehaviour, IStepOn
    {
        /// <summary>
        /// 踏んだ時に呼び出すメソッド
        /// </summary>
        public void StepOn()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.MushroomJump);
            GetComponent<Animator>().SetTrigger("Jump");
        }
    }
}