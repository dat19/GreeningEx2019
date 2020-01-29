using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class SeaTextureMaker : MonoBehaviour
    {
        [Tooltip("汚れ島のオブジェクト"), SerializeField]
        GameObject[] dirtyIslands = new GameObject[10];
        [Tooltip("試しのSphere"), SerializeField]
        GameObject testSphere = null;

        void Start()
        {
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