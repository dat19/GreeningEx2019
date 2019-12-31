using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class NaeActable : Actable
    {
        [Tooltip("手のピボットからのオフセット座標"), SerializeField]
        Vector3 offset = new Vector3(0.08f, 0.08f);

        /// <summary>
        /// 持ち上げられている時、trueにします。
        /// </summary>
        bool isHolding = false;

        /// <summary>
        /// オブジェクトをくっつける基準のトランスフォーム
        /// </summary>
        Transform parentPivot;

        private void Awake()
        {
            CanAction = true;
            isHolding = false;
        }

        public override void Action()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.LifetUp);
        }

        /// <summary>
        /// ステラが掴んだ時に呼び出します。
        /// </summary>
        public void Hold(Transform pivot)
        {
            isHolding = true;
            parentPivot = pivot;
        }

        private void LateUpdate()
        {
            if (isHolding)
            {
                transform.position = parentPivot.position + offset;
                transform.forward = Vector3.forward;
                transform.up = Vector3.up;
            }
        }
    }
}