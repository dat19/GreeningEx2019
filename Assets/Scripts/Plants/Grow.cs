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
        [Tooltip("苗が成長しきるまで、水まきを続ける植物にはチェックを入れる"), SerializeField]
        bool waitGrowDone = false;
        [Tooltip("生長する時の効果音"), SerializeField]
        SoundController.SeType growSe = SoundController.SeType.GrowFlowers;

#pragma warning disable 649
        [Tooltip("生長した時に実行したいメソッドがあったら登録します。"), SerializeField]
        UnityEngine.Events.UnityEvent growEvent;
#pragma warning restore 649

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
        /// 苗が成長した数
        /// </summary>
        public static int NaeGrowedCount { get; private set; }

        /// <summary>
        /// 生長完了を待つ残り数
        /// </summary>
        public static int WaitGrowCount { get; private set; }

        protected Animator anim;

        protected void Awake()
        {
            state = StateType.Nae;
            anim = GetComponent<Animator>();
            if (!anim)
            {
                anim = GetComponentInChildren<Animator>();
            }
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
                SoundController.Play(growSe);
                NaeGrowedCount++;
                Goal.IncrementNaeCount();
                if (waitGrowDone)
                {
                    WaitGrowCount++;
                }
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
            growEvent.Invoke();
            state = StateType.Growed;
            if (waitGrowDone)
            {
                WaitGrowCount--;
            }
        }

        /// <summary>
        /// 生長数の初期化など
        /// </summary>
        public static void Init()
        {
            WaitGrowCount = 0;
            NaeGrowedCount = 0;
        }
    }
}

