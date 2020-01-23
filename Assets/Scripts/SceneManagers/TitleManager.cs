using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

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
        [Tooltip("ミュートトグル"), SerializeField]
        Toggle seMuteToggle = null;
        [Tooltip("タイトルからムービーを再生するまでの秒数"), SerializeField]
        float movieInterval = 10f;
        [Tooltip("動画開始まで待つ間のマスク画像"), SerializeField]
        GameObject movieWrapImage = null;
        [Tooltip("Canvasアニメーター"), SerializeField]
        Animator canvasAnimator = null;
        [Tooltip("動画が終わってからフェードするまでの待ち秒数"), SerializeField]
        float waitMovie = 2f;
        [Tooltip("クレジットスクロールビュー"), SerializeField]
        GameObject creditScrollView = null;

        enum StateType
        {
            Opening,
            FadeOut,
            Title,
            FadeIn,
            Credit,
        }

        /// <summary>
        /// コンティニューかどうかのフラグ
        /// </summary>
        public static bool IsContinue { get; private set; }

        /// <summary>
        /// ミュート前の効果音ボリューム
        /// </summary>
        float lastSeVolume = 0;

        VideoPlayer videoPlayer = null;
        StateType state = StateType.FadeOut;
        float startTime = 0f;
        bool isAnimDone = false;

        public override void OnFadeOutDone()
        {
            SceneManager.SetActiveScene(gameObject.scene);
            state = StateType.FadeOut;

            movieWrapImage.SetActive(true);

            videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.Prepare();

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

            SeMute();
        }

        public override void OnFadeInDone()
        {
            base.OnFadeInDone();

            StartMovie();
        }

        /// <summary>
        /// ゲーム開始メニューの表示を開始します。
        /// </summary>
        void StartTitle()
        {
            // SoundController.PlayBGM(SoundController.BgmType.Title, true);
            startTime = Time.time;
            state = StateType.Title;
            videoPlayer.Stop();
        }

        /// <summary>
        /// オープニングムービーを開始します。
        /// </summary>
        void StartMovie()
        {
            SoundController.StopBGM();
            videoPlayer.Play();
            state = StateType.Opening;
        }

        private void Update()
        {
            if (Fade.IsFading 
                || SceneChanger.NextScene != SceneChanger.SceneType.None
                || SceneChanger.NowScene != SceneChanger.SceneType.Title) return;

            switch(state)
            {
                case StateType.Opening:
                    if (videoPlayer.time <= 0)
                    {
                        break;
                    }
                    if (movieWrapImage.activeSelf)
                    {
                        movieWrapImage.SetActive(false);
                    }
                    canvasAnimator.SetBool("Show", true);
                    if (!videoPlayer.isPlaying)
                    {
                        state = StateType.FadeOut;
                        StartCoroutine(movieFadeOut(waitMovie));
                    }else if(GameParams.IsActionAndWaterButtonDown)
                    {
                        state = StateType.FadeOut;
                        StartCoroutine(movieFadeOut(0));
                    }
                    break;

                case StateType.FadeOut:
                    break;

                case StateType.Title:
                    updateTitle();
                    break;

                case StateType.Credit:
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        ShowCredit(false);
                    }
                    break;
            }
        }

        /// <summary>
        /// 動画が終わったら再生します。
        /// </summary>
        /// <param name="time">待ち秒数</param>
        IEnumerator movieFadeOut(float time)
        {
            yield return new WaitForSeconds(time);

            isAnimDone = false;
            canvasAnimator.SetBool("Show", false);

            // フェードが終わったら、タイトル開始
            while (!isAnimDone)
            {
                yield return null;
            }
            StartTitle();
        }

        void updateTitle()
        {
            if (GameParams.IsActionAndWaterButtonDown)
            {
                SoundController.Play(SoundController.SeType.Decision);

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

            // 動画再生チェック
            if ((Time.time-startTime) > movieInterval)
            {
                StartMovie();
                return;
            }

            // クリアステージがなければ変更なし
            if (GameParams.ClearedStageCount == 0) return;

            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                SoundController.Play(SoundController.SeType.MoveCursor);
                IsContinue = true;
                startTime = Time.time;
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                SoundController.Play(SoundController.SeType.MoveCursor);
                IsContinue = false;
                startTime = Time.time;
            }
        }

        /// <summary>
        /// SEの消音を設定します。
        /// </summary>
        public void SeMute()
        {
            if (SoundController.SeVolume > 0f)
            {
                lastSeVolume = SoundController.SeVolume;
            }

            SoundController.SeVolume = !seMuteToggle.isOn ? 0f : lastSeVolume;
            SoundController.Play(SoundController.SeType.MoveCursor);
        }

        /// <summary>
        /// アニメが完了したなどのイベントが実行されたフラグを記録します。
        /// </summary>
        public void AnimDone()
        {
            isAnimDone = true;
        }

        /// <summary>
        /// クレジットの表示、非表示を設定します。
        /// </summary>
        public void ShowCredit(bool flag)
        {
            SoundController.Play(SoundController.SeType.MoveCursor);
            creditScrollView.SetActive(flag);
            state = flag ? StateType.Credit : StateType.Title;
            startTime = Time.time;
        }
    }
}
