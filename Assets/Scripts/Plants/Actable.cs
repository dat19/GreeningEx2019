using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// アクションキーで動作する対象のオブジェクトは、このクラスを継承して実装します。
    /// </summary>
    [RequireComponent(typeof(Grow))]
    public abstract class Actable : MonoBehaviour
    {
        Grow grow = null;
        protected Grow GrowInstance
        {
            get
            {
                if (grow == null)
                {
                    grow = GetComponent<Grow>();
                }
                return grow;
            }
        }

        /// <summary>
        /// 行動可能な時、trueを返します。
        /// </summary>
        public virtual bool CanAction { get; protected set; }

        /// <summary>
        /// 行動を実行します。
        /// </summary>
        public abstract void Action();

        /// <summary>
        /// 押す時に発動する動作があればこれを上書きします。
        /// </summary>
        /// <returns>動作後にステラを下がらせたい場合、trueを返します。</returns>
        public virtual bool PushAction() { return false; }

        /// <summary>
        /// 選択された時に呼び出します。
        /// </summary>
        public virtual void Select() { }

        /// <summary>
        /// 選択を解除します。
        /// </summary>
        public virtual void Deselect() { }
    }
}