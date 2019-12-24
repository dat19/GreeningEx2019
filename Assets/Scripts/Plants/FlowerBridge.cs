using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class FlowerBridge : MonoBehaviour
    {
        [Tooltip("花の橋のプレハブ"), SerializeField]
        GameObject flowerBridgePrefab = null;
        [Tooltip("花関連のスクリプタブルオブジェクト"), SerializeField]
        FlowerScriptableObject flowerScriptableObject = null;
        [Tooltip("花の橋の半径"), SerializeField]
        float radius = 3f;
        [Tooltip("花の橋の数"), SerializeField]
        int flowerCount = 8;
        [Tooltip("花の橋の右向きの開始点。左向きの時は、Xを逆にする"), SerializeField]
        Vector3 startOffset = new Vector3(0f, -0.4f, 0);
        [Tooltip("花の橋が跨ぐブロックの数。(この数+1+2*Xオフセットの幅)が、橋のアーチ"), SerializeField]
        float blockLength = 2f;
        [Tooltip("飾り花のオフセット。Xは算出するので、YとZのみ利用。手前用のデータを設定"), SerializeField]
        Vector3 kazariOffset = new Vector3(0, -0.1f, -0.4f);

        [Header("デバッグ")]
        [Tooltip("花の橋の方向。動作確認が完了したら、メソッドの引数にする"), SerializeField]
        float direction = 1;

        /// <summary>
        /// 花の橋のオブジェクト数
        /// </summary>
        List<GameObject> flowerList = new List<GameObject>();

        void Start()
        {
            if (flowerBridgePrefab == null) return;

            int fcount = flowerCount + (flowerCount - 1) * 2;

            while (flowerList.Count < fcount)
            {
                GameObject go = Instantiate(flowerBridgePrefab, transform);
                go.GetComponent<Animator>().SetTrigger("Flower");
                flowerList.Add(go);
                // 花を設定
                flowerScriptableObject.Flower(go.transform.GetChild(0).transform);
            }

            PutFlower(direction);
        }

        /// <summary>
        /// 花を指定の向きに配置する。
        /// </summary>
        /// <param name="dir">1=右方向、-1=左方向</param>
        public void PutFlower(float dir)
        {
            // 始点と終点
            float sx = transform.position.x + dir * startOffset.x;
            float ex = transform.position.x + dir * (blockLength + 1f - startOffset.x);
            float sey = transform.position.y + startOffset.y;

            Vector3 cp = Vector3.zero;
            // 中心座標 x = (ex^2-sx^2) / (2ex-2sx)
            cp.x = (ex * ex - sx * sx) / (ex - sx) * 0.5f;
            // 中心座標 y = sy - Sqrt(r^2 - (x-sx)^2)
            cp.y = sey - Mathf.Sqrt(radius * radius - (cp.x - sx) * (cp.x - sx));

            // 視点と終点へのベクトル
            Vector3 toStart = new Vector3(sx, sey) - cp;
            Vector3 toEnd = new Vector3(ex, sey) - cp;
            float rads = Vector3.Angle(Vector3.right, toStart) * Mathf.Deg2Rad;
            float rade = Vector3.Angle(Vector3.right, toEnd) * Mathf.Deg2Rad;

            // 花を配置
            for (int i = 0; i < flowerCount; i++)
            {
                // 中心の花
                float rad = Mathf.LerpAngle(rads, rade, (float)i / (float)(flowerCount - 1));
                flowerList[i*3].transform.position = cp + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
                // 飾り花
                if (i < flowerCount-1)
                {
                    rad = Mathf.LerpAngle(rads, rade, ((float)i+0.5f) / (float)(flowerCount - 1));
                    Vector3 kpos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
                    flowerList[i * 3 + 1].transform.position = cp + kpos + kazariOffset;
                    flowerList[i * 3 + 2].transform.position = cp + kpos - kazariOffset;
                }
            }
        }
    }
}