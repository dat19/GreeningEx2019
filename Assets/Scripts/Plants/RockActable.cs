//#define DEBUG_LOG
//#define DEBUG_PUSHACTION
//#define DEBUG_AUTOROLL

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019 {
    public class RockActable :Actable
    {
        [Tooltip("真下に地面がない時の移動速度"), SerializeField]
        float rollSpeed = 1f;

        /// <summary>
        /// 苗の時だけ、ミニジャンプ可
        /// </summary>
        public override bool CanMiniJump { 
            get {
                return GrowInstance.state == Grow.StateType.Nae;
            }
        }

        /// <summary>
        /// ステラの速度より少し速く押して、ひっかかりをなくす
        /// </summary>
        const float PushRate = 1.1f;

        /// <summary>
        /// 効果音を止める秒
        /// </summary>
        const float SeStopSeconds = 0.1f;

        /// <summary>
        /// 転がり効果音の基本ボリューム
        /// </summary>
        const float RollingVolume = 1f;

        /// <summary>
        /// 地面を調べる時の距離
        /// </summary>
        const float GroundCheckDistance = 0.01f;

        CharacterController chrController = null;
        static Ray ray = new Ray();

        /// <summary>
        /// 岩転がし音
        /// </summary>
        AudioSource rockAudio = null;
        AudioSource RockAudio
        {
            get
            {
                if (rockAudio == null)
                {
                    rockAudio = GetComponent<AudioSource>();
                }
                return rockAudio;
            }
        }

        /// <summary>
        /// 岩転がりボリューム。0～1
        /// </summary>
        float rollingAudioVolume;

        /// <summary>
        /// 生長後、かつ、ステラが着地時、かつ、岩が着地時に動かせる
        /// </summary>
        public override bool CanAction
        {
            get
            {
                return (GrowInstance.state == Grow.StateType.Growed)
                    && StellaMove.ChrController.isGrounded;
            }
            protected set => base.CanAction = value;
        }


        Vector3 myVelocity = Vector3.zero;
        SphereCollider sphereCollider;
        /// <summary>
        /// 水しぶきを上げた
        /// </summary>
        bool isSplashed = false;

        /// <summary>
        /// 自前で調査した設置情報
        /// </summary>
        bool isGrounded = false;

        private void Awake()
        {
            chrController = GetComponent<CharacterController>();
            sphereCollider = GetComponent<SphereCollider>();
        }

        public override bool Action()
        {
            if (!CanAction) return　false;

            StellaMove.targetJumpGround = transform.position;
            StellaMove.targetJumpGround.y = chrController.bounds.max.y;
            StellaMove.myVelocity.x = 0f;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Jump);
            return true;
        }

        public override bool PushAction()
        {
            if (!CanAction || !isGrounded)
            {
#if DEBUG_PUSHACTION
                Log($"  PushAction Not Work CanAction={CanAction} / isGrounded={isGrounded}");
#endif
                return false;
            }

            // 重力加速
            Vector3 move = Vector3.zero;
            move.x = StellaMove.myVelocity.x * Time.fixedDeltaTime * PushRate;

            int hitCount = PhysicsCaster.CharacterControllerCast(chrController, StellaMove.myVelocity.normalized, StellaMove.myVelocity.magnitude * Time.fixedDeltaTime, PhysicsCaster.MapCollisionLayer, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                if (PhysicsCaster.hits[i].collider.gameObject == gameObject) continue;

                // ぶつかった場所を確認
                Vector3 closest = PhysicsCaster.hits[i].collider.ClosestPoint(transform.position);
                // 移動方向で、一定より高い場合はキャンセル
                if (((StellaMove.myVelocity.x * (closest.x - transform.position.x)) > 0f) && (closest.y >= chrController.bounds.min.y + GroundCheckDistance))
                {
#if DEBUG_PUSHACTION
                    Log($"  高いのでキャンセル {PhysicsCaster.hits[i].collider.name} closest={closest.x}, {closest.y} / now={chrController.bounds.min.y} + {GroundCheckDistance} = {chrController.bounds.min.y + GroundCheckDistance}");
#endif
                    return false;
                }
                
            }

            Vector3 lastPos = transform.position;
            chrController.Move(move);
#if DEBUG_PUSHACTION
            Debug.Log($"  after move last={lastPos.y} to {transform.position.y}");
#endif
            CheckGrounded();

#if DEBUG_PUSHACTION
            Log($"  {Time.frameCount}: PushAction {move}");
#endif
            // 移動した分、回転
            SetRotate(lastPos.x);
            return true;
        }

        /// <summary>
        /// 地面へのめり込みを修正します。
        /// </summary>
        public void OnGround()
        {
            int goidx = PhysicsCaster.GetGround(chrController.bounds.center, chrController.bounds.extents.y);
            if (goidx != -1)
            {
                float targetY = PhysicsCaster.hits[goidx].collider.bounds.max.y + chrController.bounds.extents.y;
                if (transform.position.y < targetY)
                {
                    transform.Translate(0, targetY - transform.position.y, 0);
                }
            }
        }

        private void FixedUpdate()
        {
            // 当たり判定を一致させる
            if (chrController.enabled)
            {
                sphereCollider.radius = chrController.radius;
                sphereCollider.center = chrController.center;
                sphereCollider.enabled = true;
            }

            // 生長中、地面のめり込みを防ぐ
            if (GrowInstance.state == Grow.StateType.Growing)
            {
                OnGround();
            }

            // 苗の時はここまで
            if (!CanAction) return;

            // ボリュームを下げる
            rollingAudioVolume = Mathf.Clamp01(rollingAudioVolume - (1f / SeStopSeconds) * Time.fixedDeltaTime);

            // 着地＆自動転がし確認。半径の分、下に何かないか確認
            int count = PhysicsCaster.CharacterControllerCast(chrController, Vector3.down, chrController.radius, PhysicsCaster.RockGroundedLayer, QueryTriggerInteraction.Ignore);
            float minOffset = float.PositiveInfinity;
            Vector3 minHeight = Vector3.zero;
            minHeight.y = 0f;
            isGrounded = false;
            for (int i = 0; i < count; i++) {
                // 自分と同じか、トリガーなら無効
                if ((PhysicsCaster.hits[i].collider.gameObject == gameObject)
                    ||  (PhysicsCaster.hits[i].collider.isTrigger))
                    continue;

                // ぶつかった先
                AutoRollLog($"  hit[{i}] {PhysicsCaster.hits[i].collider.name} / {PhysicsCaster.hits[i].point.x}, {PhysicsCaster.hits[i].point.y}");
                Vector3 tempPivot = PhysicsCaster.hits[i].collider.ClosestPoint(transform.position);
                Vector3 destPivot = tempPivot;

                // 真上に岩に向かってキャスト
                RaycastHit hit;
                tempPivot.y = sphereCollider.bounds.min.y - GroundCheckDistance;
                ray.origin = tempPivot;
                ray.direction = Vector3.up;
                if (!sphereCollider.Raycast(ray, out hit, sphereCollider.radius+GroundCheckDistance*2f))
                {
                    AutoRollLog($"  接点見つからず。本来はないはず origin={ray.origin} / dir={ray.direction} / dist={sphereCollider.radius+GroundCheckDistance*2f} / x={chrController.bounds.min.x} - {chrController.bounds.max.x}");
                    continue;
                }

                // 衝突した場所が、今の一番下から誤差分より下ならぶつからないので無視
                if (destPivot.y+GroundCheckDistance < sphereCollider.bounds.min.y)
                {
                    AutoRollLog($"  {destPivot.y}が、{sphereCollider.bounds.min.y}より下なのでぶつからない");
                    continue;
                }

                isGrounded = true;

                // 接した高さ分、下げる
                float diffY = destPivot.y - hit.point.y;
                AutoRollLog($"  接点 {hit.point.x}, {hit.point.y} / under={sphereCollider.bounds.min.y} - {hit.point.y} = {diffY} > {GroundCheckDistance}");
                if (diffY < minHeight.y)
                {
                    minHeight.y = diffY;
                }

                float temp = transform.position.x - hit.point.x;
                if (Mathf.Abs(temp) < Mathf.Abs(minOffset))
                {
                    minOffset = temp;
                }
                AutoRollLog($"  自動転がし：　CapsuleCast {i} / {count} / {PhysicsCaster.hits[i].collider.name} / point={PhysicsCaster.hits[i].point.x}, {PhysicsCaster.hits[i].point.y} / tempPivot={tempPivot.x}, {tempPivot.y} / minY={chrController.bounds.min.y} / minOffset={minOffset} / minHeight={minHeight.y}");
            }
            if (isGrounded)
            {
                // 着地しているので速度はリセット
                myVelocity.y = 0f;

                // 自動転がり
                if (Mathf.Abs(minOffset) >= GroundCheckDistance)
                {
                    myVelocity.x = rollSpeed * Mathf.Sign(minOffset);
                    chrController.Move(minHeight);
                }
            }
            else
            {
                // 地面がない
                myVelocity.x = 0f;

                // 重力加速
                myVelocity.y -= StellaMove.GravityAdd * Time.fixedDeltaTime;
            }

            Vector3 lastPos = transform.position;
            AutoRollLog($"  {Time.frameCount}: 自動転がし {myVelocity.x}, {myVelocity.y}");
            Vector3 nextPos = lastPos + myVelocity * Time.fixedDeltaTime;

            chrController.Move(myVelocity * Time.fixedDeltaTime);
            SetRotate(lastPos.x);

            // 着地チェック
            if (rollingAudioVolume <= 0f)
            {
                RockAudio.Stop();
            }
            else
            {
                RockAudio.volume = rollingAudioVolume;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 水しぶき
            if (!isSplashed && other.CompareTag("DeadZone"))
            {
                SoundController.Play(SoundController.SeType.RockWater);
                isSplashed = true;
                Vector3 pos = chrController.bounds.center;
                pos.y = chrController.bounds.min.y;
                StellaMove.Splash(pos);
            }
        }

        /// <summary>
        /// 着地状態かを確認します。地面だけではなく、折衝するものであればすべて着地とみなして、isGroundedに設定します。
        /// </summary>
        void CheckGrounded()
        {
            isGrounded = false;
            int count = PhysicsCaster.CharacterControllerCast(chrController, Vector3.down, GroundCheckDistance, PhysicsCaster.RockGroundedLayer);
            for (int i = 0; i < count; i++)
            {
                if (PhysicsCaster.hits[i].collider.gameObject == gameObject) continue;
                Log($"  CheckGrounded {i} / {count} / {PhysicsCaster.hits[i].collider.name} / {PhysicsCaster.hits[i].collider.transform.position}");
                if (!PhysicsCaster.hits[i].collider.isTrigger
                    || (PhysicsCaster.hits[i].collider.GetComponent<IStepOn>() != null))
                {
                    Log($"  grounded");
                    isGrounded = true;
                    return;
                }
            }
        }

        /// <summary>
        /// 前回のX座標を指定して、現在のX座標に移動するのに必要な回転をさせます。
        /// </summary>
        /// <param name="lastPosX">前回のX座標</param>
        void SetRotate(float lastPosX)
        {
            float zrot = (transform.position.x - lastPosX) / chrController.radius;
            if (!Mathf.Approximately(zrot, 0f))
            {
                rollingAudioVolume = RollingVolume;

                if (!RockAudio.isPlaying)
                {
                    RockAudio.Play();
                }
            }
            
            transform.Rotate(0, 0, -zrot * Mathf.Rad2Deg);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static void Log(object mes)
        {
            Debug.Log(mes);
        }

        [System.Diagnostics.Conditional("DEBUG_AUTOROLL")]
        static void AutoRollLog(object mes)
        {
            Debug.Log(mes);
        }
    }
}