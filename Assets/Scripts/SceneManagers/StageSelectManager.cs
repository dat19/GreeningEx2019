//#define DEBUG_ENDING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;

namespace GreeningEx2019
{
    public class StageSelectManager : SceneManagerBase
    {
        [Tooltip("ステージテキスト"), SerializeField]
        TextMeshProUGUI stageText = null;
        [Tooltip("ステージ名"), SerializeField]
        string[] stageNames =
        {
            "First Island",
            "Second Island",
            "All Test Island",
            "No Name",
            "No Name",
            "No Name",
            "No Name",
            "No Name",
            "No Name",
            "No Name",
        };
        [Tooltip("動画ファイル"), SerializeField]
        VideoClip[] videoClips = new VideoClip[3];
        [Tooltip("動画フェード秒数"), SerializeField]
        float videoFadeSeconds = 0.5f;
        [Tooltip("動画を描画するRawImage"), SerializeField]
        RawImage movieImage = null;
        [Tooltip("星のインスタンス"), SerializeField]
        BaseStar baseStar = null;
        [Tooltip("キャンバスアニメ"), SerializeField]
        Animator canvasAnim = null;
        [Tooltip("クレジットアニメの高速"), SerializeField]
        float creditSpeedUp = 4f;

        // ステージ名の色指定
        const string StageNameColor = "\n<color=#afc>";

        /// <summary>
        /// 操作が可能になったら、trueを返します。
        /// </summary>
        public static bool CanMove
        {
            get
            {
                return state == StateType.PlayerControl;
            }
        }

        public static bool IsClearEffect
        {
            get
            {
                return (GameParams.Instance.toStageSelect == ToStageSelectType.Clear)
                    && (GameParams.NowClearStage == GameParams.ClearedStageCount-1);
            }
        }

        /// <summary>
        /// ステージ選択シーンに来る時の状況を表す。
        /// </summary>
        public enum ToStageSelectType
        {
            NewGame,    // 新規にゲームを開始
            Clear,      // ステージクリア
            NextStage,  // クリア済みのステージをクリア
            Back,       // ユーザー操作で戻った時、或いはコンティニューで開始
        }

        enum StateType
        {
            None = -1,
            PlayerControl,
            Clear,
            StoryMovie,
            CreditRoll,
        }

        /// <summary>
        /// ビデオの種類
        /// </summary>
        enum VideoType
        {
            Stage5,
            Ending,
        }

        /// <summary>
        /// キャンバスアニメーションのState定義
        /// </summary>
        enum CanvasAnimStateType
        {
            Standby,    // 0
            FadeOut,    // 1
            FadeIn,     // 2
            UIDisplay,  // 3
            WhiteOut,   // 4
            Credit,     // 5
        }

        /// <summary>
        /// 現在の状態
        /// </summary>
        static StateType state = StateType.None;
        /// <summary>
        /// 動画後に切り替える状態
        /// </summary>
        static StateType nextState = StateType.None;

        // リピートを防ぐための前のカーソル
        static float lastCursor = 0;

        static VideoPlayer videoPlayer = null;

        /// <summary>
        /// 処理開始
        /// </summary>
        static bool isStarted = false;

        bool isAnimDone = false;

        public override void OnFadeOutDone()
        {
            isStarted = true;
            GameParams.isMiss = false;

            //StarClean.StartClearedStage(GameParams.ClearedStageCount);
            baseStar.MakeSeaTexture();

            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }

#if DEBUG_ENDING
            GameParams.Instance.toStageSelect = ToStageSelectType.Clear;
#endif

            UpdateStageName();
            SceneManager.SetActiveScene(gameObject.scene);
            base.OnFadeOutDone();
        }

        /// <summary>
        /// フェードインが完了した時の処理
        /// </summary>
        public override void OnFadeInDone()
        {
            base.OnFadeInDone();

            if (GameParams.Instance.toStageSelect == ToStageSelectType.Clear)
            {
                nextState = StateType.Clear;
            }
            else if (   (GameParams.Instance.toStageSelect == ToStageSelectType.Back)
                ||  (GameParams.Instance.toStageSelect == ToStageSelectType.NewGame)
                ||  (GameParams.Instance.toStageSelect == ToStageSelectType.NextStage))
            {
                nextState = StateType.PlayerControl;
            }
        }

        private void Update()
        {
            if (!isStarted) { return; }

            // 初期化処理
            if (nextState != StateType.None)
            {
                state = nextState;
                nextState = StateType.None;

                switch (state)
                {
                    case StateType.Clear:
                        StartCoroutine(clearSequence());
                        break;

                    case StateType.PlayerControl:
                        canvasAnim.SetInteger("State", (int)CanvasAnimStateType.UIDisplay);
                        SoundController.PlayBGM(SoundController.BgmType.StageSelect);
                        break;
                }
            }

            // 更新処理
            if (state == StateType.PlayerControl)
            {
                UpdatePlayerControl();
            }
        }

