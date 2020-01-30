using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class FollowCamera : MonoBehaviour
    {
        #pragma warning disable 0414

        [Tooltip("カメラの画面左端"), SerializeField]
        float cameraLeft = 0.5f;
        [Tooltip("カメラの座標右端"), SerializeField]
        float cameraRight = 20.5f;
        [Tooltip("カメラの下端"), SerializeField]
        float cameraBottom = 5f;

        Transform playerTransform = null;
        Vector3 camToPlayer;

        /// <summary>
        /// ターゲットを設定
        /// </summary>
        /// <param name="tg">目的のオブジェクトのトランスフォーム</param>
        public void SetTarget(Transform tg)
        {
            playerTransform = tg;
            if (tg != null)
            {
                camToPlayer = playerTransform.position - transform.position;
            }
        }

        private void Awake()
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go == null)
            {
                return;
            }
            SetTarget(go.transform);
        }

        private void Start()
        {
            // エディター時は無効
            if (SceneChanger.NowScene == SceneChanger.SceneType.StageEditor)
            {
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!playerTransform || StageManager.IsClearPlaying) return;

            float x = (playerTransform.position.z - transform.position.z);
            float th = 5f;
            float y = Mathf.Tan(th * Mathf.Deg2Rad) * x;

            Vector3 next = playerTransform.position - camToPlayer;
            next.x = playerTransform.position.x;
            if (next.x < cameraLeft )
            {
                next.x = cameraLeft;
            }
            else if (next.x > cameraRight)
            {
                next.x = cameraRight;
            }

            next.y = playerTransform.position.y + y;
            if (next.y < cameraBottom)
            {
                next.y = cameraBottom;
            }

            transform.position = next;

            BGScroller.instance.Scroll();
        }
    }
}