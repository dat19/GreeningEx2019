using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class DandelionFlower : Grow
    {
        [Tooltip("綿毛を放出する時のオフセット座標"), SerializeField]
        Vector3 fluffOffset = new Vector3(0f, 0.5f, 0);
        [Tooltip("綿毛プレハブ"), SerializeField]
        GameObject Fluff = null;
        [Tooltip("最初に綿毛を発生させるまでの秒数"), SerializeField]
        float firstFluffTime = 1f;
        [Tooltip("2回目以降に綿毛を発生させる間隔秒数"), SerializeField]
        float insTime = 2;
        [Tooltip("綿毛を飛ばす速度"), SerializeField]
        Vector2 direction = new Vector2(0.3f, 0.3f);
        [Tooltip("綿毛を掴んでからの寿命"), SerializeField]
        float FluffLifeTime = 5f;

        /// <summary>
        /// 次の綿毛を発生させるまでの残り秒数
        /// </summary>
        float fluffTime;

        /// <summary>
        /// アニメーションで開花が完了したら呼び出すメソッド。
        /// </summary>
        public void ToFluff()
        {
            fluffTime = firstFluffTime;
            GrowDone();
        }

        private void FixedUpdate()
        {
            if (state == StateType.Growed)
            {
                fluffTime -= Time.fixedDeltaTime;
                if (fluffTime <= 0f)
                {
                    fluffTime = insTime;
                    SoundController.Play(SoundController.SeType.SpawnFluff);
                    GameObject Go = Instantiate(Fluff, transform.position + fluffOffset, Quaternion.identity);
                    Go.GetComponent<FluffActable>().Init(direction, FluffLifeTime);
                }
            }
        }
    }
}