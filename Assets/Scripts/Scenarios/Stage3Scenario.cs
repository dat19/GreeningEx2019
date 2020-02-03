using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class Stage3Scenario : MonoBehaviour
    {
        public void Mushroom()
        {
            MessageManager.instance.SetMessage(Messages.MessageType.Stage3CanJump);
        }
    }
}