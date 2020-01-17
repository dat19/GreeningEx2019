//#define DEBUG_SPHERE    // メッシュの位置をSphereで示すデバッグ
#define DEBUG_LOG

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GreeningEx2019
{
    public class BaseStar : MonoBehaviour
    {
        [Tooltip("各島のTransform"), SerializeField]
        Transform[] islands = new Transform[GameParams.StageMax];
        [Tooltip("回転率"), SerializeField]
        float rotateRate = 0.1f;
        [Tooltip("星のプレハブ"), SerializeField]
        GameObject starPrefab = null;
        [Tooltip("星の半径"), SerializeField]
        float starRadius = 4.5f;
        [Tooltip("海演算に使う各ステージの島のメッシュを持ったオブジェクト"), SerializeField]
        GameObject[] meshIslands = new GameObject[GameParams.StageMax];
        [Tooltip("星のマテリアル"), SerializeField]
        Material starMaterial = null;
        [Tooltip("汚れ海テクスチャ"), SerializeField]
        Texture2D dirtySeaTexture = null;
        [Tooltip("緑化海テクスチャ"), SerializeField]
        Texture2D cleanSeaTexture = null;
        [Tooltip("海のレンダラー"), SerializeField]
        MeshRenderer seaRenderer = null;

#if DEBUG_SPHERE
        [Tooltip("デバッグ用のSphereを置く半径"), SerializeField]
        float debugSphereRadius = 3.2f;
        [Tooltip("デバッグ用のSphere"), SerializeField]
        GameObject debugSphere = null;
#endif

        /// <summary>
        /// 海用テクスチャーの大きさ
        /// </summary>
        public const int SeaTextureSize = 128;

        public const string SeaTextureRatesFileName = "SeaTextureRates.bin";

        /// <summary>
        /// 実際に海に貼り付けるテクスチャー
        /// </summary>
        Texture2D seaTexture;
        Color32[] seaColors;
        float[] prevRate;

        Color32[] dirtyColors;
        Color32[] cleanColors;

        byte[] seaTextureRates = null;

        private void Start()
        {
            // 実際に貼り付けるテクスチャーの作成
            seaTexture = new Texture2D(SeaTextureSize, SeaTextureSize, dirtySeaTexture.format, false);
            seaColors = seaTexture.GetPixels32();

            TextAsset asset = Resources.Load("SeaTextureRatesFileName") as TextAsset;
            Stream s = new MemoryStream(asset.bytes);
            BinaryReader br = new BinaryReader(s);
            byte[] rates = br.ReadBytes((int)s.Length);

            Debug.Log($"---- readed {rates.Length}");

            /*
            // GetUVのチェック
            for (int i=1;i<2;i++)
            {
                for (int j=6;j<7;j++)
                {
                    Vector3 dir = Quaternion.Euler(360f * (float)(j-6) / 12.0f, 360f * (float)i / 12.0f, 0) * Vector3.forward;
                    Instantiate(debugSphere, dir * debugSphereRadius, Quaternion.identity, transform);
                    Vector2 uv = GetUV(dir);
                    int x = (int)((float)SeaTextureSize * uv.x);
                    x = Mathf.Min(x, SeaTextureSize - 1);
                    int y = (int)((float)SeaTextureSize * uv.y);
                    y = Mathf.Min(y, SeaTextureSize - 1);
                    Log($"  {x}, {y} = ({y*SeaTextureSize+x} / {seaColors.Length})");
                    seaColors[y * SeaTextureSize + x] = new Color32((byte)(255*j/8),(byte)(255*(8-j)/8),0,255);
                    Log($"[{i}, {j}] {dir.x}, {dir.y}, {dir.z} / uv={uv.x}, {uv.y}");
                }
            }
            */

            dirtyColors = dirtySeaTexture.GetPixels32();
            cleanColors = cleanSeaTexture.GetPixels32();
            dirtyColors[0].a = 0;

            for (int i = 0; i < seaColors.Length; i++)
            {
                seaColors[i] = Color32.Lerp(
                    dirtyColors[0],
                    cleanColors[0],
                    Mathf.Clamp01(((float)rates[i]))/256f);
            }
            seaTexture.SetPixels32(seaColors);
            seaTexture.Apply();
            seaRenderer.material.mainTexture = seaTexture;


            /*
                        Vector2 ret = GetUV(Vector3.right);
                        Debug.Log($"  {ret.x}, {ret.y}");
                        ret = GetUV(Vector3.left);
                        Debug.Log($"  {ret.x}, {ret.y}");
                        ret = GetUV(Vector3.back);
                        Debug.Log($"  {ret.x}, {ret.y}");
                        ret = GetUV(Vector3.up);
                        Debug.Log($"  {ret.x}, {ret.y}");
                        ret = GetUV(Vector3.down);
                        Debug.Log($"  {ret.x}, {ret.y}");
                        ret = GetUV(new Vector3(1,1,0));
                        Debug.Log($"  {ret.x}, {ret.y}");
                        ret = GetUV(new Vector3(1, -1, 0));
                        Debug.Log($"  {ret.x}, {ret.y}");

                        for (int i=0;i<islands.Length;i++)
                        {
                            Vector3 pos = islands[i].transform.position - transform.position;
                            pos.Normalize();
                            ret = GetUV(pos);
                            Debug.Log($"{i} / {ret.x}, {ret.y} / {pos.x}, {pos.y}, {pos.z}");
                        }
            */

            int count = Mathf.Min(GameParams.ClearedStageCount+1, GameParams.StageMax);
            int st = GameParams.ClearedStageCount;
            if (GameParams.ClearedStageCount == GameParams.StageMax)
            {
                st++;
            }
            for (int i = 0; i < count ; i++)
            {
                Vector3 pos = (islands[i].transform.position-transform.position).normalized * starRadius;
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

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}
