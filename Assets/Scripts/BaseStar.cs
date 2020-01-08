using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class BaseStar : MonoBehaviour
    {
        [Tooltip("各島のTransform"), SerializeField]
        Transform[] islands = new Transform[GameParams.StageCount];
        [Tooltip("回転率"), SerializeField]
        float rotateRate = 0.1f;
        [Tooltip("星のプレハブ"), SerializeField]
        GameObject starPrefab = null;
        [Tooltip("星の半径"), SerializeField]
        float starRadius = 4.5f;
        [Tooltip("星のマテリアル"), SerializeField]
        Material starMaterial = null;

        private void Start()
        {
            int count = Mathf.Min(GameParams.ClearedStageCount+1, GameParams.StageCount);
            int st = GameParams.ClearedStageCount;
            if (GameParams.ClearedStageCount == GameParams.StageCount)
            {
                st++;
            }
            for (int i = 0; i < count ; i++)
            {
                Vector3 pos = islands[i].transform.localPosition.normalized * starRadius;
                GameObject go = Instantiate<GameObject>(starPrefab, transform);
                go.transform.localPosition = pos;
                go.transform.up = pos.normalized;
                if (i < st)
                {
                    go.GetComponentInChildren<Renderer>().material = starMaterial;
                }
                go.GetComponent<StageStar>().myStage = i;
            }
        }

        void FixedUpdate()
        {
            Vector3 stageDir = (islands[GameParams.SelectedStage].transform.position - transform.position).normalized;
            Vector3 axis = Vector3.Cross(stageDir, Vector3.back);
            float angle = Vector3.SignedAngle(stageDir, Vector3.back, axis);
            transform.RotateAround(transform.position, axis, angle * rotateRate);
        }
    }
}