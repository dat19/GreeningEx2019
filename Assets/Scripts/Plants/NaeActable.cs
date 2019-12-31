using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class NaeActable : Actable
    {
        [Tooltip("手のピボットからのオフセット座標"), SerializeField]
        Vector3 offset = new Vector3(0.08f, 0.08f);
        [Tooltip("マーカー状態にする際のα値"), SerializeField]
        float markerAlpha = 0.25f;

        /// <summary>
        /// 持ち上げられている時、trueにします。
        /// </summary>
        bool isHolding = false;

        /// <summary>
        /// オブジェクトをくっつける基準のトランスフォーム
        /// </summary>
        Transform parentPivot;

        Collider myCollider = null;

        /// <summary>
        /// マーカー用のオブジェクト。Hold()で作成します。
        /// </summary>
        public static GameObject MarkerObject { get; private set; }

        /// <summary>
        /// コライダーの幅の半分を返します。
        /// </summary>
        public float ColliderExtentsX { get; private set; }

        private void Awake()
        {
            CanAction = true;
            isHolding = false;
            myCollider = GetComponent<Collider>();
            if (myCollider is SphereCollider)
            {
                ColliderExtentsX = ((SphereCollider)myCollider).radius;
            }
            else if (myCollider is CapsuleCollider)
            {
                ColliderExtentsX = ((CapsuleCollider)myCollider).radius;
            }
            else
            {
                ColliderExtentsX = myCollider.bounds.extents.x;
            }
        }

        public override void Action()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.LifetUp);
        }

        /// <summary>
        /// ステラが掴んだ時に呼び出します。
        /// </summary>
        public void Hold(Transform pivot)
        {
            isHolding = true;
            parentPivot = pivot;
            myCollider.enabled = false;

            // マーカー用オブジェクトを作成
            if (MarkerObject != null)
            {
                Destroy(MarkerObject);
            }

            MarkerObject = Instantiate(gameObject, transform.position, transform.rotation);
            // 当たり判定を消す
            Collider[] cols = MarkerObject.GetComponentsInChildren<Collider>();
            for (int i=0; i<cols.Length;i++)
            {
                cols[i].enabled = false;
            }
            // 半透明にする
            Renderer[] renderers = MarkerObject.GetComponentsInChildren<Renderer>();
            for (int i=0;i<renderers.Length;i++)
            {
                Material mat = new Material(renderers[i].material);
                mat.SetInt("_Mode", 2);
                Color col = mat.color;
                col.a = markerAlpha;
                mat.color = col;
                renderers[i].material = mat;
            }
        }

        private void LateUpdate()
        {
            if (isHolding)
            {
                transform.position = parentPivot.position + offset;
                transform.forward = Vector3.forward;
                transform.up = Vector3.up;
            }
        }

    }
}