//#define DEBUG_SPHERE    // メッシュの位置をSphereで示すデバッグ
#define DEBUG_LOG

using System.Collections;
using System.Collections.Generic;
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
        const int SeaTextureSize = 128;
        /// <summary>
        /// 島からの影響距離
        /// </summary>
        const float IslandDistance = 4f / 128f;

        /// <summary>
        /// 実際に海に貼り付けるテクスチャー
        /// </summary>
        Texture2D seaTexture;
        Color32[] seaColors;
        float[] prevRate;

        Color32[] dirtyColors;
        Color32[] cleanColors;

        private void Start()
        {
            // 実際に貼り付けるテクスチャーの作成
            seaTexture = new Texture2D(SeaTextureSize, SeaTextureSize, dirtySeaTexture.format, false);
            seaColors = seaTexture.GetPixels32();

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

            // 1面クリア状態で試す
            float [] beforeRates = CalcCleanRate(2);

            dirtyColors = dirtySeaTexture.GetPixels32();
            cleanColors = cleanSeaTexture.GetPixels32();
            dirtyColors[0].a = 0;

            for (int i = 0; i < seaColors.Length; i++)
            {
                seaColors[i] = Color32.Lerp(dirtyColors[0], cleanColors[0], Mathf.Clamp01(beforeRates[i]));
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

        /// <summary>
        /// クリアしたステージの数(0～GameParams.StageMax)を受け取って、
        /// そのクリア状態に対応した海のテクスチャの割合の配列を返します。
        /// </summary>
        /// <param name="clearedCount">0～GameParams.StageMax</param>
        /// <returns>算出したクリア前とクリア後のブレンド値(0～1)</returns>
        float[] CalcCleanRate(int clearedCount)
        {
            float[] rates = new float[SeaTextureSize * SeaTextureSize];
            Vector2[][] uvs = new Vector2[GameParams.StageMax][];

            // 全ての島の頂点をすべて算出して、UV値に変換する
            for (int i=0;i<islands.Length;i++)
            {
                MeshFilter mf = meshIslands[i].GetComponent<MeshFilter>();
                uvs[i] = new Vector2[mf.mesh.vertexCount];
                for (int j=0;j<mf.mesh.vertexCount;j++)
                {
                    Vector3 pos = meshIslands[i].transform.TransformPoint(mf.mesh.vertices[j]);
                    // UV値を求める
                    uvs[i][j] = GetUV(pos);
#if DEBUG_SPHERE
                    if (i == 0)
                    {
                        Instantiate(debugSphere, pos, Quaternion.identity, transform);
                        Log($"  [{j}] pos={pos.x}, {pos.y}, {pos.z} / uv={uvs[i][j].x}, {uvs[i][j].y}");
                    }
#endif
                }
            }

            // 各頂点の影響値を算出する
            int idx = 0;
            Vector2 check = Vector2.zero;
            for (int y=0;y<SeaTextureSize;y++)
            {
                for (int x=0;x<SeaTextureSize;x++, idx++)
                {
                    rates[idx] = 0;
                    check.Set((float)x / (float)SeaTextureSize, (float)y / (float)SeaTextureSize);
                    for (int i=0;i<GameParams.StageMax;i++)
                    {
                        float clearScore = i < clearedCount ? 1 : -1;

                        // 各島の頂点ごとのUV値
                        for (int vi=0;vi<uvs[i].Length;vi++)
                        {
                            Vector2 mag = check - uvs[i][vi];
                            // 0.5を超えたらループ
                            if (mag.x < -0.5f) mag.x += 1f;
                            if (mag.x > 0.5f) mag.x -= 1f;
                            if (mag.y < -0.5f) mag.y += 1f;
                            if (mag.y > 0.5f) mag.y -= 1f;
                            // 影響範囲チェック
                            if (mag.magnitude < IslandDistance)
                            {
                                float rate = 1f-(mag.magnitude / IslandDistance);
                                rates[idx] += clearScore * rate;
                            }
                        }
                    }
                }
            }

            return rates;
        }

        /// <summary>
        /// 指定の方向
        /// </summary>
        /// <param name="dir">中心からの方向</param>
        /// <returns>uv値。0～1で正規化した値を返します。</returns>
        public static Vector2 GetUV(Vector3 dir)
        {
            Vector2 uv = Vector2.zero;

            // 角度を測るときの中心のベクトル
            Vector3 baseDir = new Vector3(dir.x, 0f, dir.z);
            if (baseDir.magnitude < 0.0001f)
            {
                // 真上
                if (dir.y > 0f)
                {
                    return Vector2.zero;
                }
                // 真下
                return Vector2.one*0.999f;
            }

            baseDir.Normalize();

            // u値は、右ベクトルとのなす角
            float angle = Vector3.SignedAngle(Vector3.forward, baseDir, Vector3.up);
            uv.x = -angle / 360f + (11f / 16f);
            if (uv.x > 1f)
            {
                uv.x -= 1f;
            }
            Log($"  angle={angle} / uv.x={uv.x}");

            // v値は、baseDirと、dirのなす角
            float dot = Vector3.Dot(baseDir, dir.normalized);
            if (Mathf.Approximately(dot, 1f))
            {
                // ほぼ1の時はベクトル一致なので、なす角は0
                Log($"  zero");
                uv.y = 0.5f;
            }
            else
            {
                angle = Vector3.Angle(baseDir, dir);
                if (dir.y > 0f) { angle = -angle; }
                Log($"  angle y={angle}");
                uv.y = (-angle+90f) / 180f;
                Log($"  baseDir={baseDir} / dir={dir} / uv.y={uv.y}");
            }

            return uv;
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}
