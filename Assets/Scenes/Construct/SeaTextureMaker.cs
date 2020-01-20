using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class SeaTextureMaker : MonoBehaviour
    {
        [Tooltip("汚れ島のオブジェクト"), SerializeField]
        GameObject[] dirtyIslands = new GameObject[10];
        [Tooltip("汚れ海テクスチャ"), SerializeField]
        Texture2D dirtySeaTexture = null;
        [Tooltip("緑化海テクスチャ"), SerializeField]
        Texture2D cleanSeaTexture = null;
        [Tooltip("海のレンダラー"), SerializeField]
        MeshRenderer seaRenderer = null;
        [Tooltip("試しのSphere"), SerializeField]
        GameObject testSphere = null;

        Texture2D nowTexture;
        Color32[] dirtyColors;
        Color32[] cleanColors;
        Color32[] nowColors;

        void Start()
        {
            BaseStar baseStar = GetComponent<BaseStar>();

            for (int i = 0; i < dirtyIslands.Length; i++) {
                for (int j=0;j<dirtyIslands[i].transform.childCount;j++)
                {
                    Transform tr = dirtyIslands[i].transform.GetChild(j).gameObject.transform;
                    MeshFilter[] meshes = dirtyIslands[i].transform.GetChild(j).gameObject.GetComponents<MeshFilter>();
                    for (int k=0;k<meshes.Length;k++)
                    {
                        if (meshes[k].mesh == null) continue;
                        Vector3[] verts = meshes[k].mesh.vertices;
                        for (int l=0;l<verts.Length;l++)
                        {
                            Instantiate(testSphere, tr.TransformPoint(verts[l]), Quaternion.identity);
                        }
                    }
                }
            }
            


        }
    }
}