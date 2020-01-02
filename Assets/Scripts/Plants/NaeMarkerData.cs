using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(fileName ="NaeMarkerData", menuName ="Greening/Create NaeMarkerData")]
    public class NaeMarkerData : ScriptableObject
    {
        [Tooltip("苗マーカープレハブ。NaeActableのNaeTypeの並び順で設定すること。"), SerializeField]
        GameObject[] markerPrefabs = null;

        /// <summary>
        /// マーカーのインスタンス
        /// </summary>
        public static GameObject[] markerObjects { get; private set; }

        public void Init()
        {
            if (markerObjects == null)
            {
                markerObjects = new GameObject[System.Enum.GetNames(typeof(NaeActable.NaeType)).Length];
                for (int i = 0; i < markerObjects.Length; i++)
                {
                    markerObjects[i] = Instantiate(markerPrefabs[i]);
                    markerObjects[i].SetActive(false);
                }
            }
        }
    }
}