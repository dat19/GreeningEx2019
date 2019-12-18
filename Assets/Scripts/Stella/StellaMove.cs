using UnityEngine;
using UnityEngine.Events;

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

        [Header("デバッグ")]
        [Tooltip("常に操作可能にしたい時、チェックします。"), SerializeField]
        bool isDebugMovable = false;

        /// <summary>
        /// ステラの行動定義
        /// </summary>
        public enum ActionType
        {
            None = -1,
            Start,  // 0開始時の演出
            Walk,   // 1立ち、歩き
            Air,    // 2落下、着地
            Jump,   // 3ジャンプまでのアニメ
            Water,  // 4水まき
            Nae,    // 5苗運び
        }

        /// <summary>
        /// アニメのStateに設定する値
        /// </summary>
        public enum AnimType
        {
            Start,      // 0ゲーム開始演出
            Walk,       // 1立ち、歩き
            Jump,       // 2ジャンプ開始
            Air,        // 3空中
            OnGround,   // 4着地
            Water,      // 5水まき
            Obore,      // 6溺れ
            Ivy,        // 7ツタ
            Dandelion,  // 8綿毛に捕まる
            Clear,      // 9クリア
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
        static Vector3 forwardVector = Vector3.right;
        static Animator anim;
        static ActionType nowAction = ActionType.None;
        static RaycastHit[] raycastHits = new RaycastHit[CollisionMax];
        static LayerMask mapCollisionLayerMask;
        static Vector3 checkCenter;
        static UnityAction animEventAction = null;
        static Vector3 targetJumpGround = Vector3.zero;
        static int defaultLayer = 0;
        static int jumpLayer = 0;

        void Awake()
        {
            instance = this;
            chrController = GetComponent<CharacterController>();
            anim = GetComponentInChildren<Animator>();
            anim.SetInteger("State", (int)AnimType.Walk);
            nowAction = ActionType.Walk;
            mapCollisionLayerMask = LayerMask.GetMask("MapCollision");
            boxColliderHalfExtents.y = chrController.height * 0.5f;
            defaultLayer = LayerMask.NameToLayer("Player");
            jumpLayer = LayerMask.NameToLayer("Jump");
        }

        void FixedUpdate()
        {
#if UNITY_EDITOR
            if (!isDebugMovable)
            {
#endif
                if (!StageManager.CanMove) return;
#if UNITY_EDITOR
            }
#endif

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
                forwardVector.x = -1;
            }
            else if (h > 0.5f)
            {
                e.y = -rotateY;
                forwardVector.x = 1;
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
            if (myVelocity.y < 0)
            {
                gameObject.layer = defaultLayer;
            }
        }

        /// <summary>
        /// 移動先の高さが1かどうかを確認して、必要ならミニジャンプします。
        /// </summary>
        public void CheckMiniJump()
        {
            // 横にぶつかっていなければチェック不要
            if ((chrController.collisionFlags & CollisionFlags.Sides) == 0) return;

            // ぶつかった相手を調べる
            checkCenter = transform.position
                + chrController.center
                + forwardVector * (chrController.radius + boxColliderHalfExtents.x);

            int hitCount = Physics.BoxCastNonAlloc(checkCenter, boxColliderHalfExtents, forwardVector, raycastHits, Quaternion.identity, boxColliderHalfExtents.x, mapCollisionLayerMask);
            if (hitCount == 0) return;

            float footh = chrController.bounds.min.y;
            float h = raycastHits[0].collider.bounds.max.y - footh;

            for (int i = 1; i < hitCount; i++)
            {
                h = Mathf.Max(h, raycastHits[i].collider.bounds.max.y - footh);
            }
            if (h <= miniJumpHeight)
            {
                targetJumpGround = (transform.position + forwardVector) + Vector3.up * miniJumpHeight;
                ChangeAction(ActionType.Jump);
            }
        }

        /// <summary>
        /// targetJumpGroundへのジャンプを開始します。
        /// </summary>
        public void StartTargetJump()
        {
            gameObject.layer = jumpLayer;

            // 目的の高さと、目的高さからの段差分を求める
            float top = targetJumpGround.y + miniJumpMargin;

            // Y方向の初速を決める
            // h = (g*t*t)/2;
            // 2h/g  = t*t
            float t = Mathf.Sqrt(2f * top / gravityAdd);
            myVelocity.y = gravityAdd * t;

            // X方向の速度を決める
            float total = top + miniJumpMargin;
            t = Mathf.Sqrt(2f * total / gravityAdd);
            myVelocity.x = targetJumpGround.x - transform.position.x;
            myVelocity.x = myVelocity.x / t;

            Debug.Log($"  vel = {myVelocity}");

            ChangeAction(ActionType.Air);

            Move();
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
            EndAction();
            nowAction = type;
            stellaActionScriptableObjects[(int)type].Init();
        }

        /// <summary>
        /// 現在の行動の終了処理を呼び出します。
        /// </summary>
        public void EndAction()
        {
            if (nowAction != ActionType.None)
            {
                stellaActionScriptableObjects[(int)nowAction].End();
            }
        }

        /// <summary>
        /// アニメーションから呼び出された時に実行したい処理を登録します。
        /// </summary>
        /// <param name="act">アニメが完了した時に呼び出すアクション</param>
        public static void RegisterAnimEvent(UnityAction act)
        {
            animEventAction = act;
        }

        /// <summary>
        /// アニメーションから呼び出すメソッド。
        /// </summary>
        public void OnAnimEvent()
        {
            animEventAction?.Invoke();
        }
    }
}