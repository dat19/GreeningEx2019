using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [System.Serializable]
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

#pragma warning disable 649
        [Tooltip("発動させたいイベントがあったら登録します。"), SerializeField]
        UnityEngine.Events.UnityEvent events;
#pragma warning restore 649

        /// <summary>
        /// 発動の条件がある場合、このクラスをオーバーライドして、
        /// 発動できる時にtrueを返すようにします。
        /// </summary>
        protected virtual bool canEmit { get { return true; } }

        /// <summary>
        /// 多重呼び出しを避けるためのフラグ
        /// </summary>
        bool isEmitted = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isEmitted && canEmit)
            {
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