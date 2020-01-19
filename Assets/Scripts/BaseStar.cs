//#define DEBUG_SPHERE    // メッシュの位置をSphereで示すデバッグ
//#define DEBUG_LOG
//#define DEBUG_CALC_SPHERE_POS   // 球体の座標の算出コードのテスト

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.IO.Compression;

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
        [Tooltip("汚れ島のオブジェクト"), SerializeField]
        GameObject[] dirtyIslands = new GameObject[GameParams.StageMax];
        [Tooltip("星のマテリアル"), SerializeField]
        Material starMaterial = null;
        [Tooltip("汚れ島のマテリアル"), SerializeField]
        Material dirtyIslandMaterial = null;
        [Tooltip("奇麗島のマテリアル"), SerializeField]
        Material cleanIslandMaterial = null;
        [Tooltip("汚れ海テクスチャ"), SerializeField]
        Texture2D dirtySeaTexture = null;
        [Tooltip("緑化海テクスチャ"), SerializeField]
        Texture2D cleanSeaTexture = null;
        [Tooltip("海のレンダラー"), SerializeField]
        MeshRenderer seaRenderer = null;

#if DEBUG_SPHERE || DEBUG_CALC_SPHERE_POS
        [Tooltip("デバッグ用のSphereを置く半径"), SerializeField]
        float debugSphereRadius = 3.2f;
        [Tooltip("デバッグ用のSphere"), SerializeField]
        GameObject debugSphere = null;
#endif

        /// <summary>
        /// 島の上昇段階
        /// </summary>
        public float islandUp = 0f;

        /// <summary>
        /// 海用テクスチャーの大きさ
        /// </summary>
        public const int SeaTextureSize = 128;

        public const string SeaTextureRatesFileName = "SeaTextureRates";

        /// <summary>
        /// 実際に海に貼り付けるテクスチャー
        /// </summary>
        Texture2D seaTexture;
        Color32[] seaColors;
        float[] prevRate;

        Color32[] dirtyColors;
        Color32[] cleanColors;

        byte[] seaTextureRates = null;

#if DEBUG_CALC_SPHERE_POS
        int debugX = 0;
        int debugY = 0;
#endif

        private void Start()
        {
            // ステージ上に星を生成
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
#if DEBUG_CALC_SPHERE_POS
            return;
#endif
            Vector3 stageDir = (islands[GameParams.SelectedStage].transform.position - transform.position).normalized;
            Vector3 axis = Vector3.Cross(stageDir, Vector3.back);
            float angle = Vector3.SignedAngle(stageDir, Vector3.back, axis);
            transform.RotateAround(transform.position, axis, angle * rotateRate);
        }

#if DEBUG_CALC_SPHERE_POS
        private void Update()
        {
            bool ctrl = Input.GetKey(KeyCode.LeftControl);
            bool up = Input.GetKeyDown(KeyCode.UpArrow)
                || (Input.GetKey(KeyCode.UpArrow) && ctrl);
            bool down = Input.GetKeyDown(KeyCode.DownArrow)
                || (Input.GetKey(KeyCode.DownArrow) && ctrl);
            bool right = Input.GetKeyDown(KeyCode.RightArrow)
                || (Input.GetKey(KeyCode.RightArrow) && ctrl);
            bool left = Input.GetKeyDown(KeyCode.LeftArrow)
                || (Input.GetKey(KeyCode.LeftArrow) && ctrl);

            if (up)
            {
                debugY = debugY < 0 ? SeaTextureSize - 1 : debugY-1;
            }else if (down)
            {
                debugY = debugY >= SeaTextureSize ? 0 : debugY + 1;
            }
            if (left)
            {
                debugX = debugX < 0 ? SeaTextureSize - 1 : debugX - 1;
            }
            else if (right)
            {
                debugX = debugX >= SeaTextureSize ? 0 : debugX + 1;
            }

            Vector3 pos = MakeSeaRange.GetSphericalPoint(debugX, debugY);
            debugSphere.transform.position = pos * debugSphereRadius;
        }

        private void OnGUI()
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(20, 20, 100, 50), $"{debugX}, {debugY}");
        }
#endif

        static public byte[] Decompress(byte[] bytes)
        {
            using (var compressed = new MemoryStream(bytes))
            {
                using (var deflateStream = new DeflateStream(compressed, CompressionMode.Decompress))
                {
                    using (var decompressed = new MemoryStream())
                    {
                        deflateStream.CopyTo(decompressed);
                        return decompressed.ToArray();
                    }
                }
            }
        }
        public void MakeSeaTexture()
        {
#if DEBUG_CALC_SPHERE_POS
            return;
#endif

            // 実際に貼り付けるテクスチャーの作成
            seaTexture = new Texture2D(SeaTextureSize, SeaTextureSize, dirtySeaTexture.format, false);
            seaColors = seaTexture.GetPixels32();

            TextAsset asset = Resources.Load(SeaTextureRatesFileName+"0") as TextAsset;
            Stream s = new MemoryStream(asset.bytes);
            BinaryReader br = new BinaryReader(s);
            byte[] rates = Decompress( br.ReadBytes((int)s.Length));

            Log($"---- readed {rates.Length}");

            dirtyColors = dirtySeaTexture.GetPixels32();
            cleanColors = cleanSeaTexture.GetPixels32();
            dirtyColors[0].a = 0;

            Debug.Log($"  {dirtyColors[0]} / {cleanColors[0]}");

            for (int i = 0; i < seaColors.Length; i++)
            {
                float t = Mathf.Clamp01(((float)rates[i]) / 256f);
                seaColors[i] = Color32.Lerp(dirtyColors[0], cleanColors[0], t);
                Log($"  rates[{i%128}, {i/128} / {i} / {seaColors.Length}]={rates[i]} / t={t} / col={seaColors[i]}");
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

        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}
