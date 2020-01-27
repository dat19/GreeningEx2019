using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class Stage4Scenario : MonoBehaviour
    {
        const float NextMessage = 4.5f;

        public void Rock()
        {
            MessageManager.instance.SetMessage(MessageManager.MessageType.Stage4Rock);
            SoundController.Play(SoundController.SeType.GimmickRock);
        }

        public void Action()
        {
            MessageManager.instance.SetMessage(MessageManager.MessageType.Stage4Action);
            Invoke("OnMessage", NextMessage);
        }

        void OnMessage()
        {
            MessageManager.instance.SetMessage(MessageManager.MessageType.Stage4On);
        }
    }
}