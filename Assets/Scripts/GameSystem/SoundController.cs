/*
 * MIT License
 * Copyright (c) 2018 Yu Tanaka
 * https://github.com/am1tanaka/OpenSimpleFramework201801/blob/master/LICENSE
 */

using System.Collections;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// BGMやSEを制御するクラスです。
    /// </summary>
    public class SoundController : MonoBehaviour
    {
        /// <summary>
        /// 自分のインスタンス
        /// </summary>
        public static SoundController Instance
        {
            get;
            private set;
        }

        [TooltipAttribute("SE用のオーディオソース")]
        public AudioSource audioSE;
        [TooltipAttribute("BGM用のオーディオソース")]
        public AudioSource audioBGM;

        /// <summary>
        /// 効果音リスト。この並びと<c>SEList</c>にセットするAudioClipの並びを合わせます。
        /// </summary>
        [SerializeField]
        public enum SeType
        {
            None = -1,      // なし
            Decision,       // 0 決定、ゲーム開始音
            MushroomJump,   // 1 マッシュルームでジャンプ
            Obore,          // 2 溺れ
            CanClear,       // 3 全て発芽させて、星が光る
            GrowIvy,        // 4 ツタが伸びる
            GrowMushroom,   // 5 キノコが発芽
            GrowDandelion,  // 6 たんぽぽが発芽
            Landing,        // 7 着地
            OpenMenu,       // 8 メニューを開く(未実装)
            //
            MoveCursor,     // 9  カーソル移動
            GrowFlowers,    // 10 花の橋が発芽
            RollingStone,   // 11 岩が転がる(RockBeforeに実装)
            WalkOnFlower,   // 12 花の上を歩く(未実装)
            SpawnFluff,     // 13 たんぽぽから綿毛出現(Fluffプレハブ側で鳴らしている)
            MiniJump,       // 14 1段差をジャンプ0
            GrowRock,       // 15 岩が発芽
            Water,          // 16 水をまく(StellaのAudioSourceで制御)
            LiftUp,         // 17 苗を持ち上げる
            PutDown,        // 18 苗を置く
            RockWater,      // 19 岩が水に落ちる
            // 台詞
            NarrationOpening,   // 20
            NarrationStage5,    // 21
            NarrationEnding,    // 22
            GimmickDandelion,   // 23
            GimmickMushroom,    // 24
            GimmickFlowerBridge,// 25
            GimmickRock,        // 26
            GimmickGanbare,     // 27
            GimmickDaijoubu,    // 28
            GimmickKiwotsukete, // 29
            // 追加効果音
            HitHead,            // 30頭ぶつけ
            Walk0,              // 31歩き1
            Walk1,              // 32歩き2
            Walk2,              // 33歩き3
            Tsuta0,             // 34ツタ登り
            StarFly,            // 35星が飛び去る時
            DandelionFlower,    // 36たんぽぽが咲く時
        };
        [TooltipAttribute("効果音リスト")]
        public AudioClip[] seList = null;

        /// <summary>
        /// BGMの列挙子。この並びと<c>BGMList</c>にセットするAudioClipの並びを合わせます。
        /// </summary>
        public enum BgmType
        {
            Title,
            StageSelect,
            Game0,
            Game1,
            Game2,
            Clear,
            Ending,
        }
        [Tooltip("BGMリスト"), SerializeField]
        private AudioClip[] bgmList = null;

        /// <summary>
        /// 効果音を連続で再生する時にずらしておく秒数
        /// </summary>
        const float RapidSeWait = 0.04f;

        /// <summary>
        /// 同時再生登録の最高登録数
        /// </summary>
        const int EntrySeMax = 8;

        static SeType[] entrySe = new SeType[EntrySeMax];
        static int entrySeCount = 0;
        static int entrySeIndex = 0;
        static float seWait;

        /// <summary>
        /// フェードに合わせたボリューム調整をする時、true
        /// </summary>
        static bool useFade = false;

        static float seVolume = 0.5f;
        /// <summary>
        /// SEのボリューム
        /// </summary>
        public static float SeVolume
        {
            get
            {
                return seVolume;
            }
            set
            {
                seVolume = Mathf.Clamp01(value);
            }
        }
        static float bgmVolume = 0.5f;
        /// <summary>
        /// BGMのボリューム
        /// </summary>
        public static float BgmVolume
        {
            get
            {
                return bgmVolume;
            }
            set
            {
                bgmVolume = Mathf.Clamp01(value);
            }
        }

        /// <summary>
        /// オーディオリスナ
        /// </summary>
        public static AudioListener audioListener;

        /// <summary>
        /// フェードアウトを表すフラグ。trueの時、フェードアウト中。
        /// </summary>
        static bool isFadingOut;

        /// <summary>
        /// Fadeのα値に合わせてフェードさせる時true。falseの時は、独自に指定の秒数でフェードアウト
        /// </summary>
        public static bool useFadeAlpha = true;

        /// <summary>
        /// Fadeのα値を利用しない時、独自に秒数の経過でフェードさせます。
        /// </summary>
        static float targetFadeSeconds;

        /// <summary>
        /// フェードを開始してからの経過秒数
        /// </summary>
        static float fadeSeconds;

        #region System
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            isFadingOut = false;
            useFade = false;
            audioListener = GetComponent<AudioListener>();
            entrySeCount = 0;
            seWait = 0;
            entrySeIndex = 0;
        }

        private void FixedUpdate()
        {
            fadeSeconds += Time.fixedDeltaTime;
            if (fadeSeconds > targetFadeSeconds)
            {
                fadeSeconds = targetFadeSeconds;
            }
        }

        private void LateUpdate()
        {
            if (entrySeCount > 0)
            {
                seWait -= Time.deltaTime;
                if (seWait <= 0)
                {
                    entrySeCount--;
                    PlaySe(entrySe[entrySeIndex]);
                    entrySeIndex = (entrySeIndex + 1) % EntrySeMax;
                }
            }

            SetVolume();
        }

        #endregion System

        /// <summary>
        /// 指定の効果音を鳴らします。
        /// </summary>
        /// <param name="snd">再生したい効果音</param>
        public static void Play(SeType snd)
        {
            // すでに上限の時はキャンセル
            if (entrySeCount >= EntrySeMax) return;

            if ((seWait <= 0) && (entrySeCount == 0))
            {
                PlaySe(snd);
                return;
            }

            int idx = (entrySeIndex + entrySeCount) % EntrySeMax;
            entrySe[idx] = snd;
            entrySeCount++;
        }

        static void PlaySe(SeType se)
        {
            seWait = RapidSeWait;
            if (se != SeType.None)
            {
                Instance.audioSE.PlayOneShot(Instance.seList[(int)se]);
            }
        }


        /// <summary>
        /// 効果音を停止します。
        /// </summary>
        public static void Stop()
        {
            Instance.audioSE.Stop();
        }

        /// <summary>
        /// 指定のBGMを再生します。
        /// </summary>
        /// <param name="bgm">再生したいBGM</param>
        /// <param name="isFadeIn">フェードインさせたい時trueを設定。デフォルトはfalse。</param>
        public static void PlayBGM(BgmType bgm, bool isFadeIn = false)
        {
            useFade = isFadeIn;
            isFadingOut = false;
            useFadeAlpha = true;

            // 同じ曲が設定されていて再生中ならなにもしない
            if (Instance.audioBGM.clip == Instance.bgmList[(int)bgm])
            {
                if (Instance.audioBGM.isPlaying)
                {
                    SetVolume();
                    return;
                }
            }
            else
            {
                // 違う曲の場合
                // 曲が設定されていたら、曲を停止
                if (Instance.audioBGM.clip != null)
                {
                    Instance.audioBGM.Stop();
                }

                // 曲を設定
                Instance.audioBGM.clip = Instance.bgmList[(int)bgm];
            }

            // 再生開始
            SetVolume();
            Instance.audioBGM.Play();
        }

        /// <summary>
        /// BGMを停止します。
        /// フェードアウトさせる場合は、先にFadeをフェードアウトさせておくこと。
        /// </summary>
        /// <param name="isFadeOut">trueを設定すると、フェードアウトしたのち停止。falseだとすぐ停止。</param>
        public static void StopBGM(bool isFadeOut = false, float seconds = 0f)
        {
            // フェードの指定がないなら、すぐに停止
            if (!isFadeOut)
            {
                isFadingOut = false;
                useFade = false;
                Instance.audioBGM.Stop();
                return;
            }

            // フェードアウトを開始していないなら、フェードアウト開始
            if (!isFadingOut)
            {
                useFade = true;
                isFadingOut = true;
                useFadeAlpha = true;
                if (seconds > 0f)
                {
                    useFadeAlpha = false;
                    targetFadeSeconds = seconds;
                    fadeSeconds = 0f;
                }

                Instance.StartCoroutine(FadeOutBGM());
            }
        }

        /// <summary>
        /// BGMのフェードアウトが完了するのを待って、BGMを停止する。
        /// 途中でフラグがfalseになったら、次の再生が始まっているかも知れないので
        /// BGMの停止をせずにすぐにコルーチンを終了する。
        /// </summary>
        static IEnumerator FadeOutBGM()
        {
            while ((Fade.FadeState == Fade.FadeStateType.Out)
                || (!useFadeAlpha && (fadeSeconds < targetFadeSeconds)))
            {
                // フェードアウトが途中でキャンセルされたらコルーチンを強制終了
                if (!isFadingOut)
                {
                    yield break;
                }
                yield return null;
            }
        }

        #region Private Methods

        /// <summary>
        /// 現在のフェードのα値からボリュームを設定します。
        /// フェード未使用の場合はボリュームを最大にします。
        /// </summary>
        static void SetVolume()
        {
            float newvol = bgmVolume;

            if (useFade)
            {
                if (useFadeAlpha)
                {
                    newvol = bgmVolume * (1f - Fade.NowColor.a);
                }
                else
                {
                    newvol = bgmVolume * (1f - (fadeSeconds / targetFadeSeconds));
                }
            }

            if (Instance.audioBGM.volume != newvol)
            {
                Instance.audioBGM.volume = newvol;
            }
            if (Instance.audioSE.volume != seVolume)
            {
                Instance.audioSE.volume = seVolume;
            }
        }

        #endregion Private Methods

    }
}

