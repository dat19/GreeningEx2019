using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class Stage1Scenario : MonoBehaviour
    {
        enum ScenarioState
        {
            WaitStart,
            WaterControl,
            SideControl,
        }

        ScenarioState state = ScenarioState.WaitStart;
        const float shortTime = -0.5f;
        float waitTime;
        bool isFlowered = false;

        private void FixedUpdate()
        {
            if (Fade.IsFading) { return; }

            waitTime += Time.fixedDeltaTime;

            switch(state)
            {
                case ScenarioState.WaitStart:
                    MessageManager.instance.SetMessage(MessageManager.MessageType.WaterKey);
                    state++;
                    waitTime = 0f;
                    break;

                case ScenarioState.WaterControl:
                    if (waitTime < (MessageManager.MessageMaxTime + shortTime)) break;
                    state++;
                    MessageManager.instance.SetMessage(MessageManager.MessageType.MoveKey);
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

        public void FlowerBridge()
        {
            if (isFlowered) return;

            isFlowered = true;
            SoundController.Play(SoundController.SeType.GimmickFlowerBridge);
        }
    }
}

