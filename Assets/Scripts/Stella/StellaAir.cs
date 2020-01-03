using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// ステラの空中制御。
    /// targetPositionのx座標に達するまでは、現在の移動を続けます。
    /// </summary>
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Air", fileName = "StellaActionAir")]
    public class StellaAir : StellaActionScriptableObject
    {
        /// <summary>
        /// 着地中
        /// </summary>
        bool isLanding = false;

        public override void Init()
        {
            base.Init();
            isLanding = false;
            StellaMove.SetAnimState(StellaMove.AnimType.Air);
        }

        public override void UpdateAction()
        {
            Debug.Log($"  b {StellaMove.myVelocity.y}");
            StellaMove.instance.Gravity();
            Debug.Log($"  m {StellaMove.myVelocity.y}");
            StellaMove.instance.Move();
            Debug.Log($"  a {StellaMove.myVelocity.y}");

            if (!isLanding && StellaMove.chrController.isGrounded && StellaMove.myVelocity.y < 0f)
            {
                StellaMove.myVelocity.x = 0;
                StellaMove.RegisterAnimEvent(Grounded);
                isLanding = true;
                int hcnt = StellaMove.GetUnderMap(hits);
                for (int i=0;i<hcnt;i++)
                {
                    IStepOn so = hits[i].collider.GetComponent<IStepOn>();
                    if( so != null)
                    {
                        so.StepOn();
                    }
                }
            }
        }

        void Grounded()
        {
            StellaMove.instance.ChangeAction(StellaMove.hasNae ? StellaMove.ActionType.NaeWalk : StellaMove.ActionType.Walk);
            isLanding = false;
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (!StageManager.CanMove) return;

            if (other.CompareTag("DeadZone"))
            {
                StellaMove.instance.ChangeAction(StellaMove.ActionType.Obore);
            }
        }
    }
}
