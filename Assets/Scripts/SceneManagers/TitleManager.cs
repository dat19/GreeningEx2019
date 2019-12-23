using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GreeningEx2019
{
    public class TitleManager : SceneManagerBase
    {
        [Tooltip("New Gameの座標。0=保存無し / 1=保存あり"), SerializeField]
        Vector3[] newGamePositions = new Vector3[2];
        [Tooltip("New Gameテキストのオブジェクト"), SerializeField]
        GameObject newGameObject = null;
        [Tooltip("Continueテキストのオブジェクト"), SerializeField]
        GameObject continueObject = null;

        /// <summary>
        /// コンティニューかどうかのフラグ
        /// </summary>
        public static bool IsContinue { get; private set; }
        
        public override void OnFadeOutDone()
        {
            SoundController.PlayBGM(SoundController.BgmType.Title, true);
            SceneManager.SetActiveScene(gameObject.scene);
            if (GameParams.ClearedStageCount == 0)
            {
                newGameObject.transform.localPosition = newGamePositions[0];
                continueObject.SetActive(false);
                IsContinue = false;
            }
            else
            {
                newGameObject.transform.localPosition = newGamePositions[1];
                continueObject.SetActive(true);
                IsContinue = true;
            }
        }

        private void Update()
        {
            if (Fade.IsFading 
                || SceneChanger.NextScene != SceneChanger.SceneType.None
                || SceneChanger.NowScene != SceneChanger.SceneType.Title) return;

            if (GameParams.IsActionAndWaterButtonDown)
            {
                SoundController.Play(SoundController.SeType.Click);

                if (!IsContinue)
                {
                    GameParams.SetNewGame();
                }
                else
                {
                    GameParams.SetContinue();
                }
                SceneChanger.ChangeScene(SceneChanger.SceneType.StageSelect);
                return;
            }

            if (Input.GetButtonDown("Esc"))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            // クリアステージがなければ変更なし
            if (GameParams.ClearedStageCount == 0) return;

            if(Input.GetAxisRaw("Horizontal")>0)
            {
                IsContinue = true;
            }
            else if(Input.GetAxisRaw("Horizontal")<0)
            {
                IsContinue = false;
            }
        }
    }
}
