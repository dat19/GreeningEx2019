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
        [Tooltip("生成間隔"), SerializeField]
        float insTime = 2;
        [Tooltip("綿毛を飛ばす速度"), SerializeField]
        Vector2 direction = new Vector2(0.3f, 0.3f);
        [Tooltip("綿毛を掴んでからの寿命"), SerializeField]
        float FluffLifeTime = 5f;

        float lastTime;

        /// <summary>
        /// アニメーションで開花が完了したら呼び出すメソッド。
        /// </summary>
        public void ToFluff()
        {
            lastTime = Time.time;
            GrowDone();
        }

        private void FixedUpdate()
        {
            if (state == StateType.Growed)
            {
                if (Time.time - lastTime > insTime)
                {
                    GameObject Go = Instantiate(Fluff, transform.position + fluffOffset, Quaternion.identity);
                    lastTime = Time.time;
                    Go.GetComponent<FluffActable>().Init(direction, FluffLifeTime);
                }
            }
        }
    }
}