using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class Stage2Scenario : MonoBehaviour
    {
        public void Dandelion()
        {
            MessageManager.instance.SetMessage(Messages.MessageType.Stage2Watage);
        }
    }
}