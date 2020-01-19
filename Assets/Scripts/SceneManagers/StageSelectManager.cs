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
        [Tooltip("ストーリー動画が始まるまで画面を隠しておくためのイメージ"), SerializeField]
        Image movieFadeImage = null;
        [Tooltip("星のインスタンス"), SerializeField]
        BaseStar baseStar = null;

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

        /// <summary>
        /// ステージ選択シーンに来る時の状況を表す。
        /// </summary>
        public enum ToStageSelectType
        {
            NewGame,    // 新規にゲームを開始
            Clear,      // ステージクリア
            Back,       // ユーザー操作で戻った時、或いはコンティニューで開始
        }

        enum StateType
        {
            OpeningMovie,
            PlayerControl,
            Clear,
            Back,
            StoryMovie,
            ToTitle,
        }

        /// <summary>
        /// ビデオの種類
        /// </summary>
        enum VideoType
        {
            Opening,
            Stage5,
            Ending,
        }

        /// <summary>
        /// 現在の状態
        /// </summary>
        static StateType state = StateType.OpeningMovie;
        /// <summary>
        /// 動画後に切り替える状態
        /// </summary>
        static StateType nextState;

        // リピートを防ぐための前のカーソル
        static float lastCursor = 0;

        static VideoPlayer videoPlayer = null;

        /// <summary>
        /// 処理開始
        /// </summary>
        static bool isStarted = false;

        public override void OnFadeOutDone()
        {
            isStarted = true;

            StarClean.StartClearedStage(GameParams.ClearedStageCount);
            baseStar.MakeSeaTexture();

            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }

            switch (GameParams.Instance.toStageSelect)
            {
                case ToStageSelectType.NewGame:
                    PlayVideo(VideoType.Opening);
                    nextState = StateType.PlayerControl;
                    break;
                case ToStageSelectType.Clear:
                    // 途中の動画チェック
                    if (GameParams.NowClearStage == 4)
                    {
                        PlayVideo(VideoType.Stage5);
                        nextState = StateType.PlayerControl;
                    }
                    else if (GameParams.NowClearStage == 9)
                    {
                        PlayVideo(VideoType.Ending);
                        nextState = StateType.ToTitle;
                    }
                    else
                    {
                        state = StateType.Clear;
                    }
                    break;
                case ToStageSelectType.Back:
                    state = StateType.Back;
                    break;
            }

            UpdateStageName();
            base.OnFadeOutDone();
            SceneManager.SetActiveScene(gameObject.scene);
        }


        private void Update()
        {
            if (!isStarted) { return; }

            switch (state)
            {
                case StateType.PlayerControl:
                    UpdatePlayerControl();
                    break;

                case StateType.StoryMovie:
                    break;

                default:
                    // デバッグコード。未実装の状態は、すぐに操作に移行
                    if (Fade.IsFading) return;
                    state = StateType.PlayerControl;
                    SoundController.PlayBGM(SoundController.BgmType.StageSelect);
                    break;
            }
        }

        /// <summary>
        /// ビデオの再生を開始します。
        /// </summary>
        /// <param name="vtype">再生するビデオの種類</param>
        void PlayVideo(VideoType vtype)
        {
            videoPlayer.enabled = true;
            videoPlayer.clip = videoClips[(int)vtype];
            videoPlayer.Play();
            state = StateType.StoryMovie;
            movieImage.enabled = true;
            movieFadeImage.enabled = true;
            StartCoroutine(StoryMovie());
        }

        IEnumerator StoryMovie()
        {
            // 動画開始を待つ
            while (videoPlayer.time <= 0.0001f)
            {
                yield return null;
            }

            movieFadeImage.enabled = false;

            // 動画終了を待つ
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            // タイトルへ戻る？
            if (nextState== StateType.ToTitle)
            {
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
            SoundController.PlayBGM(SoundController.BgmType.StageSelect);
            state = nextState;
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

        /// <summary>
        /// ステージ名を更新したものに変更します
        /// </summary>
        void UpdateStageName()
        {
            stageText.text = $"Stage {GameParams.SelectedStage+1}{StageNameColor}{stageNames[GameParams.SelectedStage]}</color>";
        }
    }
}
