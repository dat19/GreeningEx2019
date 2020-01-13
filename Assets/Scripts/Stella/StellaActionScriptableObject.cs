using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Base", fileName = "StellaAction")]
    public class StellaActionScriptableObject : ScriptableObject
    {
        [Tooltip("下方向の段差を降りて歩けるかどうか。歩き系の動作はチェックします。")]
        public bool canStepDown = false;

        /// <summary>
        /// Groundタグ
        /// </summary>
        protected const string GroundTag = "Ground";
        protected const int HitMax = 8;
        protected RaycastHit[] hits = new RaycastHit[HitMax];
        /// <summary>
        /// MapCollisionレイヤーのGetMaskした値
        /// </summary>
        protected int groundLayer;
        /// <summary>
        /// MapCollision, Nae, MapTriggerの3レイヤーをGetMaskした値
        /// </summary>
        protected int overlapLayer;

        /// <summary>
        /// 動作を開始する時に必要な処理があったら、overrideして実装します。
        /// </summary>
        public virtual void Init()
        {
            groundLayer = LayerMask.GetMask("MapCollision");
            overlapLayer = LayerMask.GetMask("MapCollision", "Nae", "MapTrigger");
        }

        /// <summary>
        /// 更新処理です。
        /// </summary>
        public virtual void UpdateAction() { }

        /// <summary>
        /// カメラ直前の動作を実行する場合、このメソッドを上書きします。
        /// </summary>
        public virtual void LateUpdate() { }

        /// <summary>
        /// 終了時に必要な処理があったら、overrideして実装します。
        /// </summary>
        public virtual void End()
        {
            // Zを0にします。
            Vector3 pos = StellaMove.instance.transform.position;
            pos.z = 0f;
            StellaMove.instance.transform.position = pos;
        }

        /// <summary>
        /// 接触時の処理のうち、必要なものをoverrideします。
        /// </summary>
        /// <param name="other"></param>
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerStay(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }

        /// <summary>
        /// コライダーとの接触を処理したい場合はoverrideします。
        /// </summary>
        /// <param name="hit"></param>
        public virtual void OnControllerColliderHit(ControllerColliderHit hit) { }
    }
}