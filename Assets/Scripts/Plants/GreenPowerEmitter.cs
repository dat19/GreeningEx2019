using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class GreenPowerEmitter : MonoBehaviour
    {
        public static GreenPowerEmitter instance = null;

        [Tooltip("緑パワープレハブ"), SerializeField]
        GameObject greenPowerPrefab = null;

        public const int SpawnCount = 8;

        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// 指定の座標に緑エネルギーを発生させます。
        /// </summary>
        /// <param name="pos"></param>
        public static void Emit(Vector3 pos)
        {
            for (int i=0;i<SpawnCount;i++)
            {
                Instantiate(instance.greenPowerPrefab, pos, Quaternion.identity);
            }
        }
    }
}