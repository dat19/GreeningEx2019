using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class BGScroller : MonoBehaviour
    {
        public static BGScroller instance { get; private set; }

        [Tooltip("カメラの上限座標"), SerializeField]
        float cameraTop = 35f;
        [Tooltip("カメラの下限座標"), SerializeField]
        float cameraBottom = 3.5f;
        [Tooltip("背景オフセットの上限値"), SerializeField]
        float[] bgTops = {  0.04f, 0.04f, 1f};
        [Tooltip("背景オフセットの下限値"), SerializeField]
        float[] bgBottoms = { -0.04f, -0.04f, -0.25f };
        [Tooltip("背景オブジェクト。背景オフセットと同じ並び順にする"), SerializeField]
        Renderer[] bgObjects = null;

        static Material[] bgMaterials = null;
        static float cameraRange;
        static float []cameraToOffsetRates;

        private void Awake()
        {
            instance = this;
            cameraRange = cameraTop - cameraBottom;
            cameraToOffsetRates = new float[bgTops.Length];
            for (int i = 0; i < bgTops.Length; i++)
            {
                cameraToOffsetRates[i] = (bgTops[i] - bgBottoms[i]) / cameraRange;
            }
        }

        void Start()
        {
            GetMaterials();
        }

        /// <summary>
        /// スクロールを実行します。カメラを移動した後に呼び出します。
        /// </summary>
        public void Scroll()
        {
            Vector2 offset = Vector2.zero;

            for (int i=0; i<bgTops.Length;i++)
            {
                // X方向計算
                offset.x = transform.position.x * cameraToOffsetRates[i];

                // Y方向計算
                float t = (transform.position.y - cameraBottom) / cameraRange;
                offset.y = Mathf.Lerp(bgBottoms[i], bgTops[i], t);

                bgMaterials[i].mainTextureOffset = offset;
            }
        }

        /// <summary>
        /// 背景を切り替えた後、呼び出します。
        /// </summary>
        public void GetMaterials()
        {
            bgMaterials = new Material[bgObjects.Length];
            for (int i = 0; i < bgObjects.Length; i++)
            {
                bgMaterials[i] = bgObjects[i].material;
            }
        }

        /// <summary>
        /// 指定のマテリアルに切り替えます。
        /// </summary>
        /// <param name="mats">設定するマテリアル。空、星、木の順</param>
        public void ChangeMaterials(Material[] mats)
        {
            for (int i = 0; i < mats.Length;i++)
            {
                bgObjects[i].material = mats[i];
            }
        }
    }
}