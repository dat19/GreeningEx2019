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
        [Tooltip("クリア済みのステージ数。0なら全くクリアしていない。1ならStage1をクリア済み。")]
        public int clearedStageCount = 10;

        /// <summary>
        /// クリア済みステージ数
        /// </summary>
        public static int ClearedStageCount { get { return Instance.clearedStageCount; } }

        /// <summary>
        /// 全ステージ数
        /// </summary>
        public const int StageCount = 10;

        /// <summary>
        /// クリア済みステージ数の保存用キー
        /// </summary>
        const string ClearedStageCountKey = "ClearedStageCount";

        /// <summary>
        /// ステージ選択で選ばれたステージ。0=Stage1
        /// </summary>
        public static int SelectedStage { get; private set; }

        public static bool IsActionAndWaterButtonDown {
            get {
                return Input.GetButtonDown("Action") || Input.GetButtonDown("Water");
            }
        }

        private void Awake()
        {
            LoadClearedStageCount();

            SelectedStage = Mathf.Min(clearedStageCount, StageCount-1);
            if (SelectedStage == StageCount - 1)
            {
                SelectedStage = 0;
            }
        }

        /// <summary>
        /// 新規にゲームを開始するための初期化
        /// </summary>
        public static void SetNewGame()
        {
            Instance.clearedStageCount = 0;
            SaveClearedStageCount();

            SelectedStage = 0;
        }

        /// <summary>
        /// 現在のクリアステージカウントを保存する。
        /// </summary>
        public static void SaveClearedStageCount()
        {
            PlayerPrefs.SetInt(ClearedStageCountKey, Instance.clearedStageCount);
        }

        /// <summary>
        /// 保存してあるクリアステージカウントを読み出す。
        /// </summary>
        public static void LoadClearedStageCount()
        {
            Instance.clearedStageCount = PlayerPrefs.GetInt(ClearedStageCountKey, Instance.clearedStageCount);
        }

        /// <summary>
        /// コンティニューのための初期化
        /// </summary>
        public static void SetContinue()
        {
            if (ClearedStageCount >= StageCount)
            {
                // クリア済みの時は最初のステージ
                SelectedStage = 0;
            }
            else
            {
                // プレイ中の時は最後のステージ
                SelectedStage = ClearedStageCount;
            }
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
            SelectedStage = (SelectedStage == 0) ? ClearedStageCount : SelectedStage-1;
            SelectedStage = Mathf.Min(SelectedStage, StageCount - 1);
        }

        /// <summary>
        /// ステージクリア時に呼び出して、パラメーターを更新する。
        /// </summary>
        public static void StageClear()
        {
            Instance.toStageSelect = StageSelectManager.ToStageSelectType.Clear;
            if (SelectedStage == ClearedStageCount-1)
            {
                Instance.clearedStageCount++;

                // TODO: PlayerPrefsで、ClearedStageCountを保存
            }
        }
    }
}
