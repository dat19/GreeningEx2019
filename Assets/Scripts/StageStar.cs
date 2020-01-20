using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class StageStar : MonoBehaviour
    {
        [Tooltip("回転速度"), SerializeField]
        float angularVelocity = 90f;
        [Tooltip("クリア演出時、回転速度を上げる"), SerializeField]
        float clearAngularVelocityRate = 6f;
        [Tooltip("星のマテリアル。0=最初, 1=完成直前, 2=完成形"), SerializeField]
        Material[] materials = new Material[3];

        /// <summary>
        /// アニメStateの値
        /// </summary>
        public enum AnimType
        {
            Hide,
            Show,
            ForceShow,
        };

        /// <summary>
        /// 担当するステージ。Stage1が0
        /// </summary>
        public int myStage = 0;

        /// <summary>
        /// Y軸の現在の値
        /// </summary>
        float rotateY = 0;
        float angularRate = 1f;

        Camera mainCamera = null;
        MeshRenderer myRenderer = null;
        Animator myAnimator = null;

        private void Awake()
        {
            myRenderer = GetComponentInChildren<MeshRenderer>();
            myAnimator = GetComponent<Animator>();
        }

        void Update()
        {
            if (myStage == GameParams.SelectedStage)
            {
                rotateY += Mathf.Repeat(angularVelocity * Time.deltaTime * angularRate, 360f);
            }
            else
            {
                rotateY = 0f;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            Vector3 forward = Quaternion.Euler(0, rotateY, 0) * mainCamera.transform.forward;
            transform.rotation = Quaternion.LookRotation(forward);
        }

        /// <summary>
        /// 指定のレートで星のマテリアルを設定します。
        /// </summary>
        /// <param name="rate"></param>
        public void SetMaterialRate(float rate)
        {
            if (rate >= 1f)
            {
                angularRate = 1f;
                myRenderer.material = materials[(int)Goal.MaterialIndex.Completed];
            }
            else
            {
                angularRate = clearAngularVelocityRate;
                myRenderer.material.Lerp(
                    materials[(int)Goal.MaterialIndex.First],
                    materials[(int)Goal.MaterialIndex.Last], rate);
            }
        }

        /// <summary>
        /// 指定のアニメを設定します。
        /// </summary>
        /// <param name="type">StageStar.AnimTypeで指定</param>
        public void SetAnimState(AnimType type)
        {
            myAnimator.SetInteger("State", (int)type);
        }
    }
}