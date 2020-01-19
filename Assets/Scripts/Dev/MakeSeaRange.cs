//#define DEBUG_GET_POINT
//#define DEBUG_CALC_RATE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
namespace GreeningEx2019
{
    public class MakeSeaRange : MonoBehaviour
    {
        public static MakeSeaRange instance = null;

        [Tooltip("海演算に使う各ステージの島のメッシュを持ったオブジェクト"), SerializeField]
        GameObject[] meshIslands = new GameObject[GameParams.StageMax];

        [Tooltip("データ生成を実行"), SerializeField]
        bool isRun = false;
        [Tooltip("頂点からの有効範囲。正規座標系"), SerializeField]
        float islandDistance = 0.2f;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            // 各ステージのクリア状態を生成
            for (int i=0;i<GameParams.StageMax;i++)
            {
                if (File.Exists($"Assets/Resources/{BaseStar.SeaTextureRatesFileName}{i}.bytes"))
                {
                    Debug.Log($"Stage{i}のデータがあるので作成キャンセル");
                    return;
                }

                byte[] beforeRates = CalcCleanRate(i);
                File.WriteAllBytes($"Assets/Resources/{BaseStar.SeaTextureRatesFileName}{i}.bytes", beforeRates);
            }
        }

        /// <summary>
        /// 指定の星から各テクスチャが受ける影響の値を算出して、0～255で返します。
        /// </summary>
        /// <param name="取得したいステージ番号">0～GameParams.StageMax</param>
        /// <returns>算出した影響(0～255)</returns>
        public static byte[] CalcCleanRate(int stg)
        {
            byte[] rates = new byte[BaseStar.SeaTextureSize * BaseStar.SeaTextureSize];
            Mesh mesh = instance.meshIslands[stg].GetComponent<MeshFilter>().mesh;
            Transform islandTransform = instance.meshIslands[stg].transform;

            // 各頂点の影響値を算出する
            int idx = 0;
            Vector3 check = Vector3.zero;
            for (int y = 0; y < BaseStar.SeaTextureSize; y++)
            {
                for (int x = 0; x < BaseStar.SeaTextureSize; x++, idx++)
                {
                    rates[idx] = 0;
                    check = GetSphericalPoint(x,y);
                    float temprate = 0f;

                    // 各島の頂点ごとのUV値
                    for (int vi = 0; vi < mesh.vertexCount ; vi++)
                    {
                        Vector3 meshpos = islandTransform.TransformPoint(mesh.vertices[vi]).normalized;
                        float dist = Vector3.Distance(check, meshpos);

                        // 影響範囲チェック
                        if (dist < instance.islandDistance)
                        {
                            float rate = 1f - (dist / instance.islandDistance);
                            temprate += rate;
                        }

#if DEBUG_CALC_RATE
                        if (x==12 && ((y >= 66) && (y<=66)))
                        {
                            Debug.Log($"  {vi} check={check.x}, {check.y}, {check.z} / ver={meshpos.x}, {meshpos.y}, {meshpos.z} / dist={dist} / range={instance.islandDistance} / temprate={temprate}");
                        }
#endif
                    }

                    int dt = (int)(temprate * 256f);
                    if (dt >= 256)
                    {
                        dt = 255;
                    }
                    rates[idx] = (byte)dt;
#if DEBUG_CALC_RATE
                    Debug.Log($"  ({x}, {y}) rates[{idx}] = {rates[idx]}");
#endif
                }
            }

            return rates;
        }

        /// <summary>
        /// UV値を受け取って、半径1の球体上の点に変換して返します。
        /// </summary>
        /// <param name="x">u値。0～テクスチャサイズで指定</param>
        /// <param name="y">v値。0～テクスチャサイズで指定</param>
        /// <returns>求めた半径1の球体表面の頂点</returns>
        public static Vector3 GetSphericalPoint(int x, int y)
        {
            // xからX-Z平面を求める
            Vector3 xz = Vector3.zero;
            float th = ((float)x - ((float)BaseStar.SeaTextureSize) * 0.5f) * Mathf.PI * 2f / (float)BaseStar.SeaTextureSize;
            th += (Mathf.PI *14f/ 128f);
            xz.Set(Mathf.Cos(th), 0f, Mathf.Sin(th));

            // yから上下の角度を求める
            //            float ylen = (2f-(((float)y * 2f) / (float)BaseStar.SeaTextureSize)) - 1f;
            //            float thy = Mathf.Asin(ylen);
            float thy = ((/*1f-*/(((float)y) / ((float)BaseStar.SeaTextureSize))) -0.5f) * Mathf.PI;
#if DEBUG_GET_POINT
            Debug.Log($"  {x}, {y}  thy={thy}");
#endif
            Vector3 axis = Vector3.Cross(xz, Vector3.up);
            Vector3 pos = Quaternion.AngleAxis(thy * Mathf.Rad2Deg, axis) * xz;

#if DEBUG_GET_POINT
            Debug.Log($"  GetSphericalPoint({x}, {y}) th={th} / thy={thy} / axis={axis} / xz={xz} / pos={pos}");
#endif

            return pos;
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
