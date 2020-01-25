using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// プレイヤーがめり込んだ時に、指定の秒数当たり判定をオフにするためのスクリプトです。
    /// </summary>
    public class ColliderIgnore : MonoBehaviour
    {
        Collider myCollider = null;

        const float OffTime = 1f;

        /// <summary>
        /// 当たり判定の上座標
        /// </summary>
        public float Top { get { return myCollider.bounds.max.y; } }

        private void Awake()
        {
            Collider[] cols = GetComponentsInChildren<Collider>();
            for (int i=0;i<cols.Length;i++)
            {
                if (!cols[i].isTrigger)
                {
                    myCollider = cols[i];
                    return;
                }
            }
        }

        /// <summary>
        /// 決まった秒数、衝突を無効化します。
        /// </summary>
        public void Sleep()
        {
            if (myCollider != null)
            {
                myCollider.isTrigger = true;
                Invoke("Enabler", OffTime);
            }
        }

        void Enabler()
        {
            if (myCollider != null)
            {
                myCollider.isTrigger = false;
            }
        }
    }
}