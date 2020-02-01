using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class GreenPower : MonoBehaviour
    {
        [Tooltip("最小サイズ"), SerializeField]
        float scaleMin = 0.15f;
        [Tooltip("最大サイズ"), SerializeField]
        float scaleMax = 0.25f;
        [Tooltip("発生時の最小速度"), SerializeField]
        float spawnVelocityMin = 3f;
        [Tooltip("発生時の最大速度"), SerializeField]
        float spawnVelocityMax = 6f;
        [Tooltip("最小角度"), SerializeField]
        float degreeMin = 30f;
        [Tooltip("最大角度"), SerializeField]
        float degreeMax = 150f;
        [Tooltip("発生時の停止までの秒数"), SerializeField]
        Vector3 spawnDampingMin = new Vector3(0.75f, 1f, 0f);
        [Tooltip("発生時の停止までの秒数"), SerializeField]
        Vector3 spawnDampingMax = new Vector3(1f, 1.25f, 0f);
        [Tooltip("星に飛び始めるまでの最小秒数"), SerializeField]
        float toStarMin = 0.75f;
        [Tooltip("星に飛び始めるまでの最大秒数"), SerializeField]
        float toStarMax = 1.25f;
        [Tooltip("星に飛ぶまでの最小加速度"), SerializeField]
        float addMin = 1f;
        [Tooltip("星に飛ぶまでの最大加速度"), SerializeField]
        float addMax = 2f;

        Vector3 spawnVelocity = Vector3.zero;
        Vector3 spawnDamping = Vector3.zero;
        Vector3 toStarVelocity = Vector3.zero;
        float toStarSeconds;
        float add;
        float seconds;
        float scale;

        private void Awake()
        {
            scale = Random.Range(scaleMin, scaleMax);
            float speed = Random.Range(spawnVelocityMin, spawnVelocityMax);
            float th = Random.Range(degreeMin, degreeMax) * Mathf.Deg2Rad;
            spawnVelocity.Set(Mathf.Cos(th) * speed, Mathf.Sin(th) * speed, 0f);
            spawnDamping.Set(
                Random.Range(spawnDampingMin.x, spawnDampingMax.x),
                Random.Range(spawnDampingMin.y, spawnDampingMax.y), 0f);
            toStarVelocity = Vector3.zero;
            toStarSeconds = Random.Range(toStarMin, toStarMax);
            add = Random.Range(addMin, addMax);
            seconds = 0f;
        }

        void FixedUpdate()
        {
            seconds += Time.fixedDeltaTime;

            // 発生時速度
            spawnVelocity.x = Mathf.Lerp(spawnVelocity.x, 0f, seconds / spawnDamping.x);
            spawnVelocity.y = Mathf.Lerp(spawnVelocity.y, 0f, seconds / spawnDamping.y);

            float t = Mathf.Clamp01(seconds / toStarSeconds);
            transform.GetChild(0).transform.localScale = (t * scale) * Vector3.one;

            // その後の加速
            if (seconds >= toStarSeconds)
            {
                Vector3 to = Goal.instance.transform.position - transform.position;
                if (to.magnitude < toStarVelocity.magnitude * Time.fixedDeltaTime) {
                    // 到達
                    Destroy(gameObject);
                    Goal.IncrementGreenPower();
                    return;
                }

                // 加速
                toStarVelocity += to.normalized * add * Time.fixedDeltaTime;
            }

            // 移動
            transform.Translate((spawnVelocity + toStarVelocity) * Time.fixedDeltaTime);
        }
    }
}