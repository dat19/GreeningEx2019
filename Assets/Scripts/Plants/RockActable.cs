﻿//#define DEBUG_LOG

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
                return false;
            }

            // 重力加速
            Vector3 move = Vector3.zero;
            move.x = StellaMove.myVelocity.x * Time.fixedDeltaTime * PushRate;
            Vector3 lastPos = transform.position;
            chrController.Move(move);
            if (transform.position.y > lastPos.y)
            {
                // 持ちあがる時は押しキャンセル
                transform.position = lastPos;
            }
            CheckGrounded();

            Log($"  {Time.frameCount}: PushAction {move}");

            // 移動した分、回転
            SetRotate(lastPos.x);
            return true;
        }

        private void FixedUpdate()
        {
            // 当たり判定を一致させる
            if (sphereCollider == null)
            {
                sphereCollider = GetComponent<SphereCollider>();
            }
            if (chrController.enabled)
            {
                sphereCollider.radius = chrController.radius;
                sphereCollider.center = chrController.center;
                sphereCollider.enabled = true;
            }

            // 生長中、地面のめり込みを防ぐ
            if (GrowInstance.state == Grow.StateType.Growing)
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

            // 苗の時はここまで
            if (!CanAction) return;

            // ボリュームを下げる
            rollingAudioVolume = Mathf.Clamp01(rollingAudioVolume - (1f / SeStopSeconds) * Time.fixedDeltaTime);

            // 真下に地面がない場合、自動的に転がす
            int count = PhysicsCaster.CharacterControllerCast(chrController, Vector3.down, GroundCheckDistance, PhysicsCaster.RockGroundedLayer);
            Vector3 pivotPoint = Vector3.zero;
            float minOffset = float.PositiveInfinity;
            bool isAutoRoll = false;
            for (int i = 0; i < count; i++) {
                if (PhysicsCaster.hits[i].collider.gameObject == gameObject) continue;

                isAutoRoll = true;
                Vector3 tempPivot = PhysicsCaster.hits[i].collider.ClosestPoint(transform.position);
                float temp = transform.position.x - tempPivot.x;
                if (Mathf.Abs(temp) < Mathf.Abs(minOffset))
                {
                    minOffset = temp;
                    pivotPoint = tempPivot;
                }
                Log($"  CapsuleCast {i} / {count} / {PhysicsCaster.hits[i].collider.name} / {PhysicsCaster.hits[i].point} / me={transform.position} / {tempPivot} / minOffset={minOffset}");
            }
            if (isAutoRoll && (Mathf.Abs(minOffset) >= GroundCheckDistance))
            {
                myVelocity.x = rollSpeed * Mathf.Sign(minOffset);
            }
            else
            {
                isAutoRoll = false;
                myVelocity.x = 0f;
            }

            // 重力加速
            myVelocity.y -= StellaMove.GravityAdd * Time.fixedDeltaTime;
            Vector3 lastPos = transform.position;
            Log($"  {Time.frameCount}: 自動転がし {myVelocity.x}, {myVelocity.y}");
            Vector3 nextPos = lastPos + myVelocity * Time.fixedDeltaTime;

            // 自動回転時、回転に合わせて落下させる
            if (isAutoRoll)
            {
                Vector3 cp = pivotPoint - lastPos;
                float movedX = pivotPoint.x - nextPos.x;
                float dx = (cp.x - movedX) / chrController.radius;
                Log($"  dx={dx}");
                if (Mathf.Abs(dx) < 1f)
                {
                    float th = Mathf.Acos(dx);
                    float nextY = nextPos.y - chrController.radius * Mathf.Sin(th);
                    float dy = nextY - pivotPoint.y;
                    Log($"  th={th} / {th * Mathf.Rad2Deg} / cp={cp.x}, {cp.y} / pivotPoint={pivotPoint.x}, {pivotPoint.y} / lastPos={lastPos.x}, {lastPos.y} / movedX={movedX} / sin={Mathf.Sin(th)} / radius={chrController.radius} / dy={dy} / nextY={nextY} / center={chrController.bounds.center.x}, {chrController.bounds.center.y}");
                    //UnityEditor.EditorApplication.isPaused = true;
                    myVelocity.y += dy / Time.fixedDeltaTime;
                }
            }

            chrController.Move(myVelocity * Time.fixedDeltaTime);


            CheckGrounded();
            SetRotate(lastPos.x);

            // 着地チェック
            if (isGrounded && myVelocity.y <= 0f)
            {
                myVelocity.y = 0f;
            }

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
    }
}