using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class Stage1Scenario : MonoBehaviour
    {
        [Tooltip("花を咲かせた時に消しておくイベントエミッター"), SerializeField]
        GameObject destroyEmitterWhenOpenFlower = null;


        enum ScenarioState
        {
            WaitStart,
            WaterControl,
            SideControl,
            Escape,
            Done,
        };

        MessageManager.MessageType[] messages =
        {
            MessageManager.MessageType.None,
            MessageManager.MessageType.WaterKey,
            MessageManager.MessageType.MoveKey,
            MessageManager.MessageType.Escape,
        };

        ScenarioState state = ScenarioState.WaitStart;
        const float shortTime = -0.5f;
        float waitTime;
        bool isFlowered = false;

        private void Start()
        {
            waitTime = 0;
        }

        private void FixedUpdate()
        {
            if (Fade.IsFading) { return; }

            waitTime += Time.fixedDeltaTime;

            switch(state)
            {
                case ScenarioState.WaitStart:
                    waitTime = MessageManager.MessageMaxTime;
                    state++;
                    break;

                case ScenarioState.WaterControl:
                case ScenarioState.SideControl:
                case ScenarioState.Escape:
                    if (waitTime < (MessageManager.MessageMaxTime + shortTime)) break;
                    MessageManager.instance.SetMessage(messages[(int)state]);
                    waitTime = 0f;
                    state++;
                    break;
            }
        }

        /// <summary>
        /// 上下操作
        /// </summary>
        public void UpDown()
        {
            MessageManager.instance.SetMessage(MessageManager.MessageType.Ivy);
        }

        /// <summary>
        /// 緑にする
        /// </summary>
        public void LetsGreen()
        {
            MessageManager.instance.SetMessage(MessageManager.MessageType.Stage1Nice);
            FlowerBridge();
        }

        /// <summary>
        /// 花を咲かせる声
        /// </summary>
        public void FlowerBridge()
        {
            if (isFlowered) return;

            isFlowered = true;
            if (destroyEmitterWhenOpenFlower != null)
            {
                Destroy(destroyEmitterWhenOpenFlower);
            }
            SoundController.Play(SoundController.SeType.GimmickFlowerBridge);
        }

        /// <summary>
        /// クリア可能状態になった時の処理
        /// </summary>
        public void CanClear()
        {
            MessageManager.instance.SetMessage(MessageManager.MessageType.CanClear);
        }
    }
}

