using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// ツタの処理。アクションキーでの発動ではないので、Actableは使わない。
    /// </summary>
    public class Ivy : MonoBehaviour
    {
        Grow grow;

        private void Awake()
        {
            grow = GetComponent<Grow>();
        }

        /// <summary>
        /// 捕まれる状態なら、捕まりへ移行
        /// </summary>
        /// <returns></returns>
        public bool Hold()
        {
            // 苗の時は発動なし
            if (grow.state == Grow.StateType.Nae) return false;

            StellaMove.instance.ChangeAction(StellaMove.ActionType.Ivy);
            return true;
        }
    }
}