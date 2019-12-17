using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class StageSelectManager : SceneManagerBase
    {
        [Tooltip("島のオブジェクト"), SerializeField]
        Transform[] islands = null;
        [Tooltip("ステージの星を表示する高さ"), SerializeField]
        float stageStarHeight = 4.5f;

        /// <summary>
        /// ステージ選択シーンに来る時の状況を表す。
        /// </summary>
        public enum ToStageSelectType
        {
            NewGame,    // 新規にゲームを開始
            Clear,
            Back,
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
        StateType state = StateType.OpeningMovie;

        private new void Awake()
        {
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

            base.Awake();
        }

#if DEBUG
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SoundController.Play(SoundController.SeType.Click);
                SceneChanger.ChangeScene(SceneChanger.SceneType.Game);
            }
        }
#endif
    }
}
