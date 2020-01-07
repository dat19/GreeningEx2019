// #define AUTO_START

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class StarClean : MonoBehaviour
    {
        public static StarClean instance = null;

        [Tooltip("cleanの直下の島の設定用"), SerializeField]
        GameObject[] islands = null;

        //スタート前に設定 → 設定したステージまでの全ステージを緑化
        //スタート後に設定 → 設定したステージを緑化
        [Tooltip("クリアしたステージ"), SerializeField]
        int clearedStageNum = 0;
        [Tooltip("アニメーション速度"), SerializeField]
        float albedoChangeSpeed = 1.5f;

        static Renderer[][] islandsRenderer = null;

        static float albedoChange = 0f;
        static int hozonCleardStageNum = 0;
        static bool isStarted = false;

        private void Awake()
        {
            instance = this;
            isStarted = false;
            hozonCleardStageNum = 0;
        }

#if AUTO_START
    private void Start()
    {
        StartClearedStage(clearedStageNum);
    }
#endif

        void Update()
        {
            if (!isStarted) return;

            if (clearedStageNum != hozonCleardStageNum && albedoChange < 1)
            {
                Color color;
                albedoChange += albedoChangeSpeed * Time.deltaTime;
                for (int j = 0; j < islandsRenderer[clearedStageNum - 1].Length; j++)
                {
                    color = islandsRenderer[clearedStageNum - 1][j].material.color;
                    color.a = albedoChange;
                    islandsRenderer[clearedStageNum - 1][j].material.color = color;
                }
            }
            else if (clearedStageNum != hozonCleardStageNum && albedoChange >= 1)
            {
                hozonCleardStageNum = clearedStageNum;
                albedoChange = 0f;
            }
        }

        /// <summary>
        /// 指定のステージ
        /// </summary>
        /// <param name="num"></param>
        public static void StartClearedStage(int num)
        {
            instance.clearedStageNum = num;
            hozonCleardStageNum = instance.clearedStageNum;

            islandsRenderer = new Renderer[instance.islands.Length][];
            Color color;

            for (int i = instance.clearedStageNum; i < instance.islands.Length; i++)
            {
                islandsRenderer[i] = instance.islands[i].GetComponentsInChildren<Renderer>();
                for (int j = 0; j < islandsRenderer[i].Length; j++)
                {
                    color = islandsRenderer[i][j].material.color;
                    color.a = 0f;
                    islandsRenderer[i][j].material.color = color;
                }
            }

            isStarted = true;
        }

        /// <summary>
        /// クリアしたステージ数を設定します。
        /// </summary>
        /// <param name="num"></param>
        public static void SetClearedStageNum(int num)
        {
            instance.clearedStageNum = num;
        }

    }
}