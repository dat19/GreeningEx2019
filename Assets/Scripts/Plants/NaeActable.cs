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
    /// <param name="layer">対象のレイヤー</param>
    /// <returns>見つけたオブジェクト数</returns>
    public delegate int FetchObjects(Vector3 pos, int layer);

    /// <summary>
    /// 苗として持ち上げられる処理のためのスクリプト。
    /// タグがNaeの時のみ発動します。成長したら、タグをGrowに変えます。
    /// </summary>
    public class NaeActable : Actable
    {
        [Tooltip("この苗の種類"), SerializeField]
        NaeType type = NaeType.FlowerBridge;
        [Tooltip("手のピボットからのオフセット座標"), SerializeField]
        Vector3 offset = new Vector3(0.08f, 0.08f);
        [Tooltip("マーカーの地面からの高さ"), SerializeField]
        float heightFromGround = 0.5f;
        [Tooltip("苗マーカーなどのデータ"), SerializeField]
        NaeMarkerData naeMarkerData = null;

        /// <summary>
        /// 苗の種類
        /// </summary>
        public enum NaeType
        {
            FlowerBridge,
            Ivy,
            Dandelion,
            Mushroom,
            Rock
        }

        /// <summary>
        /// 持ち上げられている時、trueにします。
        /// </summary>
        bool isHolding = false;

        /// <summary>
        /// 重なっているオブジェクトを検出するためのメソッド。保持しているコライダーに応じて登録するメソッドを切り替えます。
        /// </summary>
        public FetchObjects FetchOverlapObjects;

        /// <summary>
        /// マーカー用のオブジェクト。Hold()で作成します。
        /// </summary>
        public static GameObject MarkerObject
        {
            get
            {
                return NaeMarkerData.markerObjects[(int)selectedType];
            }
        }

        /// <summary>
        /// 苗を持つオフセットX座標
        /// </summary>
        public float NaeOffsetX
        {
            get
            {
                return offset.x;
            }
        }

        /// <summary>
        /// コライダーの幅の半分を返します。
        /// </summary>
        public float ColliderExtentsX { get; private set; }
        public float HeightFromGround { get { return heightFromGround; } }

        Collider myCollider = null;
        Animator anim = null;
        BoxCollider boxCollider = null;
        SphereCollider sphereCollider = null;
        CapsuleCollider capsuleCollider = null;
        static NaeType selectedType;

        /// <summary>
        /// 動作フラグ。苗の時で着地のみ有効
        /// </summary>
        public override bool CanAction
        {
            get
            {
                return (GrowInstance.state == Grow.StateType.Nae)
                    && (StellaMove.ChrController.isGrounded);
            }
            protected set => base.CanAction = value;
        }

        private void Awake()
        {
            CanAction = true;
            isHolding = false;
            anim = GetComponent<Animator>();
            if (anim == null)
            {
                anim = GetComponentInChildren<Animator>();
            }

            Collider[] cols = GetComponents<Collider>();
            for (int i=0;i<cols.Length;i++)
            {
                if (cols[i].enabled)
                {
                    myCollider = GetComponent<Collider>();
                    break;
                }
            }
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
            naeMarkerData.Init();
        }

        /// <summary>
        /// ボックスコライダーのオブジェクトと重なっているオブジェクトを列挙して返します。
        /// </summary>
        /// <param name="pos">左右中央、下の座標</param>
        /// <param name="hits">結果を返すための配列</param>
        /// <param name="layer">対象のレイヤー</param>
        /// <returns>見つけたオブジェクト数</returns>
        int FetchOverlapObjectsWithBoxCollider(Vector3 pos, int layer)
        {
            Vector3 center = pos + Vector3.up * heightFromGround + boxCollider.center;
            return PhysicsCaster.BoxCast(center, myCollider.bounds.extents, Vector3.down, 0f, layer);
        }

        /// <summary>
        /// スフィアコライダーのオブジェクトと重なっているオブジェクトを列挙して返します。
        /// </summary>
        /// <param name="pos">左右中央、下の座標</param>
        /// <param name="layer">対象のレイヤー</param>
        /// <returns>見つけたオブジェクト数</returns>
        int FetchOverlapObjectsWithSphereCollider(Vector3 pos, int layer)
        {
            Vector3 center = pos + Vector3.up * heightFromGround + sphereCollider.center;
            return PhysicsCaster.SphereCast(center, sphereCollider.radius, Vector3.down, 0f, layer);
        }

        /// <summary>
        /// カプセルコライダーのオブジェクトと重なっているオブジェクトを列挙して返します。
        /// </summary>
        /// <param name="pos">左右中央、下の座標</param>
        /// <param name="layer">対象のレイヤー</param>
        /// <returns>見つけたオブジェクト数</returns>
        int FetchOverlapObjectsWithCapsuleCollider(Vector3 pos, int layer)
        {
            Vector3 center = pos + Vector3.up * heightFromGround + capsuleCollider.center;
            return PhysicsCaster.CapsuleCast(center, capsuleCollider, Vector3.down, 0f, layer);
        }

        public override bool Action()
        {
            if (!CanAction) return false;

            StellaMove.naePutPosition = transform.position;
            anim.enabled = false;
            myCollider.enabled = false;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.LiftUp);
            selectedType = type;
            return true;
        }

        /// <summary>
        /// ステラが掴んだ時に呼び出します。
        /// </summary>
        public void Hold()
        {
            isHolding = true;
            SetCollider(false);
        }

        /// <summary>
        /// 苗をおろします。
        /// </summary>
        public void PutDown()
        {
            isHolding = false;
            transform.position = StellaMove.naePutPosition + Vector3.up * heightFromGround;

            // マーカー用オブジェクトを削除
            MarkerObject.SetActive(false);
        }

        /// <summary>
        /// コライダーとアニメの有効無効を設定します。
        /// </summary>
        /// <param name="flag">コライダーとアニメの状態</param>
        public void SetCollider(bool flag)
        {
            anim.enabled = flag;
            myCollider.enabled = flag;
        }

        /// <summary>
        /// 持っている時に位置を調整します。
        /// </summary>
        private void LateUpdate()
        {
            if (isHolding)
            {
                Vector3 forwardOffset = offset;
                forwardOffset.x *= StellaMove.forwardVector.x;
                transform.position = StellaMove.HoldPosition + forwardOffset;
                transform.forward = Vector3.forward;
                transform.up = Vector3.up;
            }
        }
    }
}