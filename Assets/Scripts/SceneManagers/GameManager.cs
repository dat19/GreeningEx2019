using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class GameManager : SceneManagerBase
    {
        [Tooltip("デバッグキーを有効にする"), SerializeField]
        bool useDebugKey = false;

        private void Update()
        {
#if DEBUG
            if (useDebugKey) {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    // エンディングへ
                    SceneChanger.ChangeScene(SceneChanger.SceneType.StageSelect);
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    // エンディングへ
                    SceneChanger.ChangeScene(SceneChanger.SceneType.Ending);
                }
            }
#endif
        }
    }
}
