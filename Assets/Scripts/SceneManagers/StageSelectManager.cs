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
        [Tooltip("動画ファイル"), SerializeField]
        VideoClip[] videoClips = new VideoClip[2];
        [Tooltip("カットインイメージ。0=Stage5, 1=エンディング前"), SerializeField]
        Sprite[] storySprites = new Sprite[2];
        //[Tooltip("動画フェード秒数"), SerializeField]
        //float videoFadeSeconds = 0.5f;
        [Tooltip("動画を描画するRawImage"), SerializeField]
        RawImage movieImage = null;
        [Tooltip("画像表示、あるいは、動画を隠すために使うイメージ"), SerializeField]
        Image waitMovieFade = null;
        [Tooltip("星のインスタンス"), SerializeField]
        BaseStar baseStar = null;
        [Tooltip("キャンバスアニメ"), SerializeField]
        Animator canvasAnim = null;
        [Tooltip("クレジットアニメの高速"), SerializeField]
        float creditSpeedUp = 4f;

        /// <summary>
        /// ナレーション前の余白秒数
        /// </summary>
        const float NarrationBeforeMarginSeconds = 1.25f;
        /// <summary>
        /// ナレーション後の余白秒数
        /// </summary>
        const float NarrationAfterMarginSeconds = 1.25f;

        /// <summary>
        /// ステージ名
        /// </summary>
        readonly string[] stageNames =
        {
            "First Island",
            "You Can Fly",
            "Mushroom Jump",
            "Rock Flower",
            "Flower Garden",
            "Dandelion River",
            "Guru Guru",
            "Rolling Rocks",
            "Underground Maze",
            "Last Star",
        };

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

        VideoPlayer videoPlayer = null;

        /// <summary>
        /// 処理開始
        /// </summary>
        static bool isStarted = false;

        bool isAnimDone = false;

        AudioSource audioSource = null;

        public override void OnFadeOutDone()
        {
            isStarted = true;
            GameParams.isMiss = false;
            audioSource = GetComponent<AudioSource>();
            videoPlayer = GetComponent<VideoPlayer>();

            //StarClean.StartClearedStage(GameParams.ClearedStageCount);
            baseStar.MakeSeaTexture();

#if DEBUG_ENDING
            GameParams.Instance.toStageSelect = ToStageSelectType.Clear;
#endif

            if (GameParams.Instance.toStageSelect != ToStageSelectType.NewGame)
            {
                StageSelectCamera.SetAnim(StageSelectCamera.AnimType.BackStandby);
            }

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
                ||  (GameParams.Instance.toStageSelect == ToStageSelectType.NewGame))
            {
                nextState = StateType.PlayerControl;
            }
            else if (GameParams.Instance.toStageSelect == ToStageSelectType.NextStage)
            {
                MovieOrNextStage();
            }

            // カメラアニメ
            if (GameParams.Instance.toStageSelect == ToStageSelectType.NewGame)
            {
                StageSelectCamera.SetAnim(StageSelectCamera.AnimType.NewGame);
            }
            else
            {
                StageSelectCamera.SetAnim(StageSelectCamera.AnimType.Back);
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
                        StartCoroutine(ClearSequence());
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
        IEnumerator ClearSequence()
        {
            // 切り替え
            yield return baseStar.UpdateClean();

            // 差し込み動画チェック
            MovieOrNextStage();
        }

        void MovieOrNextStage()
        {
            if (GameParams.NowClearStage == 4)
            {
                //PlayVideo(VideoType.Stage5);
                CutScene(VideoType.Stage5);
            }
            else if (GameParams.NowClearStage == 9)
            {
                //PlayVideo(VideoType.Ending);
                CutScene(VideoType.Ending);
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
        /// カットシーンを開始します。
        /// </summary>
        /// <param name="type">表示するスプライトの種類</param>
        void CutScene(VideoType type)
        {
            nextState = StateType.StoryMovie;
            waitMovieFade.sprite = storySprites[(int)type];
            StartCoroutine(StoryCutin(type));
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

        IEnumerator StoryCutin(VideoType type)
        {
            // 画像表示
            audioSource.clip = SoundController.Instance.seList[(int)SoundController.SeType.NarrationStage5 + (int)type];

            yield return AnimProc(CanvasAnimStateType.FadeOut);
            yield return new WaitForSeconds(NarrationBeforeMarginSeconds);
            audioSource.Play();

            // ナレーションの終了、あるいは、操作待ち
            while (audioSource.isPlaying && !GameParams.IsActionAndWaterButtonDown)
            {
                yield return null;
            }
            audioSource.Stop();

            // 終了待ち
            float startTime = Time.time;
            while (((Time.time-startTime) < NarrationAfterMarginSeconds) && !GameParams.IsActionAndWaterButton)
            {
                yield return null;
            }

            // Stage5なら、フェードアウトしてすぐにプレイヤー操作へ
            if (type == VideoType.Stage5)
            {
                yield return AnimProc(CanvasAnimStateType.FadeIn);
                waitMovieFade.sprite = null;
                SelectNextStage();
                nextState = StateType.PlayerControl;
                yield break;
            }

            // エンディングムービー
            videoPlayer.enabled = true;
            videoPlayer.clip = videoClips[(int)type];
            videoPlayer.Play();
            movieImage.enabled = true;

            // 動画開始を待つ
            while (videoPlayer.time <= 0.0001f)
            {
                yield return null;
            }

            // フェードイン
            yield return AnimProc(CanvasAnimStateType.FadeIn);
                waitMovieFade.sprite = null;

            // 動画終了を待つ
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            // クレジットロールへ
            yield return CreditRoll();
            SceneChanger.ChangeScene(SceneChanger.SceneType.Title);
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
                state = StateType.None;
                SoundController.Play(SoundController.SeType.Decision);
                StageSelectCamera.SetAnim(StageSelectCamera.AnimType.Start);
                SceneChanger.ChangeScene(SceneChanger.SceneType.Game);
            }
            else if (Input.GetButtonDown("Esc"))
            {
                state = StateType.None;
                SoundController.Play(SoundController.SeType.Decision);
                StageSelectCamera.SetAnim(StageSelectCamera.AnimType.ToTitle);
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
