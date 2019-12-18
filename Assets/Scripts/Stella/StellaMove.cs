using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    /// <summary>
    /// ステラを制御するためのスクリプト。
    /// このスクリプトには以下を実装します。
    /// <list type="bullet">
    /// <item>ステラの状態管理</item>
    /// <item>各状態のためのスクリプタブルオブジェクトの登録と呼び出し</item>
    /// <item>MonoBehaviourを制御するためのサービスメソッド</item>
    /// </list>
    /// </summary>
    public class StellaMove : MonoBehaviour
    {
        public static StellaMove instance = null;

        [Tooltip("移動速度(秒速)"), SerializeField]
        float moveSpeed = 3f;
        [Tooltip("重力加速度(速度/秒)"), SerializeField]
        float gravityAdd = 20f;
        [Tooltip("ステラの横向きの角度"), SerializeField]
        float rotateY = 40f;
        [Tooltip("ステラの各状態を制御するためのStellaActionアセット。Create > Greening > StellaActionsから、必要なものを作成"), SerializeField]
        StellaActionScriptableObject[] stellaActionScriptableObjects = null;
        [Tooltip("歩く際のデフォルトの落下距離"), SerializeField]
        float walkDownY = 0.1f;
        [Tooltip("ミニジャンプで乗れる高さ"), SerializeField]
        float miniJumpHeight = 1f;
        [Tooltip("ミニジャンプ時に、余分にジャンプする高さ"), SerializeField]
        float miniJumpMargin = 0.25f;

        /// <summary>
        /// アニメのStateに設定する値
        /// </summary>
        public enum AnimType
        {
            Start,
            Walk,
            Jump,       // 2ジャンプ開始
            Air,        // 3空中
            OnGround,   // 4着地
        }

        /// <summary>
        /// ステラの行動定義
        /// </summary>
        public enum ActionType
        {
            Start,  // 開始時の演出
            Walk,   // 立ち、歩き
            Air,    // 落下、着地
            Jump,   // ジャンプまでのアニメ
            Water,  // 水まき
            Nae,    // 苗運び
        }

        /// <summary>
        /// 列挙する接触したオブジェクトの上限数
        /// </summary>
        const int CollisionMax = 8;
        /// <summary>
        /// 移動先を判定する厚さの半分
        /// </summary>
        Vector3 boxColliderHalfExtents = new Vector3(0.05f, 0, 1);

        public static CharacterController chrController { get; private set; }
        public static Vector3 myVelocity = Vector3.zero;
        static Animator anim;
        static ActionType nowAction;
        static RaycastHit[] raycastHits = new RaycastHit[CollisionMax];
        static LayerMask mapCollisionLayerMask;
        static Vector3 checkCenter;

        void Awake()
        {
            instance = this;
            chrController = GetComponent<CharacterController>();
            anim = GetComponentInChildren<Animator>();
            anim.SetInteger("State", (int)AnimType.Walk);
            nowAction = ActionType.Walk;
            mapCollisionLayerMask = LayerMask.GetMask("MapCollision");
            boxColliderHalfExtents.y = chrController.height * 0.5f;
        }

        void FixedUpdate()
        {
            if (!StageManager.CanMove) return;

            stellaActionScriptableObjects[(int)nowAction]?.UpdateAction(Time.fixedDeltaTime);
        }

        /// <summary>
        /// 設定した動きをキャラクターコントローラーに反映。
        /// </summary>
        public void Move()
        {
            // 設定された速度を反映
            anim.SetFloat("VelX", Mathf.Abs(myVelocity.x));
            anim.SetFloat("VelY", myVelocity.y);

            Vector3 move = myVelocity * Time.fixedDeltaTime;
            if (chrController.isGrounded)
            {
                move.y -= walkDownY;
            }
            chrController.Move(move);
        }

        /// <summary>
        /// 歩いたり止まったりします。
        /// </summary>
        public void Walk()
        {
            // キーの入力を調べる
            float h = Input.GetAxisRaw("Horizontal");

            // 左右の移動速度(秒速)を求める
            float vx = h * moveSpeed;

            // 動かす
            myVelocity.x = vx;

            Vector3 e = transform.eulerAngles;
            if (h < -0.5f)
            {
                e.y = rotateY;
            }
            else if (h > 0.5f)
            {
                e.y = -rotateY;
            }
            transform.eulerAngles = e;
        }

        /// <summary>
        /// 重力加速を処理します。
        /// </summary>
        public void Gravity()
        {
            // 落下
            if (chrController.isGrounded)
            {
                myVelocity.y = 0f;
            }

            myVelocity.y += -gravityAdd * Time.fixedDeltaTime;
        }

        /// <summary>
        /// 移動先の高さが1かどうかを確認して、必要ならミニジャンプします。
        /// </summary>
        public void CheckMiniJump()
        {
            // 横にぶつかっていなければチェック不要
            if ((chrController.collisionFlags & CollisionFlags.Sides) == 0) return;

            // ぶつかった相手を調べる
            Vector3 dir = (transform.eulerAngles.y < 0 || transform.eulerAngles.y > 180) ? Vector3.right : Vector3.left;
            checkCenter = transform.position
                + chrController.center
                + dir * (chrController.radius + boxColliderHalfExtents.x);

            int hitCount = Physics.BoxCastNonAlloc(checkCenter, boxColliderHalfExtents, dir, raycastHits, Quaternion.identity, boxColliderHalfExtents.x, mapCollisionLayerMask);
            if (hitCount == 0) return;

            float footh = chrController.bounds.min.y;
            float h = raycastHits[0].collider.bounds.max.y - footh;

            for (int i = 1; i < hitCount; i++)
            {
                h = Mathf.Max(h, raycastHits[i].collider.bounds.max.y - footh);
            }
            if (h <= miniJumpHeight)
            {
                JumpToGround((transform.position + dir) + Vector3.up * miniJumpHeight);
            }
        }

        /// <summary>
        /// ジャンプで着地したい地面の座標
        /// </summary>
        static Vector3 targetJumpGround = Vector3.zero;

        /// <summary>
        /// 目的の床の座標を指定して、そこに向けてジャンプ
        /// </summary>
        /// <param name="target">目的の足場</param>
        public void JumpToGround(Vector3 target)
        {
            targetJumpGround = target;
            ChangeAction(ActionType.Jump);
            SetAnimState(AnimType.Jump);
        }

        /// <summary>
        /// targetJumpGroundから、ジャンプの初速を設定します。
        /// 移動速度は、moveSpeedを一先ず利用します。
        /// </summary>
        public void SetFirstVelocityY()
        {
            chrController.detectCollisions = false;     // 当たり判定を一時無効化
            float targetY = targetJumpGround.y + miniJumpMargin;

        }

        /// <summary>
        /// アニメーターのStateを指定の状態にします。
        /// </summary>
        /// <param name="type">StellaMove.AnimTypeで指定</param>
        public void SetAnimState(AnimType type)
        {
            anim.SetInteger("State", (int)type);
        }

        /// <summary>
        /// 違う行動に切り替えます。
        /// </summary>
        /// <param name="type">StellaMove.ActionTypeで次の動作を指定します。</param>
        public void ChangeAction(ActionType type)
        {
            int idx = (int)type;
            stellaActionScriptableObjects[idx].End();
            nowAction = type;
            stellaActionScriptableObjects[idx].Init();
        }
    }
}