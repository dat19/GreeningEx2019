using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
namespace GreeningEx2019
{
    public class MakeSeaRange : MonoBehaviour
    {
        [Tooltip("海演算に使う各ステージの島のメッシュを持ったオブジェクト"), SerializeField]
        GameObject[] meshIslands = new GameObject[GameParams.StageMax];

        [Tooltip("データ生成を実行"), SerializeField]
        bool isRun = false;
        [Tooltip("海テクスチャーの一辺のサイズ"), SerializeField]
        int seaSize = 128;
        [Tooltip("頂点からの有効範囲。正規座標系"), SerializeField]
        float islandDistance = 4f / 128f;

        void Start()
        {
            byte[] writeData = new byte[seaSize * seaSize];

            for (int i=0; i<256;i++)
            {
                writeData[i] = (byte)i;
                Debug.Log($"{writeData[i]}");
            }
        }
    }
}
#endif
