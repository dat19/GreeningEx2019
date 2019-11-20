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
        /// <summary>
        /// クリア済みのステージ数。0なら全くクリアしていない。1ならStage1をクリア済み。
        /// </summary>
        public static int ClearedStage { get; private set; }
    }
}
