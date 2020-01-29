﻿//#define DEBUG_SPHERE    // メッシュの位置をSphereで示すデバッグ
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
        [Tooltip("緑化前の待ち秒数"), SerializeField]
        float beforeCleanWait = 0.2f;
        [Tooltip("緑化後の待ち秒数"), SerializeField]
        float afterCleanWait = 1f;
        [Tooltip("汚れ時の減衰レート"), SerializeField]
        float dirtyRate = 0.8f;

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
        /// 色を変更する汚れ島のマテリアルにアタッチするためのインスタンス
        /// </summary>
        Material workDirtyIslandMaterial;

        /// <summary>
        /// 実際に海に貼り付けるテクスチャー
        /// </summary>
        Texture2D seaTexture;
        Color32[] seaColors;
        /*
        float[] prevRate;
        byte[] seaTextureRates = null;
        */
        StageStar[] stageStars;
        AudioSource myAudioSource = null;

#if DEBUG_CALC_SPHERE_POS
        int debugX = 0;
        int debugY = 0;
#endif

        Animator islandCleanAnim = null;

        private void Awake()
        {
            islandCleanAnim = GetComponent<Animator>();
            myAudioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            // ステージ上に星を生成
            int starCount = Mathf.Min(GameParams.ClearedStageCount+1, GameParams.StageMax);
            int displayCount = GameParams.ClearedStageCount;
            int cleanedCount = GameParams.ClearedStageCount - 1;

            if (GameParams.Instance.toStageSelect != StageSelectManager.ToStageSelectType.Clear)
            {
                // 戻った時
                displayCount = starCount;
                cleanedCount++;
            }

            stageStars = new StageStar[starCount];
            for (int i = 0; i < starCount; i++)
            {
                Vector3 pos = (islands[i].transform.position - transform.position).normalized * starRadius;
                GameObject go = Instantiate<GameObject>(starPrefab, transform);
                stageStars[i] = go.GetComponent<StageStar>();
                stageStars[i].myStage = i;

                go.transform.localPosition = pos;
                go.transform.up = pos.normalized;

                // 星を表示
                if (i < displayCount)
                {
                    stageStars[i].SetAnimState(StageStar.AnimType.ForceShow);
                }

                // 奇麗にする
                if (i < cleanedCount)
                {
                    stageStars[i].SetMaterialRate(1f);
                }
            }

            float rotateBackup = rotateRate;
            rotateRate = 1f;
            UpdateRotate();
            rotateRate = rotateBackup;
        }

        void FixedUpdate()
        {
#if DEBUG_CALC_SPHERE_POS
            return;
#endif

            UpdateRotate();
        }

        /// <summary>
        /// 選択したステージを正面に向けるように回転させます。
        /// </summary>
        void UpdateRotate()
        {
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

        Color32[] beforeSeaColors = null;
        Color32[] afterSeaColors = null;
        public void MakeSeaTexture()
        {
#if DEBUG_CALC_SPHERE_POS
            return;
#endif
            // 実際に貼り付けるテクスチャーの作成
            seaTexture = new Texture2D(SeaTextureSize, SeaTextureSize, dirtySeaTexture.format, false);
            seaColors = seaTexture.GetPixels32();

            // 前の海を生成
            int clearedIndex = GameParams.NowClearStage;
            if (GameParams.Instance.toStageSelect == StageSelectManager.ToStageSelectType.Clear)
            {
                beforeSeaColors = CalcSeaColors(clearedIndex);
                afterSeaColors = CalcSeaColors(GameParams.ClearedStageCount);
                UpdateSeaTexture(0f);
            }
            else
            {
                clearedIndex = GameParams.ClearedStageCount;
                beforeSeaColors = CalcSeaColors(clearedIndex);
                seaTexture.SetPixels32(beforeSeaColors);
                seaTexture.Apply();
            }

            // 最初の海の色を設定
            seaRenderer.material.mainTexture = seaTexture;

            // 島の状態を設定
            islandCleanAnim.SetFloat("Speed", 100f);
            islandCleanAnim.SetInteger("Cleared", clearedIndex);
        }

        /// <summary>
        /// 指定の割合で海のテクスチャーを合成して、海テクスチャーに反映させます。
        /// </summary>
        /// <param name="rate">0～1</param>
        public void UpdateSeaTexture(float rate)
        {
            for (int i = 0; i < seaColors.Length; i++)
            {
                seaColors[i] = Color32.Lerp(beforeSeaColors[i], afterSeaColors[i], rate);
            }

            seaTexture.SetPixels32(seaColors);
            seaTexture.Apply();
        }

        /// <summary>
        /// 指定のステージまでクリアした状態の海の色を生成して返します。
        /// </summary>
        /// <param name="stnum">クリアしたステージ数</param>
        /// <returns>Color32の配列</returns>
        Color32 []CalcSeaColors(int stnum)
        {
            Color32[] cols = new Color32[SeaTextureSize * SeaTextureSize];
            Color32 cleanColor = cleanSeaTexture.GetPixel(0, 0);
            cleanColor.a = 255;

            // 全クリなら100%奇麗な海
            if (stnum >= GameParams.StageMax)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i] = cleanColor;
                }
                return cols;
            }

            // クリア状態に応じて、ウェイトを算出
            float[] tempRates = new float[SeaTextureSize * SeaTextureSize];
            for (int i=0;i<tempRates.Length;i++)
            {
                tempRates[i] = 0f;
            }

            for (int st = 0; st < GameParams.StageMax; st++)
            {
                TextAsset asset = Resources.Load(SeaTextureRatesFileName + st) as TextAsset;
                Stream s = new MemoryStream(asset.bytes);
                BinaryReader br = new BinaryReader(s);
                byte[] rates = Decompress(br.ReadBytes((int)s.Length));

                for (int i = 0; i < rates.Length; i++)
                {
                    float t = Mathf.Clamp01(((float)rates[i]) / 256f);
                    // クリア済みならそのまま加算
                    if (st < stnum)
                    {
                        tempRates[i] += t;
                    }
                    else
                    {
                        // 未クリアなら2乗してから引く
                        t *= dirtyRate;
                        tempRates[i] -= t * t;
                    }
                }
            }

            // 海の色を取得
            Color32 dirtyColor = dirtySeaTexture.GetPixel(0, 0);
            dirtyColor.a = 0;

            // 色算出
            for (int i = 0; i < tempRates.Length; i++)
            {
                cols[i] = Color32.Lerp(dirtyColor, cleanColor, Mathf.Clamp01(tempRates[i]));
                Log($"  rates[{i % 128}, {i / 128} / {i} / {tempRates.Length}]={tempRates[i]} / col={cols[i]}");
            }
            return cols;
        }

        /// <summary>
        /// 星をアニメに合わせて奇麗に変化させます。
        /// </summary>
        public IEnumerator UpdateClean()
        {
            // 少し待ってから開始
            yield return new WaitForSeconds(beforeCleanWait);

            islandUp = 0;
            islandCleanAnim.SetFloat("Speed", 1f);
            islandCleanAnim.SetInteger("Cleared", GameParams.ClearedStageCount);
            MeshRenderer rend = dirtyIslands[GameParams.NowClearStage].GetComponent<MeshRenderer>();
            workDirtyIslandMaterial = new Material(rend.material);
            rend.material = workDirtyIslandMaterial;
            myAudioSource.Play();

            while (islandUp < 1f)
            {
                // 島の色を変化させます
                workDirtyIslandMaterial.color = Color.Lerp(dirtyIslandMaterial.color, cleanIslandMaterial.color, islandUp);

                // 海の色を変化させます
                UpdateSeaTexture(islandUp);

                // 星
                stageStars[GameParams.NowClearStage].SetMaterialRate(islandUp);

                yield return null;
            }

            // 変化を完了させます
            workDirtyIslandMaterial.color = cleanIslandMaterial.color;
            UpdateSeaTexture(1f);
            stageStars[GameParams.NowClearStage].SetMaterialRate(1f);

            // 奇麗にしたら少し待つ
            yield return new WaitForSeconds(afterCleanWait);

            // 新ステージの星を出現
            if (GameParams.ClearedStageCount < GameParams.StageMax)
            {
                stageStars[GameParams.ClearedStageCount].SetAnimState(StageStar.AnimType.Show);
            }
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}
