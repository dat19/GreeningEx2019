using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GreeningEx2019
{
    public class MessageManager : MonoBehaviour
    {
        public static MessageManager instance = null;

        [Tooltip("メッセージ表示先"), SerializeField]
        TextMeshProUGUI messageText = null;
        [Tooltip("メッセージボックスのアニメーター"), SerializeField]
        Animator messageAnim = null;
        [Tooltip("メッセージの最低表示秒数"), SerializeField]
        float messageMinTime = 2f;
        [Tooltip("メッセージの最大表示秒数"), SerializeField]
        float messageMaxTime = 5f;

        /// <summary>
        /// メッセージを表示した時のTime.time
        /// </summary>
        static float messageStartTime = 0;

        static List<string> messageList = new List<string>();
        static bool messageBoxShowed = false;

        private void Awake()
        {
            instance = this;
            messageList.Clear();
            messageBoxShowed = false;
            messageAnim.SetBool("Show", false);
        }

        private void FixedUpdate()
        {
            messageStartTime += Time.fixedDeltaTime;

            // メッセージの開始を確認
            if (!messageBoxShowed && (messageList.Count > 0))
            {
                messageBoxShowed = true;
                messageAnim.SetBool("Show", true);
                NextText();
                return;
            }

            // メッセージの切り替えを確認
            if (messageBoxShowed
                && (messageList.Count > 0)
                && (messageStartTime > messageMinTime))
            {
                NextText();
            }

            // メッセージを消す確認
            if (messageBoxShowed
                &&  (messageList.Count == 0)
                &&  (messageStartTime > messageMaxTime))
            {
                messageBoxShowed = false;
                messageAnim.SetBool("Show", false);
            }
        }

        /// <summary>
        /// 次のメッセージを表示します。
        /// </summary>
        void NextText()
        {
            messageStartTime = 0;
            messageText.text = messageList[0];
            messageList.RemoveAt(0);
        }

        /// <summary>
        /// 指定の文字列をメッセージ表示キューに登録します。
        /// </summary>
        /// <param name="mes"></param>
        public void SetMessage(string mes)
        {
            messageList.Add(mes);
        }
    }
}