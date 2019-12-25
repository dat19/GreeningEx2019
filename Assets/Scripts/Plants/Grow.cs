using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// 植物(苗)の親クラス。
    /// 水がかかったら、アニメのGrowをトリガーして、
    /// アニメが完了した時のパラメーターを受け取る。
    /// </summary>
    public class Grow : MonoBehaviour
    {
        /// <summary>
        /// 汎用の状態
        /// </summary>
        public enum StateType
        {
            Nae,
            Growing,
            Growed,
        }

        /// <summary>
        /// 現在の状態を表します。
        /// </summary>
        public StateType state = StateType.Nae;

        /// <summary>
        /// 苗の数
        /// </summary>
        public static int NaeCount { get; private set; }

        /// <summary>
        /// 苗が成長した数
        /// </summary>
        public static int NaeGrowedCount { get; private set; }

        protected Animator anim;

        protected void Awake()
        {
            state = StateType.Nae;
            anim = GetComponent<Animator>();
            if (!anim)
            {
                anim = GetComponentInChildren<Animator>();
            }
            NaeCount++;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            CanGrow(other);
        }

        /// <summary>
        /// 成長チェックをして、基本的な成長処理を実行します。
        /// </summary>
        /// <param name="other">当たり判定データ</param>
        /// <returns>true=発芽 / false=発芽なし</returns>
        protected bool CanGrow(Collider other)
        {
            if ((state == StateType.Nae) && other.CompareTag("Water"))
            {
                state = StateType.Growing;
                anim.SetTrigger("Grow");
                NaeGrowedCount++;
                Goal.IncrementNaeCount();
                return true;
            }

            return false;
        }

        /// <summary>
        /// アニメーションの最後のフレームのイベントから
        /// 呼び出します。
        /// </summary>
        public virtual void GrowDone()
        {
            state = StateType.Growed;
        }

        /// <summary>
        /// シーン切り替え時に呼び出す。
        /// </summary>
        public static void ClearNaeCount()
        {
            NaeCount = 0;
        }
    }
}

