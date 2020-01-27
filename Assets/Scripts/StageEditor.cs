﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GreeningEx2019 {
    public class StageEditor : MonoBehaviour
    {
        [SerializeField]
        MapType selectedMapChip = MapType.GroundTop;
        [SerializeField]
        GameObject[] mapChipPrefabs = null;

        /// <summary>
        /// 配置するZ座標
        /// </summary>
        const float mapZ = 0;

        public enum MapType
        {
            None = -1,
            GroundTop,
            GroundUnder,
            WaterTop,
            WaterUnder,
            //
            FlowerLeaf,
            IvyLittle,
            DandelionLittle,
            MushroomLittle,
            RockBefore,
            //
            /*
            Flower,
            Ivy,
            Dandelion,
            Mushroom,
            RockAfter,
            */
        }

        /// <summary>
        /// ヒットチェック時の上限数
        /// </summary>
        const int HitMax = 4;

        /// <summary>
        /// マウスの場所にあったものとの衝突データ
        /// </summary>
        readonly RaycastHit[] hits = new RaycastHit[HitMax];

        private void Start()
        {
            if (SceneChanger.NowScene == SceneChanger.SceneType.StageEditor)
            {
                // フェードアウト
                Fade.Instance.StartCoroutine(Fade.StartFade(Fade.FadeStateType.In, 0f));

                enabled = true;
            }
            else
            {
                enabled = false;
            }
        }

        void Update()
        {
            Camera myCam = Camera.main;
            Vector3 mpos = myCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mapZ - myCam.transform.position.z));
            Vector3 cpos = myCam.transform.position;

            mpos.x = Mathf.Round(mpos.x);
            mpos.y = Mathf.Round(mpos.y);
            mpos.z = mapZ;

            if (Input.GetMouseButton(0))
            {
                // マウスでクリックした場所のマップ座標mpos
                int hitCount = Physics.RaycastNonAlloc(mpos + Vector3.back, Vector3.forward, hits);
                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        DestroyImmediate(hits[i].collider.gameObject);
                    }
                }

#if UNITY_EDITOR
                if (selectedMapChip != MapType.None)
                {
                    GameObject clone = PrefabUtility.InstantiatePrefab(mapChipPrefabs[(int)selectedMapChip]) as GameObject;
                    clone.transform.position = mpos;
                    clone.transform.SetParent(transform);
                }
#endif
            }

            //カメラ移動
            cpos.x += (Input.GetAxisRaw("Horizontal") / 4);
            cpos.y += (Input.GetAxisRaw("Vertical") / 4);
            myCam.transform.position = cpos;
        }
    }
}