#define DEBUG_GUI
//#define DEBUG_MINIJUMP

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
        [Tooltip("ミニジャンプ(飛び降り)最高速度"), SerializeField]
        float miniJumpSpeedMax = 1.5f;
        [Tooltip("じょうろピボットオブジェクト"), SerializeField]
        Transform zyouroPivot = null;
        [Tooltip("じょうろの水のエミッターの位置"), SerializeField]
        Transform zyouroEmitterPosition = null;
        [Tooltip("じょうろの水のエミッター"), SerializeField]
        Transform zyouroEmitter = null;
        [Tooltip("左手Transform"), SerializeField]
        Transform leftTransform = null;

        [Header("デバッグ")]
        [Tooltip("常に操作可能にしたい時、チェックします。"), SerializeField]
        bool isDebugMovable = false;
        [Tooltip("GUISkin"), SerializeField]
        GUISkin guiSkin = null;

        /// <summary>
        /// 移動速度(秒速)
        /// </summary>
        public const float MoveSpeed = 3.5f;

        /// <summary>
        /// ステラの横向きの角度
        /// </summary>
        public float RotateY = 40f;

        /// <summary>
        /// 方向転換秒数
        /// </summary>
        public const float TurnSeconds = 0.15f;

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
            LiftUp,   // 5苗を持ち上げる
            NaeWalk,  // 6苗運び
            PutDown,  // 7苗を置く
            Ivy,      // 8ツタにつかまる
            Fluff,    // 9綿毛につかまる
            Tamanori, // 10岩にのる
            Obore,    // 11溺れ
            MushroomJump,   // 12マッシュルームジャンプ
            Clear     // 13ステージクリア
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
            LiftUp,     // 8持ち上げる
            PutDown,    // 9下す
            Clear,      // 10クリア
            ClearFly,   // 11クリアで飛んで行くアニメ
        }

        /// <summary>
        /// AdjustWalkの結果の列挙子
        /// </summary>
        public enum AdjustWalkResult
        {
            Continue,   // 継続
            Reach,      // 到着
            Abort       // 引っかかったので中断
        }

        /// <summary>
        /// 列挙する接触したオブジェクトの上限数
        /// </summary>
        const int CollisionMax = 8;

        /// <summary>
        /// 苗を置く時のオフセットX座標
        /// </summary>
        public const float NaePutDownOffsetX = 0.4f;

        /// <summary>
        /// 当たり判定を補正する際に間に入れるすきま
        /// </summary>
        public const float CollisionMargin = 0.01f;

        /// <summary>
        /// ミニジャンプ時に、余分にジャンプする高さ
        /// </summary>
        public const float MiniJumpMargin = 0.25f;

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
        /// ミニジャンプの最高速。ミニジャンプで飛び乗る時の最高速度
        /// </summary>
        public static float MiniJumpSpeedMax
        {
            get
            {
                return instance.miniJumpSpeedMax;
            }
        }

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
        /// 現在つかんでいるツタのインスタンス
        /// </summary>
        public static Ivy IvyInstance { get; private set; }

        /// <summary>
        /// 苗を保持する座標
        /// </summary>
        public static Vector3 HoldPosition
        {
            get
            {
                return (instance.zyouroPivot.position+instance.leftTransform.position) * 0.5f;
            }
        }

        /// <summary>
        /// ピボットのTransform
        /// </summary>
        public static Transform Pivot { get; private set; }

        /// <summary>
        /// アクション用判定インスタンス
        /// </summary>
        public static ActionBox ActionBoxInstance { get; private set; }

        /// <summary>
        /// アニメの再生時間を返します。
        /// </summary>
        public static float AnimTime
        {
            get
            {
                return anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
        }

        /// <summary>
        /// 苗を持っている時、インスタンスを設定します。離したらnullにします。
        /// </summary>
        public static NaeActable naeActable = null;

        /// <summary>
        /// 苗を置く座標
        /// </summary>
        public static Vector3 naePutPosition;

        /// <summary>
        /// ジャンプ時の目的地面座標
        /// </summary>        
        public static Vector3 targetJumpGround = Vector3.zero;

        /// <summary>
        /// 現在の行動
        /// </summary>
        public static ActionType NowAction { get; private set; }

        /// <summary>
        /// ステップオンを有効にしたオブジェクト
        /// </summary>
        public static GameObject stepOnObject;

        static Animator anim;
        static RaycastHit[] raycastHits = new RaycastHit[CollisionMax];
        static Vector3 checkCenter;
        static UnityAction animEventAction = null;
        static int defaultLayer = 0;
        static int jumpLayer = 0;
        static ParticleSystem splashParticle = null;
        /// <summary>
        /// ターンを開始してからの経過秒数
        /// </summary>
        static float turnTime = 0;

        /// <summary>
        /// ターンする方向。-1=左 / 1=右
        /// </summary>
        static float turnDirecory = 0;


        void Awake()
        {
            instance = this;
            chrController = GetComponent<CharacterController>();
            anim = GetComponentInChildren<Animator>();
            anim.SetInteger("State", (int)AnimType.Walk);
            NowAction = ActionType.Walk;
            MapCollisionLayerMask = LayerMask.GetMask("MapCollision");
            boxColliderHalfExtents.y = chrController.height * 0.5f - walkDownY;
            defaultLayer = LayerMask.NameToLayer("Player");
            jumpLayer = LayerMask.NameToLayer("Jump");
            forwardVector = Vector3.right;
            splashParticle = transform.Find("Splash").GetComponent<ParticleSystem>();
            splashParticle.transform.SetParent(null);
            Pivot = transform.Find("Pivot");
            ActionBoxInstance = GetComponentInChildren<ActionBox>();
            naeActable = null;
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

            stellaActionScriptableObjects[(int)NowAction]?.UpdateAction();
        }

        private void LateUpdate()
        {
            stellaActionScriptableObjects[(int)NowAction]?.LateUpdate();
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

            if (!chrController.isGrounded 
                && stellaActionScriptableObjects[(int)NowAction].canStepDown)
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
            if (chrController.isGrounded && myVelocity.y <= 0f)
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
        /// 移動先の高さをボックスコライダーでチェックして、必要ならミニジャンプします。
        /// 移動していない時は、チェックしません。
        /// </summary>
        public void CheckMiniJump()
        {
            Vector3 move = Vector3.zero;
            move.x = Input.GetAxisRaw("Horizontal");
            if (Mathf.Approximately(move.x, 0f)) return;

            // 移動先に段差がないかを確認
            float startOffset = chrController.radius + boxColliderHalfExtents.x;
            checkCenter = chrController.bounds.center
                + move * startOffset;
            float dist = (miniJumpCheckX - startOffset);
            int hitCount = Physics.BoxCastNonAlloc(checkCenter, boxColliderHalfExtents, move, raycastHits, Quaternion.identity, dist, MapCollisionLayerMask);
            if (hitCount == 0) return;

            float footh = chrController.bounds.min.y;
            int hitIndex = -1;
            float h = float.NegativeInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                // ミニジャンプできないものは対象から外す
                Actable []act = raycastHits[i].collider.GetComponents<Actable>();
                if (act != null)
                {
                    bool cantMiniJump = false;
                    for (int j=0;j<act.Length;j++)
                    {
                        if (!act[j].CanMiniJump)
                        {
                            Log($"  cant MiniJump {raycastHits[i].collider.name}");
                            cantMiniJump = true;
                            break;
                        }
                    }
                    if (cantMiniJump) continue;
                }

                float temph = raycastHits[i].collider.bounds.max.y - footh;
                if (temph > h)
                {
                    h = temph;
                    hitIndex = i;
                }
            }

            if (hitIndex == -1) return;

            Log($"  h={h} > {chrController.stepOffset} and <= {miniJumpHeight}");
            if ((h > chrController.stepOffset) && (h <= miniJumpHeight))
            {
                targetJumpGround = raycastHits[hitIndex].transform.position;
                targetJumpGround.y = chrController.bounds.min.y + h;
                ChangeAction(ActionType.Jump);
            }
        }

        /// <summary>
        /// 静止状態から指定の高さを落下するのにかかる秒数を返します。
        /// この値にgravityAddをかけるとジャンプの初速が得られる。
        /// </summary>
        /// <param name="h">高さ</param>
        /// <returns>指定の高さ落下するのにかかる秒数</returns>
        public static float GetFallTime(float h)
        {
            // Y方向の初速を決める
            // h = (g*t*t)/2;
            // 2h/g  = t*t
            return Mathf.Sqrt(2f * h / GravityAdd);
        }

        /// <summary>
        /// targetJumpGroundへのジャンプを開始します。
        /// </summary>
        public void StartTargetJump()
        {
            gameObject.layer = jumpLayer;

            // 目的の高さと、目的高さからの段差分を求める
            float top = targetJumpGround.y + MiniJumpMargin - chrController.bounds.min.y;

            // Y方向の初速を決める
            // h = (g*t*t)/2;
            // 2h/g  = t*t
            float t = GetFallTime(top);
            myVelocity.y = gravityAdd * (t - Time.fixedDeltaTime);

            // X方向の速度を決める
            float total = top + MiniJumpMargin;
            t = Mathf.Sqrt(2f * total / gravityAdd);
            myVelocity.x = (targetJumpGround.x - transform.position.x) / (t + Time.fixedDeltaTime);

            SoundController.Play(SoundController.SeType.MiniJump);

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
        /// アニメに状態を設定
        /// </summary>
        /// <param name="key">パラメーター</param>
        /// <param name="value">設定する値</param>
        public static void SetAnimFloat(string key, float value)
        {
            anim.SetFloat(key, value);
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
            NowAction = type;
            stellaActionScriptableObjects[(int)type].Init();
        }

        /// <summary>
        /// 苗を持っているなら苗歩き、そうでないなら歩きへ
        /// </summary>
        public void ChangeToWalk()
        {
            ChangeAction(naeActable != null ? ActionType.NaeWalk : ActionType.Walk);
        }

        /// <summary>
        /// 現在の行動の終了処理を呼び出します。
        /// </summary>
        public void EndAction()
        {
            if (NowAction != ActionType.None)
            {
                stellaActionScriptableObjects[(int)NowAction].End();
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
        /// 指定の座標に水しぶきを発生させます。
        /// </summary>
        /// <param name="pos"></param>
        public static void Splash(Vector3 pos)
        {
            splashParticle.transform.position = pos;
            splashParticle.transform.forward = Vector3.up;
            splashParticle.Play();
        }

        /// <summary>
        /// 現在の場所で水しぶきを上げる
        /// </summary>
        public void Splash()
        {
            Splash(new Vector3(chrController.bounds.center.x, chrController.bounds.min.y));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ClearCheck(other)) return;

            if ((NowAction != ActionType.Obore) && other.CompareTag("DeadZone"))
            {
                ChangeAction(ActionType.Obore);
                return;
            }

            stellaActionScriptableObjects[(int)NowAction]?.OnTriggerEnter(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (ClearCheck(other)) return;

            stellaActionScriptableObjects[(int)NowAction]?.OnTriggerStay(other);
        }
        private void OnTriggerExit(Collider other)
        {
            stellaActionScriptableObjects[(int)NowAction]?.OnTriggerExit(other);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            stellaActionScriptableObjects[(int)NowAction]?.OnControllerColliderHit(hit);
        }

#if DEBUG_GUI
        private void OnGUI()
        {
            GUI.Label(new Rect(30, 30, 1000, 50), $"Act={NowAction}", guiSkin.GetStyle("Label"));
        }
#endif

        /// <summary>
        /// クリアチェック。これ以降の処理が不要な場合、trueを返します。
        /// </summary>
        /// <returns>以降の当たり判定が不要な時、true</returns>
        bool ClearCheck(Collider other)
        {
            if (!StageManager.CanMove || StageManager.IsClearPlaying) return true;

            // クリアチェック
            if (other.CompareTag("Finish"))
            {
                StageManager.StartClear();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 指定の座標に、向きをそのままで歩きます。到着したらtrueを返します。
        /// </summary>
        /// <param name="targetX">目的地X座標</param>
        /// <param name="walkSpeed">歩き速度</param>
        /// <returns>結果をAdjustWalkResultで返します。</returns>
        public static AdjustWalkResult AdjustWalk(float targetX, float walkSpeed)
        {
            float dist = targetX - instance.transform.position.x;
            float sign = Mathf.Sign(dist);
            dist = Mathf.Abs(dist);
            AdjustWalkResult reached = AdjustWalkResult.Continue;
            float step = walkSpeed * Time.fixedDeltaTime;

            if (dist <= step)
            {
                reached = AdjustWalkResult.Reach;
                walkSpeed = dist * Time.fixedDeltaTime;
                anim.SetBool("Back", false);
            }
            else
            {
                // 向きと移動方向が逆ならアニメをバックにする
                anim.SetBool("Back", (sign * forwardVector.x) < -0.5f);
            }

            anim.SetFloat("VelX", walkSpeed);

            myVelocity.x = walkSpeed * sign;
            instance.Gravity();
            float lastx = instance.transform.position.x;
            instance.Move();

            // 移動していないのでアボート
            if ((reached != AdjustWalkResult.Reach)
                && Mathf.Approximately(lastx, instance.transform.position.x))
            {
                return AdjustWalkResult.Abort;
            }

            return reached;
        }

        /// <summary>
        /// 下にあるMapCollisionレイヤーのオブジェクトを返します。
        /// </summary>
        /// <returns>見つけたオブジェクト。オブジェクトがなければnull</returns>
        public static int GetUnderMap(RaycastHit[] hits)
        {
            Vector3 origin = chrController.bounds.center;
            return Physics.RaycastNonAlloc(origin, Vector3.down, hits, chrController.height * 0.5f + 0.1f, MapCollisionLayerMask); ;
        }

        /// <summary>
        /// 足元のチェックをして、StepOnを呼び出します。
        /// </summary>
        public static void CheckStepOn()
        {
            int hcnt = GetUnderMap(raycastHits);
            for (int i = 0; i < hcnt; i++)
            {
                IStepOn so = raycastHits[i].collider.GetComponent<IStepOn>();
                if (so != null)
                {
                    so.StepOn();
                }
            }
        }

        /// <summary>
        /// 上下キーが押されていて、ツタと重なっていたら、そのツタのインスタンスを返します。
        /// </summary>
        /// <returns></returns>
        public static bool CheckIvyHold()
        {
            float v = Input.GetAxisRaw("Vertical");

            // 上下キーが押されていなければなし
            if (Mathf.Approximately(v, 0f)) return false;

            if (v < -0.5f)
            {
                // 下キーの時は、着地していたら移行無し
                Vector3 foot = chrController.bounds.center;
                foot.y = chrController.bounds.min.y;
                int goidx = PhysicsCaster.GetGround(foot, 0.1f);
                if (goidx != -1)
                {
                    return false;
                }
            }

            IvyInstance = CheckIvyOverlap();
            if (IvyInstance == null) return false;
            if (IsIvyUp())
            {
                return IvyInstance.Hold();
            }

            return false;
        }

        /// <summary>
        /// ツタと重なっているかを確認します。重なっていたら、ツタのインスタンスを返します。
        /// </summary>
        /// <returns>見つけたツタのインスタンス。なければnull</returns>
        static Ivy CheckIvyOverlap()
        {
            int hitCount = PhysicsCaster.CharacterControllerCast(chrController, Vector3.down, 0f, PhysicsCaster.MapLayer, QueryTriggerInteraction.Collide);
            for (int i=0;i<hitCount;i++)
            {
                Ivy ivy = PhysicsCaster.hits[i].collider.GetComponent<Ivy>();
                if (ivy != null) return ivy;
            }

            return null;
        }

        /// <summary>
        /// ツタが手の上にあればtrueを返します。
        /// </summary>
        /// <returns>ツタが手の上にある時、true</returns>
        public static bool IsIvyUp()
        {
            // 上にツタがあるか？
            Vector3 origin = HoldPosition;
            origin.x = IvyInstance.BoxColliderInstance.bounds.min.x - 0.05f;
            origin.z = IvyInstance.transform.position.z;
            int hitCount = PhysicsCaster.Raycast(origin, Vector3.right, 0.1f, PhysicsCaster.NaeLayer, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hitCount; i++)
            {
                if (PhysicsCaster.hits[i].collider.GetComponent<Ivy>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ターンを開始する
        /// </summary>
        public void StartTurn(float dir)
        {
            turnTime = 0f;
            turnDirecory = dir;
            myVelocity.x = 0f;
            Turn();
        }

        /// <summary>
        /// ターン処理
        /// </summary>
        /// <returns>ターンが完了したらtrue</returns>
        public bool Turn()
        {
            Vector3 e = StellaMove.Pivot.eulerAngles;

            if (turnDirecory < -0.5f)
            {
                // 左を向く
                e.y = RotateY;
            }
            else
            {
                // 右を向く
                e.y = -RotateY;
            }

            turnTime += Time.fixedDeltaTime;
            float delta = turnTime / TurnSeconds;

            if (delta >= 1f)
            {
                delta = 1f;
                StellaMove.forwardVector.x = -Mathf.Sign(e.y);
                return true;
            }

            e.y = Mathf.LerpAngle(-e.y, e.y, delta);
            StellaMove.Pivot.eulerAngles = e;
            return false;
        }

        [System.Diagnostics.Conditional("DEBUG_MINIJUMP")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }
    }
}