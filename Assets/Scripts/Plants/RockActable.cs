using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019 {
    public class RockActable :Actable
    {
        [Tooltip("真下に地面がない時の移動速度"), SerializeField]
        float rollSpeed = 0.75f;

        /// <summary>
        /// 苗の時だけ、ミニジャンプ可
        /// </summary>
        public override bool CanMiniJump { get { return GrowInstance.state == Grow.StateType.Nae; }}

        /// <summary>
        /// ステラの速度より少し速く押して、ひっかかりをなくす
        /// </summary>
        const float PushRate = 1.1f;

        /// <summary>
        /// 効果音を止める秒
        /// </summary>
        const float seStopSeconds = 0.1f;

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
        AudioSource _rockAudio = null;
        AudioSource RockAudio
        {
            get
            {
                if (_rockAudio == null)
                {
                    _rockAudio = GetComponent<AudioSource>();
                }
                return _rockAudio;
            }
        }

        /// <summary>
        /// 岩転がりボリューム。0～1
        /// </summary>
        float rollingAudioVolume;

        CharacterController ChrController
        {
            get
            {
                if (chrController == null)
                {
                    chrController = GetComponent<CharacterController>();
                }
                return chrController;
            }
        }

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

        public override bool Action()
        {
            if (!CanAction) return　false;

            StellaMove.targetJumpGround = transform.position;
            StellaMove.targetJumpGround.y = ChrController.bounds.max.y;
            StellaMove.myVelocity.x = 0f;
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Jump);
            return true;
        }

        public override bool PushAction()
        {
            if (!CanAction || !chrController.isGrounded)
            {
                return false;
            }

            // 重力加速
            Vector3 move = Vector3.zero;
            move.x = StellaMove.myVelocity.x * Time.fixedDeltaTime * PushRate;
            Vector3 lastPos = transform.position;
            ChrController.Move(move);
            if (transform.position.y > lastPos.y)
            {
                // 持ちあがる時は押しキャンセル
                transform.position = lastPos;
            }

            // 移動した分、回転
            setRotate(lastPos.x);
            return true;
        }

        private void FixedUpdate()
        {
            // 当たり判定を一致させる
            if (sphereCollider == null)
            {
                sphereCollider = GetComponent<SphereCollider>();
            }
            if (ChrController.enabled)
            {
                sphereCollider.radius = ChrController.radius;
                sphereCollider.center = ChrController.center;
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
            rollingAudioVolume = Mathf.Clamp01(rollingAudioVolume - (1f / seStopSeconds) * Time.fixedDeltaTime);

            // 真下に地面がない場合、自動的に転がす
            Vector3 origin = chrController.bounds.center;
            float dist = chrController.bounds.extents.y + GroundCheckDistance;
            GameObject ground = PhysicsCaster.GetGroundWater(origin, dist);
            if (ground == null)
            {
                origin.x = chrController.bounds.min.x;
                ground = PhysicsCaster.GetGroundWater(origin, dist);
                myVelocity.x = 0;
                if (ground != null)
                {
                    myVelocity.x += rollSpeed;
                }

                origin.x = chrController.bounds.max.x;
                ground = PhysicsCaster.GetGroundWater(origin, dist);
                if (ground != null)
                {
                    myVelocity.x -= rollSpeed;
                }
            }

            // 重力加速
            myVelocity.y -= StellaMove.GravityAdd * Time.fixedDeltaTime;
            Vector3 lastPos = transform.position;
            ChrController.Move(myVelocity * Time.fixedDeltaTime);
            setRotate(lastPos.x);

            // 着地チェック
            if (ChrController.isGrounded && myVelocity.y <= 0f)
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
        /// 前回のX座標を指定して、現在のX座標に移動するのに必要な回転をさせます。
        /// </summary>
        /// <param name="lastPosX">前回のX座標</param>
        void setRotate(float lastPosX)
        {
            float zrot = (transform.position.x - lastPosX) / ChrController.radius;
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
    }
}