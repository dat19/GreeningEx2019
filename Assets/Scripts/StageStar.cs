using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class StageStar : MonoBehaviour
    {
        [Tooltip("回転速度"), SerializeField]
        float angularVelocity = 90f;

        /// <summary>
        /// 担当するステージ。Stage1が0
        /// </summary>
        public int myStage = 0;

        /// <summary>
        /// Y軸の現在の値
        /// </summary>
        float rotateY = 0;

        Camera mainCamera = null;

        void Update()
        {
            if (myStage == GameParams.SelectedStage)
            {
                rotateY += Mathf.Repeat(angularVelocity * Time.deltaTime, 360f);
            }
            else
            {
                rotateY = 0f;
            }

            if (mainCamera ==null)
            {
                mainCamera = Camera.main;
            }

            Vector3 forward = Quaternion.Euler(0, rotateY, 0) * mainCamera.transform.forward;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}