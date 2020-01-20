//#define DEBUG_STAGE_CLEAR

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
        public const int StageMax = 10;

        /// <summary>
        /// クリア済みステージ数の保存用キー
        /// </summary>
        const string ClearedStageCountKey = "ClearedStageCount";

        /// <summary>
        /// ステージ選択で選ばれたステージ。0=Stage1
        /// </summary>
        public static int SelectedStage { get; private set; }

        /// <summary>
        /// 今クリアしたステージ。0=Stage1
        /// </summary>
        public static int NowClearStage { get; private set; }

        public static bool IsActionAndWaterButtonDown {
            get {
                return Input.GetButtonDown("Action") || Input.GetButtonDown("Water");
            }
        }

        public static bool IsActionAndWaterButton
        {
            get
            {
                return Input.GetButton("Action") || Input.GetButton("Water");
            }
        }

        private void Awake()
        {
            LoadClearedStageCount();
            PhysicsCaster.Init();

            SelectedStage = Mathf.Min(clearedStageCount, StageMax-1);
            if (SelectedStage == StageMax - 1)
            {
                SelectedStage = 0;
            }
        }

        /// <summary>
        /// 新規にゲームを開始するための初期化
        /// </summary>
        public static void SetNewGame()
        {
            SelectedStage = 0;
            Instance.clearedStageCount = 0;
            NowClearStage = 0;
            SaveClearedStageCount();
            Instance.toStageSelect = StageSelectManager.ToStageSelectType.NewGame;
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
            Instance.toStageSelect = StageSelectManager.ToStageSelectType.Back;

            if (ClearedStageCount >= StageMax)
            {
                // クリア済みの時は最初のステージ
                SelectedStage = 0;
            }
            else
            {
                // プレイ中の時は最後のステージ
                SelectedStage = ClearedStageCount;
            }

            NowClearStage = ClearedStageCount;

#if DEBUG_STAGE_CLEAR
            SelectedStage = NowClearStage = 9;
            Instance.clearedStageCount = NowClearStage + 1;
            Instance.toStageSelect = StageSelectManager.ToStageSelectType.Clear;
#endif
        }

        /// <summary>
        /// 選択しているステージ番号を、次のステージに変更。ClearedStageをオーバーしていたら0に戻す
        /// </summary>
        public static void NextSelectStage()
        {
            int div = Mathf.Min(StageMax, ClearedStageCount + 1);
            SelectedStage = (SelectedStage + 1) % div;
        }

        /// <summary>
        /// 選択しているステージ番号を、前のステージに変更。0を越えていたら、ClearedStageへ。
        /// </summary>
        public static void PrevSelectStage()
        {
            SelectedStage = (SelectedStage == 0) ? ClearedStageCount : SelectedStage-1;
            SelectedStage = Mathf.Min(SelectedStage, StageMax - 1);
        }

        /// <summary>
        /// ステージクリア時に呼び出して、パラメーターを更新する。
        /// </summary>
        public static void StageClear()
        {
            Instance.toStageSelect = StageSelectManager.ToStageSelectType.Clear;
            NowClearStage = SelectedStage;
            if (SelectedStage == ClearedStageCount)
            {
                Instance.clearedStageCount++;
                SaveClearedStageCount();
            }
        }
    }
}
