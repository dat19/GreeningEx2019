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
        [Tooltip("頂点からの有効範囲。正規座標系"), SerializeField]
        float islandDistance = 4f / 128f;

        void Start()
        {
            // 1面クリア状態で試す
            byte[] beforeRates = CalcCleanRate(0);
            File.WriteAllBytes($"Output/{BaseStar.SeaTextureRatesFileName}", beforeRates);
        }

        /// <summary>
        /// 指定の星から各テクスチャが受ける影響の値を算出して、0～255で返します。
        /// </summary>
        /// <param name="取得したいステージ番号">0～GameParams.StageMax</param>
        /// <returns>算出した影響(0～255)</returns>
        byte[] CalcCleanRate(int stg)
        {
            byte[] rates = new byte[BaseStar.SeaTextureSize * BaseStar.SeaTextureSize];

            MeshFilter mf = meshIslands[stg].GetComponent<MeshFilter>();
            Vector2[] uvs = new Vector2[mf.mesh.vertexCount];
            for (int i = 0; i < mf.mesh.vertexCount; i++)
            {
                Vector3 pos = meshIslands[stg].transform.TransformPoint(mf.mesh.vertices[i]);
                // UV値を求める
                uvs[i] = GetUV(pos);
#if DEBUG_SPHERE
                if (i == 0)
                {
                    Instantiate(debugSphere, pos, Quaternion.identity, transform);
                    Log($"  [{j}] pos={pos.x}, {pos.y}, {pos.z} / uv={uvs[i][j].x}, {uvs[i][j].y}");
                }
#endif
            }

            // 各頂点の影響値を算出する
            int idx = 0;
            Vector2 check = Vector2.zero;
            for (int y = 0; y < BaseStar.SeaTextureSize; y++)
            {
                for (int x = 0; x < BaseStar.SeaTextureSize; x++, idx++)
                {
                    rates[idx] = 0;
                    check.Set(
                        (float)x / (float)BaseStar.SeaTextureSize,
                         (float)y / (float)BaseStar.SeaTextureSize);

                    // 各島の頂点ごとのUV値
                    for (int vi = 0; vi < uvs.Length; vi++)
                    {
                        Vector2 mag = check - uvs[vi];
                        // 0.5を超えたらループ
                        if (mag.x < -0.5f) mag.x += 1f;
                        if (mag.x > 0.5f) mag.x -= 1f;
                        if (mag.y < -0.5f) mag.y += 1f;
                        if (mag.y > 0.5f) mag.y -= 1f;
                        // 影響範囲チェック
                        if (mag.magnitude < islandDistance)
                        {
                            float rate = 1f - (mag.magnitude / islandDistance);
                            int dt = (int)(rate * 256f);
                            if (dt == 256)
                            {
                                dt = 255;
                            }
                            rates[idx] = (byte)dt;
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
                return Vector2.one * 0.999f;
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
                uv.y = (-angle + 90f) / 180f;
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
#endif