        /// <summary>
        /// クリア演出を実行します
        /// </summary>
        IEnumerator clearSequence()
        {
            // 切り替え
            yield return baseStar.UpdateClean();

            // 差し込み動画チェック
            if (GameParams.NowClearStage == 4)
            {
                PlayVideo(VideoType.Stage5);
                nextState = StateType.PlayerControl;
            }
            else if (GameParams.NowClearStage == 9)
            {
                PlayVideo(VideoType.Ending);
                nextState = StateType.CreditRoll;
            }
            else
            {
                SelectNextStage();
                nextState = StateType.PlayerControl;
            }
        }

        /// <summary>
        /// 次のステージを自動的に更新
        /// </summary>
        void SelectNextStage()
        {
            if (GameParams.ClearedStageCount < GameParams.StageMax)
            {
                GameParams.NextSelectStage();
                UpdateStageName();
            }
        }

        /// <summary>
        /// ビデオの再生を開始します。
        /// </summary>
        /// <param name="vtype">再生するビデオの種類</param>
        void PlayVideo(VideoType vtype)
        {
            nextState = StateType.StoryMovie;
            StartCoroutine(StoryMovie(vtype));
        }

        public void AnimDone()
        {
            isAnimDone = true;
        }

        IEnumerator AnimProc(CanvasAnimStateType type)
        {
            canvasAnim.SetInteger("State", (int)type);
            isAnimDone = false;
            while (!isAnimDone)
            {
                yield return null;
            }
        }

        IEnumerator StoryMovie(VideoType vtype)
        {
            yield return AnimProc(CanvasAnimStateType.FadeOut);

            videoPlayer.enabled = true;
            videoPlayer.clip = videoClips[(int)vtype];
            videoPlayer.Play();
            movieImage.enabled = true;

            // 動画開始を待つ
            while (videoPlayer.time <= 0.0001f)
            {
                yield return null;
            }

            // フェードイン
            yield return AnimProc(CanvasAnimStateType.FadeIn);

            // 動画終了を待つ
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            // 最終ステージならクレジットロールへ
            if (GameParams.NowClearStage == GameParams.StageMax-1)
            {
                yield return CreditRoll();
                SceneChanger.ChangeScene(SceneChanger.SceneType.Title);
                yield break;
            }

            // ビデオプレイヤーをフェードアウトさせる
            float fadeTime = 0;
            Color col = movieImage.color;
            while (fadeTime < videoFadeSeconds)
            {
                fadeTime += Time.deltaTime;
                float alpha = (1f - fadeTime / videoFadeSeconds);
                col.a = alpha;
                movieImage.color = col;
                yield return null;
            }

            movieImage.enabled = false;
            SelectNextStage();
            nextState = StateType.PlayerControl;
        }


        void UpdatePlayerControl()
        {
            float cursor = Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("Vertical");
            if ((cursor < -0.5f) && (lastCursor > -0.5f))
            {
                SoundController.Play(SoundController.SeType.MoveCursor);
                GameParams.PrevSelectStage();
                UpdateStageName();
            }
            else if ((cursor > 0.5f) && (lastCursor < 0.5f))
            {
                SoundController.Play(SoundController.SeType.MoveCursor);
                GameParams.NextSelectStage();
                UpdateStageName();
            }
            lastCursor = cursor;

            if (GameParams.IsActionAndWaterButtonDown)
            {
                SoundController.Play(SoundController.SeType.Decision);
                SceneChanger.ChangeScene(SceneChanger.SceneType.Game);
            }
            else if (Input.GetButtonDown("Esc"))
            {
                SoundController.Play(SoundController.SeType.Decision);
                SceneChanger.ChangeScene(SceneChanger.SceneType.Title);
            }
        }

        IEnumerator CreditRoll()
        {
            SoundController.PlayBGM(SoundController.BgmType.Ending);
            canvasAnim.SetInteger("State", (int)CanvasAnimStateType.Credit);

            isAnimDone = false;
            while (!isAnimDone)
            {
                if (GameParams.IsActionAndWaterButton)
                {
                    canvasAnim.SetFloat("Speed", creditSpeedUp);
                }
                else
                {
                    canvasAnim.SetFloat("Speed", 1);
                }

                yield return null;
            }

            // キー待ち
            while (!GameParams.IsActionAndWaterButtonDown)
            {
                yield return null;
            }
        }

        /// <summary>
        /// ステージ名を更新したものに変更します
        /// </summary>
        void UpdateStageName()
        {
            stageText.text = $"Stage {GameParams.SelectedStage+1}{StageNameColor}{stageNames[GameParams.SelectedStage]}</color>";
        }
    }
}
