using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName ="Greening/Create Flower Asset", fileName = "FlowerSOAsset")]
    public class FlowerScriptableObject : ScriptableObject
    {
        [Tooltip("生成する花モデル"), SerializeField]
        GameObject[] flowerModels=null;

        /// <summary>
        /// ランダムで花を生成して、指定のtransformの子供にする
        /// </summary>
        /// <returns>花の親のオブジェクトのtransform</returns>
        public GameObject Flower(Transform parent)
        {
            return Instantiate(flowerModels[Random.Range(0, flowerModels.Length)], parent);
        }
    }
}