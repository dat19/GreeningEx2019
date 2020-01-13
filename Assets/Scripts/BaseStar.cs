using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class BaseStar : MonoBehaviour
    {
        [Tooltip("各島のTransform"), SerializeField]
        Transform[] islands = new Transform[GameParams.StageCount];
        [Tooltip("回転率"), SerializeField]
        float rotateRate = 0.1f;
        [Tooltip("星のプレハブ"), SerializeField]
        GameObject starPrefab = null;
        [Tooltip("星の半径"), SerializeField]
        float starRadius = 4.5f;
        [Tooltip("星のマテリアル"), SerializeField]
        Material starMaterial = null;
        [Tooltip("汚れ海テクスチャ"), SerializeField]
        Texture2D dirtySeaTexture = null;
        [Tooltip("緑化海テクスチャ"), SerializeField]
        Texture2D cleanSeaTexture = null;
        [Tooltip("海のレンダラー"), SerializeField]
        MeshRenderer seaRenderer = null;

        Texture2D nowTexture;
        Color32[] dirtyColors;
        Color32[] cleanColors;
        Color32[] nowColors;

        private void Start()
        {
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



            dirtyColors = dirtySeaTexture.GetPixels32();
            cleanColors = cleanSeaTexture.GetPixels32();
            for (int i = 0; i < dirtyColors.Length; i++)
            {
                dirtyColors[i].a = 0;
            }

            nowTexture = new Texture2D(dirtySeaTexture.width, dirtySeaTexture.height, dirtySeaTexture.format, false);
            nowTexture.SetPixels32(dirtyColors);
            nowTexture.Apply();
            seaRenderer.material.mainTexture = nowTexture;

            int count = Mathf.Min(GameParams.ClearedStageCount+1, GameParams.StageCount);
            int st = GameParams.ClearedStageCount;
            if (GameParams.ClearedStageCount == GameParams.StageCount)
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
        /// 指定の方向
        /// </summary>
        /// <param name="dir">中心からの方向</param>
        /// <returns>uv値。0～1で正規化した値を返します。</returns>
        public static Vector2 GetUV(Vector3 dir)
        {
            Vector2 uv = Vector2.zero;

            // 角度を測るときの中心のベクトル
            Vector2 baseDir = new Vector2(dir.x, dir.z);
            if (Mathf.Approximately(baseDir.magnitude, 0f))
            {
                // 真上
                if (dir.y > 0f)
                {
                    return Vector2.zero;
                }
                // 真下
                return Vector2.one;
            }

            baseDir.Normalize();

            // u値は、右ベクトルとのなす角
            uv.x = (Vector2.SignedAngle(Vector2.right, baseDir))/360f + (7f/16f);

            // v値は、baseDirと、dirのなす角
            uv.y = (Vector2.SignedAngle(baseDir, dir)+180f)/360f;

            return uv;
        }
    }
}
