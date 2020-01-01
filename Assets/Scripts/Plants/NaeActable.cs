using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GreeningEx2019
{
    /// <summary>
    /// オブジェクトを列挙するためのメソッド用のデリゲート定義
    /// </summary>
    /// <param name="pos">チェックする座標</param>
    /// <param name="hits">列挙したオブジェクトの戻り値</param>
    /// <param name="layer">対象のレイヤー</param>
    /// <returns>見つけたオブジェクト数</returns>
    public delegate int FetchObjects(Vector3 pos, RaycastHit[] hits, int layer);

    public class NaeActable : Actable
    {
        [Tooltip("手のピボットからのオフセット座標"), SerializeField]
        Vector3 offset = new Vector3(0.08f, 0.08f);
        [Tooltip("マーカー状態にする際のα値"), SerializeField]
        float markerAlpha = 0.25f;
        [Tooltip("マーカーの地面からの高さ"), SerializeField]
        float heightFromGround = 0.5f;

        /// <summary>
        /// 持ち上げられている時、trueにします。
        /// </summary>
        bool isHolding = false;

        /// <summary>
        /// 重なっているオブジェクトを検出するためのメソッド。保持しているコライダーに応じて登録するメソッドを切り替えます。
        /// </summary>
        public FetchObjects FetchOverlapObjects;

        /// <summary>
        /// オブジェクトをくっつける基準のトランスフォーム
        /// </summary>
        Transform parentPivot;

        Collider myCollider = null;
        Animator anim = null;

        /// <summary>
        /// マーカー用のオブジェクト。Hold()で作成します。
        /// </summary>
        public static GameObject MarkerObject { get; private set; }

        /// <summary>
        /// コライダーの幅の半分を返します。
        /// </summary>
        public float ColliderExtentsX { get; private set; }
        public float HeightFromGround { get { return heightFromGround; } }

        BoxCollider boxCollider = null;
        SphereCollider sphereCollider = null;
        CapsuleCollider capsuleCollider = null;

        private void Awake()
        {
            CanAction = true;
            isHolding = false;
            myCollider = GetComponent<Collider>();
            anim = GetComponent<Animator>();
            if (myCollider is SphereCollider)
            {
                sphereCollider = ((SphereCollider)myCollider);
                ColliderExtentsX = sphereCollider.radius;
                FetchOverlapObjects = FetchOverlapObjectsWithSphereCollider;
            }
            else if (myCollider is CapsuleCollider)
            {
                capsuleCollider = ((CapsuleCollider)myCollider);
                ColliderExtentsX = capsuleCollider.radius;
                FetchOverlapObjects = FetchOverlapObjectsWithCapsuleCollider;
            }
            else
            {
                boxCollider = ((BoxCollider)myCollider);
                ColliderExtentsX = myCollider.bounds.extents.x;
                FetchOverlapObjects = FetchOverlapObjectsWithBoxCollider;
            }
        }

        /// <summary>
        /// ボックスコライダーのオブジェクトと重なっているオブジェクトを列挙して返します。
        /// </summary>
        /// <param name="pos">左右中央、下の座標</param>
        /// <param name="hits">結果を返すための配列</param>
        /// <param name="layer">対象のレイヤー</param>
        /// <returns>見つけたオブジェクト数</returns>
        int FetchOverlapObjectsWithBoxCollider(Vector3 pos, RaycastHit[] hits, int layer)
        {
            Vector3 center = pos + Vector3.up * heightFromGround + boxCollider.center;
            return Physics.BoxCastNonAlloc(center, myCollider.bounds.extents, Vector3.down, hits, Quaternion.identity, 0f, layer);
        }

        /// <summary>
        /// スフィアコライダーのオブジェクトと重なっているオブジェクトを列挙して返します。
        /// </summary>
        /// <param name="pos">左右中央、下の座標</param>
        /// <param name="hits">結果を返すための配列</param>
        /// <param name="layer">対象のレイヤー</param>
        /// <returns>見つけたオブジェクト数</returns>
        int FetchOverlapObjectsWithSphereCollider(Vector3 pos, RaycastHit[] hits, int layer)
        {
            Vector3 center = pos + Vector3.up * heightFromGround + sphereCollider.center;
            return Physics.SphereCastNonAlloc(center, sphereCollider.radius, Vector3.down, hits, 0f, layer);
        }

        /// <summary>
        /// カプセルコライダーのオブジェクトと重なっているオブジェクトを列挙して返します。
        /// </summary>
        /// <param name="pos">左右中央、下の座標</param>
        /// <param name="hits">結果を返すための配列</param>
        /// <param name="layer">対象のレイヤー</param>
        /// <returns>見つけたオブジェクト数</returns>
        int FetchOverlapObjectsWithCapsuleCollider(Vector3 pos, RaycastHit[] hits, int layer)
        {
            Vector3 center = pos + Vector3.up * heightFromGround + capsuleCollider.center;
            float offsetY = Mathf.Max(capsuleCollider.height * 0.5f - capsuleCollider.radius, 0f);
            return Physics.CapsuleCastNonAlloc(center + Vector3.up * offsetY, center + Vector3.down * offsetY, capsuleCollider.radius, Vector3.down, hits, 0f, layer);
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
            anim.enabled = false;
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