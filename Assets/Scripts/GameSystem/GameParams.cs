using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// クリア済みのステージ数や、プレイヤーが操作が可能な状況かどうかなど、
    /// ゲーム全体を制御するための変数や機能を提供するためのクラス。
    /// </summary>
    public class GameParams : Singleton<GameParams>
    {
        [Tooltip("ステージ選択シーンへ移行する際のモード")]
        public StageSelectManager.ToStageSelectType toStageSelect = StageSelectManager.ToStageSelectType.NewGame;

        /// <summary>
        /// 全ステージ数
        /// </summary>
        public const int StageCount = 10;

        /// <summary>
        /// クリア済みのステージ数。0なら全くクリアしていない。1ならStage1をクリア済み。
        /// </summary>
        public static int ClearedStageCount { get; private set; }

        /// <summary>
        /// ステージ選択で選ばれたステージ。0=Stage1
        /// </summary>
        public static int SelectedStage { get; private set; }

        private void Awake()
        {
            // TODO: PlayerPrefsから、ClearedStageCountを読み出す

            SelectedStage = ClearedStageCount;
        }

        /// <summary>
        /// 選択しているステージ番号を、次のステージに変更。ClearedStageをオーバーしていたら0に戻す
        /// </summary>
        public static void NextSelectStage()
        {
            int div = Mathf.Min(StageCount, ClearedStageCount + 1);
            SelectedStage = (SelectedStage + 1) % div;
        }

        /// <summary>
        /// 選択しているステージ番号を、前のステージに変更。0を越えていたら、ClearedStageへ。
        /// </summary>
        public static void PrevSelectStage()
        {
            SelectedStage = (SelectedStage == 0) ? ClearedStageCount : SelectedStage;
        }

        /// <summary>
        /// ステージクリア時に呼び出して、パラメーターを更新する。
        /// </summary>
        public static void StageClear()
        {
            Instance.toStageSelect = StageSelectManager.ToStageSelectType.Clear;
            if (SelectedStage == ClearedStageCount-1)
            {
                ClearedStageCount++;

                // TODO: PlayerPrefsで、ClearedStageCountを保存
            }
        }
    }
}
