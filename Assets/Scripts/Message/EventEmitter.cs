using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public delegate bool EventChecker();

    /// <summary>
    /// マップに配置して、プレイヤーが触れたら呼び出すUnityActionを設定するためのクラスです。
    /// </summary>
    public class EventEmitter : MonoBehaviour
    {
        [Tooltip("鳴らす音"), SerializeField]
        SoundController.SeType se = SoundController.SeType.None;
        [Tooltip("表示するメッセージ。複数設定すると、ランダムでどれか一つを表示します。"), SerializeField]
        MessageManager.MessageType []messages = new MessageManager.MessageType[0];
        [Tooltip("発動チェックをするイベント。未設定なら無条件で発動します。"), SerializeField]
        EventChecker canEmit = null;
        [Tooltip("発動させたいイベントがあったら登録します。"), SerializeField]
        UnityEngine.Events.UnityEvent events;

        /// <summary>
        /// 多重呼び出しを避けるためのフラグ
        /// </summary>
        bool isEmitted = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isEmitted)
            {
                if (canEmit != null && !canEmit())
                {
                    return;
                }

                isEmitted = true;

                events.Invoke();
                if (se != SoundController.SeType.None)
                {
                    SoundController.Play(se);
                }
                if (messages.Length > 0)
                {
                    int index = Random.Range(0, messages.Length);
                    MessageManager.instance.SetMessage(messages[index]);
                }

                Destroy(gameObject);
            }
        }
    }
}