using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Watering", fileName = "StellaActionWatering")]
    public class StellaWatering : StellaActionScriptableObject
    {
        enum StateType
        {
            Start,
            Action,
            End
        }

        StateType state;

        public override void Init()
        {
            state = StateType.Start;
            StellaMove.instance.SetAnimState(StellaMove.AnimType.Water);

            // アニメからイベントが呼ばれた時に、StartAction()を実行するように登録する
            StellaMove.RegisterAnimEvent(StartAction);

            // ステラの横移動を止めておく
            StellaMove.myVelocity.x = 0;
        }
        
        void StartAction()
        {
            state = StateType.Action;
        }

        public override void UpdateAction()
        {
            if (state == StateType.Action)
            {
                if (!Input.GetButton("Water"))
                {
                    // 水まき終了
                    state = StateType.End;
                    StellaMove.instance.SetAnimState(StellaMove.AnimType.Walk);
                    StellaMove.RegisterAnimEvent(EndAction);
                }
            }
        }

        // 水まきアニメが終わった処理
        void EndAction()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
        }
    }
}