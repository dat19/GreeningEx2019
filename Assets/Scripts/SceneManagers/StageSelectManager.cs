using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace GreeningEx2019
{
    public class StageSelectManager : SceneManagerBase
    {
        #pragma warning disable 0414
        [Tooltip("島のオブジェクト"), SerializeField]
        Transform[] islands = null;
        [Tooltip("ステージの星を表示する高さ"), SerializeField]
        float stageStarHeight = 4.5f;
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
            NewGame,
            PlayerControl,
            Clear,
            Back,
            StoryMovie,
        }

        /// <summary>
        /// 現在の状態
        /// </summary>
        static StateType state = StateType.OpeningMovie;

        // リピートを防ぐための前のカーソル
        static float lastCursor = 0;

        private void Update()
        {
            switch (state)
            {
                case StateType.PlayerControl:
                    UpdatePlayerControl();
                    break;

                default:
                    // デバッグコード。未実装の状態は、すぐに操作に移行
                    if (Fade.IsFading) return;
                    state = StateType.PlayerControl;
                    break;
            }
        }

        public override void OnFadeOutDone()
        {
            StarClean.StartClearedStage(GameParams.ClearedStageCount);

            switch (GameParams.Instance.toStageSelect)
            {
                case ToStageSelectType.NewGame:
                    state = StateType.OpeningMovie;
                    break;
                case ToStageSelectType.Clear:
                    // 途中の動画チェック
                    state = StateType.Clear;
                    break;
                case ToStageSelectType.Back:
                    state = StateType.Back;
                    break;
            }

            UpdateStageName();
            SoundController.PlayBGM(SoundController.BgmType.StageSelect);
            base.OnFadeOutDone();
            SceneManager.SetActiveScene(gameObject.scene);
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
