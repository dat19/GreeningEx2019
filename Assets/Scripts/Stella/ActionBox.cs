#define DEBUG_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// アクション対象のオブジェクトを保有しておくためのクラス。
    /// ステラの子供のActionBoxオブジェクトにアタッチします。
    /// </summary>
    public class ActionBox : MonoBehaviour
    {
        /// <summary>
        /// アクション可能なオブジェクトを保有する最大値です。
        /// この上限に達しない数を設定しておきます。
        /// </summary>
        const int ActableMax = 10;

        /// <summary>
        /// 登録中のアクション可能なインスタンス
        /// </summary>
        Actable[] actables = new Actable[ActableMax];

        /// <summary>
        /// 前回チェック時のアクション可能なインスタンス
        /// </summary>
        Actable[] lastActables = new Actable[ActableMax];

        /// <summary>
        /// 現在登録しているしているアクション可能なオブジェクトの数
        /// </summary>
        int actableCount = 0;

        /// <summary>
        /// 最寄りの行動候補のオブジェクト。
        /// </summary>
        public static Actable SelectedActable { get; private set; }

        /// <summary>
        /// コライダーの中心座標
        /// </summary>
        public Vector3 colliderCenter { get; private set; }

        /// <summary>
        /// 半分のサイズ
        /// </summary>
        public Vector3 halfExtents { get; private set; }

        /// <summary>
        /// 検出用のワーキング配列
        /// </summary>
        RaycastHit []hits = new RaycastHit[ActableMax];

        /// <summary>
        /// チェックするレイヤー
        /// </summary>
        int layerMask;

        void Start()
        {
            Init();

            layerMask = LayerMask.GetMask("Nae", "MapCollision", "MapTrigger");
            BoxCollider col = GetComponent<BoxCollider>();
            colliderCenter = col.center;
            halfExtents = col.size * 0.5f;
            SelectedActable = null;

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 最寄りのアクション可能なオブジェクトのインスタンスを返します。
        /// </summary>
        /// <returns>有効なオブジェクトがあればnull以外のインスタンスを返します。</returns>
        public Actable GetActableInstance()
        {
            Vector3 ofs = colliderCenter;
            ofs.x *= StellaMove.forwardVector.x;
            Vector3 center = StellaMove.instance.transform.position + ofs;

            int hitCount = (Physics.BoxCastNonAlloc(center, halfExtents, StellaMove.forwardVector, hits, Quaternion.identity, 0f, layerMask));

            int lastActableCount = actableCount;
            for (int i=0; i<lastActableCount;i++)
            {
                lastActables[i] = actables[i];
            }

            // 列挙するものがなければこの場でnullを返す
            if (hitCount == 0)
            {
                if (SelectedActable != null)
                {
                    SelectedActable.Deselect();
                }
                SelectedActable = null;
                return null;
            }

            // 今回のものを列挙
            float min = float.PositiveInfinity;
            Actable nextSelect = null;
            for (int i=0; i<hitCount;i++)
            {
                Actable hitAct = hits[i].collider.GetComponent<Actable>();
                if (hitAct == null || !hitAct.CanAction) continue;

                // 距離の更新確認
                float dist = Mathf.Abs(transform.position.x - hits[i].transform.position.x);
                if (dist < min)
                {
                    min = dist;
                    nextSelect = hitAct;
                }
            }

            if (SelectedActable != nextSelect)
            {
                if (SelectedActable != null)
                {
                    SelectedActable.Deselect();
                }
                if (nextSelect != null)
                {
                    nextSelect.Select();
                }
            }
            SelectedActable = nextSelect;

            return SelectedActable;
        }

        public void Init()
        {
            actableCount = 0;
            SelectedActable = null;
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}