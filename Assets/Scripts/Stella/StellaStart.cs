using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// ステージ開始時にたんぽぽを持って降りてくる演出
    /// </summary>
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Start", fileName = "StellaActionStart")]
    public class StellaStart : StellaActionScriptableObject
    {
        [Tooltip("降りてくる時の速度"), SerializeField]
        Vector3 fallVelocity = new Vector3(1, -2f, 0);
        [Tooltip("飛び降りる高さ"), SerializeField]
        float fallHeight = 2f;
        [Tooltip("開始時の高さ"), SerializeField]
        float startHeight = 5f;
        [Tooltip("わたげプレハブ"), SerializeField]
        GameObject fluffPrefab = null;

        Vector3 targetPosition;
        FluffActable fluffActable = null;
        bool isStarted = false;

        public override void Init()
        {
            base.Init();

            Debug.Log($"started");

            // 着地目標の座標を記録
            targetPosition = StellaMove.instance.transform.position;

            // 開始位置を移動させる
            Vector3 spos = targetPosition;
            spos.y += fallHeight;
            float mul = (startHeight - fallHeight) / fallVelocity.y;
            spos -= (mul * fallVelocity);

            StellaMove.instance.transform.position = spos;
            Debug.Log($"  spos = {spos}");

            // 綿毛の位置
            GameObject go = Instantiate(fluffPrefab);
            fluffActable = go.GetComponent<FluffActable>();
            fluffActable.SetPositionAndHold(StellaMove.instance.transform.position);
            fluffActable.Init(fallVelocity, mul*2f);

            // アニメ設定
            StellaMove.SetAnimState(StellaMove.AnimType.Dandelion);
            StellaMove.myVelocity = Vector3.zero;

            isStarted = true;
        }

        public override void UpdateAction()
        {
            // 手の位置と綿毛の位置に合わせて移動
            Vector3 next = fluffActable.HoldPosition;
            next -= (StellaMove.HoldPosition - StellaMove.instance.transform.position);
            Vector3 move = next - StellaMove.instance.transform.position;

            // 移動
            CollisionFlags flags = StellaMove.ChrController.Move(move);

            // 到着しているか
            Vector3 dir = StellaMove.instance.transform.position - targetPosition;
            if (Vector3.Dot(dir, fallVelocity) < 0f)
            {
                // 逆方向なら到着している
                StellaMove.instance.transform.position = targetPosition;
                fluffActable.SetFall();
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Air);

            }
        }
    }
}
