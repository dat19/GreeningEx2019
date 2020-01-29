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
            // X速度は歩き速度に入れる
            StellaMove.myVelocity.x = Mathf.Clamp(StellaMove.myVelocity.x, -StellaMove.MoveSpeed, StellaMove.MoveSpeed);
        }

        public override void UpdateAction()
        {
            // つた掴みチェック
            if (StellaMove.CheckIvyHold())
            {
                return;
            }

            // 行動ボタンチェック。着地時は何もしない
            if (Input.GetButton("Action") && !isLanding)
            {
                Actable act = StellaMove.ActionBoxInstance.GetActableInstance();
                if (act != null)
                {
                    if (act.Action())
                    {
                        return;
                    }
                }
            }

            StellaMove.instance.Gravity();
            StellaMove.instance.Move();

            // 着地チェック
            bool isGrounded = StellaMove.ChrController.isGrounded;
            if (!isGrounded)
            {
                int hitCount = PhysicsCaster.CharacterControllerCast(
                    StellaMove.ChrController,
                    Vector3.down,
                    StellaMove.CollisionMargin,
                    PhysicsCaster.MapCollisionPlayerOnlyLayer);
                isGrounded = hitCount > 0;
            }

            // 着地チェック
            if (!isLanding && isGrounded && StellaMove.myVelocity.y < 0f)
            {
                SoundController.Play(SoundController.SeType.Landing);

                StellaMove.myVelocity.x = 0;
                StellaMove.RegisterAnimEvent(Grounded);
                isLanding = true;
                StellaMove.CheckStepOn();
            }
            // 頭ぶつけチェック
            else if ((StellaMove.myVelocity.y > 0f) && StellaMove.ChrController.collisionFlags.HasFlag(CollisionFlags.Above))
            {
                SoundController.Play(SoundController.SeType.HitHead);
                StellaMove.myVelocity.y = 0f;
            }
        }

        void Grounded()
        {
            isLanding = false;
            StellaMove.instance.ChangeToWalk();
        }
    }
}
