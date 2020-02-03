using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// メッセージデータを保持しておくクラス。
    /// messages.txtを読み込んで、列挙子で要求の文字列を返します。
    /// </summary>
    public class Messages : MonoBehaviour
    {
        [Tooltip("メッセージデータ"), SerializeField]
        TextAsset messageAsset = null;

        public enum MessageType
        {
            None = -1,
            // Stage1
            WaterKey,
            MoveKey,
            LetsWater,
            CantSwim,
            Ivy,
            ActionKey,
            LetsMove,
            LetsGreen,
            Stage1Nice,
            CanClear,
            Escape,
            // Stage2
            Stage2Watage,
            Stage2Release,
            Stage2TsutaFlower,
            // Stage3
            Stage3Mushroom,
            Stage3CanJump,
            Stage3How,
            Stage3CanNaeJump,
            Stage3RevJump,
            // Stage4
            Stage4Water,
            Stage4Rock,
            Stage4Action,
            Stage4On,
            Stage4Move,
            // Stage6
            Stage6Tempo,
            // StageName
            Stage1Name,
            Stage2Name,
            Stage3Name,
            Stage4Name,
            Stage5Name,
            Stage6Name,
            Stage7Name,
            Stage8Name,
            Stage9Name,
            Stage10Name,
        }

        static string[] messages = null;

        private void Awake()
        {
            string[] lines = messageAsset.text.Split(new char[] { '\n' });
            messages = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] cols = lines[i].Split(new char[] { ',' });
                if (cols.Length > 1)
                {
                    messages[i] = cols[1];
                }
            }
        }

        /// <summary>
        /// 指定のメッセージを返します。
        /// </summary>
        /// <param name="type">取得したいメッセージのID</param>
        /// <returns>対応するメッセージ文字列</returns>
        public static string GetMessage(MessageType type)
        {
            return messages[(int)type];
        }
    }
}