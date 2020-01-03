using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// 踏むと発動するきのこや岩のためのインターフェース
    /// </summary>
    interface IStepOn
    {
        /// <summary>
        /// 踏んだ時に呼び出すメソッド
        /// </summary>
        void StepOn();
    }
}