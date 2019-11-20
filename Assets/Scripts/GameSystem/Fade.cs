/**
 * 画面のフェードを管理する
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GreeningEx2019
{
    public class Fade : MonoBehaviour
    {
        /// <summary>
        /// 自分のインスタンス
        /// </summary>
        public static Fade Instance { get; private set; }

        /// <summary>
        /// フェードの色
        /// </summary>
        static Color defaultColor;

        /// <summary>
        /// フェードの状態
        /// </summary>
        public enum FadeStateType
        {
            None=-1,   // 何もしていない
            Out,    // フェードアウト中
            In      // フェードイン中
        }

        /// <summary>
        /// フェード用のイメージ
        /// </summary>
        private static Image fadeImage;

        /// <summary>
        /// フェード中フラグ
        /// </summary>
        public static bool IsFading
        {
            get
            {
                return FadeState != FadeStateType.None;
            }
        }

        /// <summary>
        /// 現在のフェード状態
        /// </summary>
        public static FadeStateType FadeState
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在のフェードのカラー
        /// </summary>
        public static Color NowColor
        {
            get;
            private set;
        }

        private void Awake()
        {
            Instance = this;
            fadeImage = GetComponent<Image>();
            defaultColor = fadeImage.color;
            defaultColor.a = 1f;
            NowColor = defaultColor;

            fadeImage.enabled = true;
            fadeImage.color = NowColor;
            FadeState = FadeStateType.None;
        }

        /// <summary>
        /// フェードインかフェードアウトを、指定の秒数で実行します。
        /// stopBGMをtrueにすると、フェードに合わせてBGMを停止します。
        /// </summary>
        /// <param name="type">フェードアウトかフェードインかをFADEで指定</param>
        /// <param name="time">秒数</param>
        /// <param name="stopBGM">BGMをフェードに合わせて停止したい時、true</param>
        public static IEnumerator StartFade(FadeStateType type, float time, bool stopBGM = false)
        {
            return StartFade(type, defaultColor, time, stopBGM);
        }

        /// <summary>
        /// フェードインかフェードアウトを、指定の秒数で実行します。
        /// stopBGMをtrueにすると、フェードに合わせてBGMを停止します。
        /// </summary>
        /// <param name="type">フェードアウトかフェードインかをFADEで指定</param>
        /// <param name="color">フェードの色</param>
        /// <param name="time">秒数</param>
        /// <param name="stopBGM">BGMをフェードに合わせて停止したい時、true</param>
        public static IEnumerator StartFade(FadeStateType type, Color color, float time, bool stopBGM=false)
        {
            if (fadeImage == null) yield break;

            FadeState = type;
            if (stopBGM)
            {
                SoundController.StopBGM(true);
            }
            float startTime = Time.time;
            Color nowColor;
            nowColor = color;
            nowColor.a = type == FadeStateType.In ? 1f : 0f;
            fadeImage.color = nowColor;

            fadeImage.enabled = true;
            while (time > 0f && ((Time.time - startTime) <= time))
            {
                // フェードイン中
                float keika = (Time.time - startTime) / time;
                if (type == FadeStateType.In)
                {
                    nowColor.a = 1f - keika;
                }
                else
                {
                    // フェードアウト中
                    nowColor.a = keika;
                }
                fadeImage.color = nowColor;

                NowColor = nowColor;

                yield return null;
            }

            nowColor.a = type == FadeStateType.In ? 0f : 1f;
            fadeImage.color = nowColor;
            NowColor = nowColor;
            if (type == FadeStateType.In)
            {
                fadeImage.enabled = false;
            }

            FadeState = FadeStateType.None;

            if (stopBGM)
            {
                SoundController.StopBGM();
            }
        }

        /// <summary>
        /// フェードイメージを指定の色に変更します。
        /// </summary>
        /// <param name="col">設定する色</param>
        public static void SetFadeColor(Color col)
        {
            fadeImage.enabled = true;
            fadeImage.color = col;
        }
    }
}
