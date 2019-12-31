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

        [Tooltip("重力加速度(速度/秒)"), SerializeField]
        float gravityAdd = 20f;
        [Tooltip("ステラの各状態を制御するためのStellaActionアセット。Create > Greening > StellaActionsから、必要なものを作成"), SerializeField]
        StellaActionScriptableObject[] stellaActionScriptableObjects = null;
        [Tooltip("歩く際のデフォルトの落下距離"), SerializeField]
        float walkDownY = 0.1f;
        [Tooltip("ミニジャンプチェックをする距離"), SerializeField]
        float miniJumpCheckX = 0.5f;
        [Tooltip("ミニジャンプで乗れる高さ"), SerializeField]
        float miniJumpHeight = 1.2f;
        [Tooltip("ミニジャンプ時に、余分にジャンプする高さ"), SerializeField]
        float miniJumpMargin = 0.25f;
        [Tooltip("じょうろピボットオブジェクト"), SerializeField]
        Transform zyouroPivot = null;
        [Tooltip("じょうろの水のエミッターの位置"), SerializeField]
        Transform zyouroEmitterPosition = null;
        [Tooltip("じょうろの水のエミッター"), SerializeField]
        Transform zyouroEmitter = null;

        [Header("デバッグ")]
        [Tooltip("常に操作可能にしたい時、チェックします。"), SerializeField]
        bool isDebugMovable = false;

        /// <summary>
        /// ステラの行動定義
        /// </summary>
        public enum ActionType
        {
            None = -1,
            Start,    // 0開始時の演出
            Walk,     // 1立ち、歩き
            Air,      // 2落下、着地
            Jump,     // 3ジャンプまでのアニメ
            Water,    // 4水まき
            LifetUp,  // 5苗を持ち上げる
            NaeWalk,  // 6苗運び
            Putdown,  // 7苗を置く
            Ivy,      // 8ツタにつかまる
            Watage,   // 9綿毛につかまる
            Tamanori, // 10岩にのる
            Obore,    // 11溺れ
            Clear     // 12ステージクリア
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
            Water,      // 4水まき
            Obore,      // 5溺れ
            Ivy,        // 6ツタ
            Dandelion,  // 7綿毛に捕まる
            Clear,      // 8クリア
        }

        /// <summary>
        /// 列挙する接触したオブジェクトの上限数
        /// </summary>
        const int CollisionMax = 8;

        /// <summary>
        /// 前方を表すベクトル
        /// </summary>
        public static Vector3 forwardVector;

        /// <summary>
        /// 
        /// </summary>
        public static float GravityAdd { get { return instance.gravityAdd; } }

        /// <summary>
        /// 地面のレイヤーマスク
        /// </summary>
        public static LayerMask MapCollisionLayerMask { get; private set; }

        /// <summary>
        /// 移動先を判定する厚さの半分
        /// </summary>
        Vector3 boxColliderHalfExtents = new Vector3(0.05f, 0, 1);

        public static CharacterController chrController { get; private set; }
        public static Vector3 myVelocity = Vector3.zero;
        public static Transform ZyouroPivot { get { return instance.zyouroPivot; } }
        public static Transform ZyouroEmitterPosition { get { return instance.zyouroEmitterPosition; } }
        public static Transform ZyouroEmitter { get { return instance.zyouroEmitter; } }
        /// <summary>
        /// ピボットのTransform
        /// </summary>
        public static Transform Pivot { get; private set; }

        static Animator anim;
        static ActionType nowAction = ActionType.None;
        static RaycastHit[] raycastHits = new RaycastHit[CollisionMax];
        static Vector3 checkCenter;
        static UnityAction animEventAction = null;
        static Vector3 targetJumpGround = Vector3.zero;
        static int defaultLayer = 0;
        static int jumpLayer = 0;
        static ParticleSystem splashParticle = null;
        public static ActionBox ActionBoxInstance { get; private set; }

        void Awake()
        {
            instance = this;
            chrController = GetComponent<CharacterController>();
            anim = GetComponentInChildren<Animator>();
            anim.SetInteger("State", (int)AnimType.Walk);
            nowAction = ActionType.Walk;
            MapCollisionLayerMask = LayerMask.GetMask("MapCollision");
            boxColliderHalfExtents.y = chrController.height * 0.5f - walkDownY;
            defaultLayer = LayerMask.NameToLayer("Player");
            jumpLayer = LayerMask.NameToLayer("Jump");
            forwardVector = Vector3.right;
            splashParticle = transform.Find("Splash").GetComponent<ParticleSystem>();
            Pivot = transform.Find("Pivot");
            ActionBoxInstance = GetComponentInChildren<ActionBox>();
            ActionBoxInstance.Init();
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

            stellaActionScriptableObjects[(int)nowAction]?.UpdateAction();
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
            chrController.Move(move);

            if (!chrController.isGrounded && (nowAction == ActionType.Walk))
            {
                // 歩き時は、乗り越えられる段差の高さ分、落下を許容する
                move.Set(0, -chrController.stepOffset - move.y, 0);
                chrController.Move(move);
            }

            anim.SetBool("IsGrounded", chrController.isGrounded);
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
        /// 移動先の高さをチェックして、必要ならミニジャンプします。
        /// </summary>
        public void CheckMiniJump()
        {
            // 移動先に段差がないかを確認
            float startOffset = chrController.radius + boxColliderHalfExtents.x;
            checkCenter = transform.position
                + chrController.center
                + forwardVector * startOffset;
            float dist = (miniJumpCheckX - startOffset);

            int hitCount = Physics.BoxCastNonAlloc(checkCenter, boxColliderHalfExtents, forwardVector, raycastHits, Quaternion.identity, dist, MapCollisionLayerMask);
            if (hitCount == 0) return;

            float footh = chrController.bounds.min.y;
            int hitIndex = 0;
            float h = raycastHits[hitIndex].collider.bounds.max.y - footh;

            for (int i = 1; i < hitCount; i++)
            {
                float temph = raycastHits[i].collider.bounds.max.y - footh;
                if (temph > h)
                {
                    h = temph;
                    hitIndex = i;
                }
            }

            if (h <= miniJumpHeight)
            {
                targetJumpGround = raycastHits[hitIndex].transform.position;
                targetJumpGround.y = chrController.bounds.min.y + h;
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
            float top = targetJumpGround.y + miniJumpMargin - chrController.bounds.min.y;

            // Y方向の初速を決める
            // h = (g*t*t)/2;
            // 2h/g  = t*t
            float t = Mathf.Sqrt(2f * top / gravityAdd);
            myVelocity.y = gravityAdd * (t - Time.fixedDeltaTime);

            // X方向の速度を決める
            float total = top + miniJumpMargin;
            t = Mathf.Sqrt(2f * total / gravityAdd);
            myVelocity.x = (targetJumpGround.x - transform.position.x) / (t + Time.fixedDeltaTime);

            ChangeAction(ActionType.Air);

            Move();
        }

        /// <summary>
        /// アニメーターのStateを指定の状態にします。
        /// </summary>
        /// <param name="type">StellaMove.AnimTypeで指定</param>
        public static void SetAnimState(AnimType type)
        {
            anim.SetInteger("State", (int)type);
        }

        /// <summary>
        /// 指定のアニメのパラメーターのbool値を設定します。
        /// </summary>
        /// <param name="param">パラメーター名</param>
        /// <param name="flag">設定したいbool値</param>
        public static void SetAnimBool(string param, bool flag)
        {
            anim.SetBool(param, flag);
        }

        /// <summary>
        /// 指定のトリガーを設定します。
        /// </summary>
        /// <param name="param"></param>
        public static void SetAnimTrigger(string param)
        {
            anim.SetTrigger(param);
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

        void Restart()
        {
            SceneChanger.ChangeScene(SceneChanger.SceneType.Game);
        }

        /// <summary>
        /// 現在の場所で水しぶきを上げる
        /// </summary>
        public void Splash()
        {
            splashParticle.transform.position = new Vector3(chrController.bounds.center.x, chrController.bounds.min.y);
            splashParticle.transform.forward = Vector3.up;
            splashParticle.Play();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ClearCheck(other)) return;

            stellaActionScriptableObjects[(int)nowAction]?.OnTriggerEnter(other);
        }
        private void OnTriggerStay(Collider other)
        {
            if (ClearCheck(other)) return;

            stellaActionScriptableObjects[(int)nowAction]?.OnTriggerStay(other);
        }
        private void OnTriggerExit(Collider other)
        {
            stellaActionScriptableObjects[(int)nowAction]?.OnTriggerExit(other);
        }

        /// <summary>
        /// クリアチェック。これ以降の処理が不要な場合、trueを返します。
        /// </summary>
        /// <returns>以降の当たり判定が不要な時、true</returns>
        bool ClearCheck(Collider other)
        {
            if (!StageManager.CanMove) return true;

            // クリアチェック
            if (other.CompareTag("Finish"))
            {
                StageManager.StartClear();
                return true;
            }

            return false;
        }
    }
}