using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// 苗の上げ下ろしアクションの親クラス。苗との距離を作る処理を共通化します。
    /// </summary>
    public abstract class StellaLiftUpPutDownBase : StellaActionScriptableObject
    {
        protected enum StateType
        {
            TargetWalk,
            Action,
            BackOff
        }

        protected StateType state;
        protected float targetX;

        /// <summary>
        /// 目的地を設定
        /// </summary>
        public override void Init()
        {
            base.Init();
        }

        public override void UpdateAction()
        {
            switch (state)
            {
                case StateType.TargetWalk:
                    StellaMove.AdjustWalkResult res = StellaMove.AdjustWalk(targetX, StellaMove.MoveSpeed);
                    if (res == StellaMove.AdjustWalkResult.Reach)
                    {
                        state = StateType.Action;
                        StellaMove.myVelocity = Vector3.zero;
                        ToAction();
                    }
                    else if (res == StellaMove.AdjustWalkResult.Abort)
                    {
                        // 移動できなければ歩きに戻します
                        StellaMove.myVelocity = Vector3.zero;
                        if (StellaMove.NowAction == StellaMove.ActionType.LiftUp)
                        {
                            ToWalk();
                        }
                        else
                        {
                            StellaMove.instance.ChangeAction(StellaMove.ActionType.NaeWalk);
                        }
                    }
                    break;
                case StateType.Action:
                    StellaMove.myVelocity.x = 0f;
                    StellaMove.instance.Move();
                    break;
            }
        }

        /// <summary>
        /// 動作を開始するメソッドです。オーバーライドして実装してください。
        /// </summary>
        protected abstract void ToAction();

        /// <summary>
        /// 歩きへ
        /// </summary>
        protected void ToWalk()
        {
            StellaMove.myVelocity.x = 0;
            StellaMove.naeActable.SetCollider(true);
            StellaMove.naeActable = null;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
        }
    }
}
