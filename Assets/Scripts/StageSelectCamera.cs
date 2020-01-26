using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class StageSelectCamera : MonoBehaviour
    {
        static Animator anim = null;

        public enum AnimType
        {
            NewGameStandby, // 0 星から遠い
            NewGame,        // 1 星に近づく
            BackStandby,    // 2 星から戻る状態
            Back,           // 3 ステージから戻る
            Start,          // 4 ステージへ
            ToTitle,        // 5 タイトルヘ
        }

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        /// <summary>
        /// 指定のアニメを開始します。
        /// </summary>
        /// <param name="type">AnimTypeで設定します。</param>
        public static void SetAnim(AnimType type)
        {
            anim.SetInteger("State", (int)type);
        }
    }
}