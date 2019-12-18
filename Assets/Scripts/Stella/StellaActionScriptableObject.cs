using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Base", fileName = "StellaAction")]
    public class StellaActionScriptableObject : ScriptableObject
    {
        /// <summary>
        /// 動作を開始する時に必要な処理があったら、overrideして実装します。
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// 更新処理です。
        /// </summary>
        public virtual void UpdateAction() { }

        /// <summary>
        /// 終了時に必要な処理があったら、overrideして実装します。
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// 接触時の処理のうち、必要なものをoverrideします。
        /// </summary>
        /// <param name="other"></param>
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerStay(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }

        public virtual void OnCollisionEnter(Collision col) { }
        public virtual void OnCollisionStay(Collision col) { }
        public virtual void OnCollisionExit(Collision col) { }

    }
}