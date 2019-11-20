using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class EndingManager : SceneManagerBase
    {
#if DEBUG
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SoundController.Play(SoundController.SeType.Click);
                SceneChanger.ChangeScene(SceneChanger.SceneType.Title);
            }
        }
#endif
    }
}
