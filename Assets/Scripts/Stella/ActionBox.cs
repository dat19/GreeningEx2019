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
        /// 現在登録しているしているアクション可能なオブジェクトの数
        /// </summary>
        int actableCount = 0;

        /// <summary>
        /// 最寄りの行動候補のオブジェクト。
        /// </summary>
        Actable selectedActable = null;

        /// <summary>
        /// 最寄りのオブジェクトを検出したフレーム。同じフレームなら
        /// 前回検出したオブジェクトを返します。
        /// </summary>
        int detectFrame = -1;

        /// <summary>
        /// ローカルXのオフセット座標
        /// </summary>
        float offsetX;

        /// <summary>
        /// 現在選択中のオブジェクトを返します。
        /// ない場合はnullを返します。
        /// TODO: パフォーマンス上の問題が出てきたらキャッシュ
        /// </summary>
        public Actable SelectedActable
        {
            get
            {
                // 登録オブジェクトがない時はnull
                if (actableCount == 0)
                {
                    if (selectedActable != null)
                    {
                        selectedActable.Deselect();
                        selectedActable = null;
                    }

                    return null;
                }

                Actable lastAct = selectedActable;

                if (detectFrame != Time.frameCount)
                {
                    detectFrame = Time.frameCount;

                    // 最寄りオブジェクトを探索
                    float min = Mathf.Abs(actables[0].transform.position.x - transform.position.x);
                    selectedActable = actables[0];
                    for (int i=1;i<actableCount;i++)
                    {
                        float dist = Mathf.Abs(actables[i].transform.position.x - transform.position.x);
                        if (dist < min)
                        {
                            min = dist;
                            selectedActable = actables[i];
                        }
                    }
                }

                // 結果を返す
                if (lastAct != selectedActable)
                {
                    lastAct.Deselect();
                    selectedActable.Select();
                }
                return selectedActable;
            }
        }

        void Start()
        {
            Init();
            offsetX = Mathf.Abs(transform.localPosition.x);
        }

        public void UpdateSide()
        {
            Vector3 pos = transform.localPosition;
            pos.x = offsetX * StellaMove.forwardVector.x;
            transform.localPosition = pos;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"trigger {name}");
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Debug.Log($"  Controller Collider");            
        }

        public void Trigger(Collider other)
        {
            Debug.Log($" tri");

            Actable act = other.GetComponent<Actable>();
            if (act == null) return;

            for (int i = 0; i < actableCount; i++)
            {
                // 登録済みなら不要
                if (actables[i] == act)
                {
                    return;
                }
            }

            if (actableCount >= ActableMax)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"ActionBox.ActableMaxが{ActableMax}では足りません。");
#endif
                return;
            }

            actables[actableCount] = act;
            actableCount++;

            Log($"  Add Actable {actableCount}");
        }

        public void TriggerExit(Collider other)
        {
            Actable act = other.GetComponent<Actable>();
            if (act == null) return;

            for (int i=0;i<actableCount;i++)
            {
                act.Deselect();
                if (actables[i] == act)
                {
                    actables[i] = actables[actableCount - 1];
                    actableCount--;
                    detectFrame = -1;
                    return;
                }
            }
        }

        public void Init()
        {
            actableCount = 0;
            selectedActable = null;
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}