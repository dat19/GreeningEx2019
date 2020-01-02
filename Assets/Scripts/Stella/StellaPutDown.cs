using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create PutDown", fileName = "StellaActionPutDown")]
    public class StellaPutDown : StellaLiftUp
    {
        protected override void ToAction()
        {
            StellaMove.RegisterAnimEvent(PutDownNae);
            StellaMove.RegisterAnimEvent(NaeOff);
            StellaMove.SetAnimState(StellaMove.AnimType.PutDown);
        }

        void NaeOff()
        {
            StellaMove.SetAnimBool("Nae", false);
            StellaMove.hasNae = false;
            StellaMove.RegisterAnimEvent(PutDownNae);
        }

        void PutDownNae()
        {
            StellaMove.RegisterAnimEvent(ToWalk);
            ((NaeActable)ActionBox.SelectedActable).PutDown();
        }

        void ToWalk()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
        }
    }
}
