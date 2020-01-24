using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class CanClearEventEmitter : EventEmitter
    {
        /// <summary>
        /// クリア可能な時に発動
        /// </summary>
        protected override bool canEmit
        {
            get
            {
                return StageManager.CanClear;
            }
        }
    }
}