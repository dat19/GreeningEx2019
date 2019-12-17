using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class StageSelectManager : SceneManagerBase
    {
        /// <summary>
        /// ステージ選択シーンに来る時の状況を表す。
        /// </summary>
        public enum ToStageSelectType
        {
            NewGame,    // 新規にゲームを開始
            Clear,
            Back,
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
