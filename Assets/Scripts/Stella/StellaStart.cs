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
        Vector3 fallVelocity = new Vector3(1, -3f, 0);
        [Tooltip("飛び降りる高さ"), SerializeField]
        float fallHeight = 1.5f;
        [Tooltip("開始時の高さ"), SerializeField]
        float startHeight = 5f;
        [Tooltip("わたげプレハブ"), SerializeField]
        GameObject fluffPrefab = null;

        /// <summary>
        /// アニメの完了待ちのための待ちフレーム数
        /// </summary>
        const int startSkipFrame = 2;

        Vector3 targetPosition;
        FluffActable fluffActable = null;
        int startFrame;

        public override void Init()
        {
            base.Init();

            // 着地目標の座標を記録
            targetPosition = StellaMove.instance.transform.position;
            targetPosition.y += fallHeight;

            // 開始位置を移動させる
            float mul = (startHeight - fallHeight) / fallVelocity.y;
            Vector3 spos = targetPosition;
            spos += (mul * fallVelocity);
            StellaMove.instance.transform.position = spos;
            StellaMove.SetAnimTrigger("StartDandelion");

            // 綿毛の位置
            GameObject go = Instantiate(fluffPrefab);
            go.GetComponent<Animator>().SetTrigger("Spawned");
            fluffActable = go.GetComponent<FluffActable>();
            fluffActable.Init(fallVelocity, -mul * 2f);
            fluffActable.SetPositionAndHold(StellaMove.instance.transform.position);

            // アニメ設定
            StellaMove.SetAnimState(StellaMove.AnimType.Dandelion);
            StellaMove.myVelocity = Vector3.zero;

            startFrame = startSkipFrame;
        }

        public override void UpdateAction()
        {
            startFrame--;
            if (startFrame >= 0) return;

            // 手の位置と綿毛の位置に合わせて移動
            Vector3 next = fluffActable.HoldPosition;
            next -= (StellaMove.HoldPosition - StellaMove.instance.transform.position);
            Vector3 move = next - StellaMove.instance.transform.position;

            // 移動
            CollisionFlags flags = StellaMove.ChrController.Move(move);

            // 到着しているか
            Vector3 dir = targetPosition - StellaMove.instance.transform.position;
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
