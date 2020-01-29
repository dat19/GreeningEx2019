using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(menuName = "Greening/Stella Actions/Create Watering", fileName = "StellaActionWatering")]
    public class StellaWatering : StellaActionScriptableObject
    {
        [Tooltip("じょうろの左右時のオイラー"), SerializeField]
        Vector3[] zyouroEular =
        {
            new Vector3(20,-55,0),
            new Vector3(15,-150,0)
        };
        [Tooltip("じょうろPivotの左右のローカル座標"), SerializeField]
        Vector3[] zyouroPivotPosition =
        {
            new Vector3(0.25f, 0.15f, -0.05f),
            Vector3.zero
        };
        [Tooltip("当たり判定用のオブジェクト"), SerializeField]
        GameObject waterTrigger = null;
        [Tooltip("水まき効果音のループ秒数"), SerializeField]
        float waterSeSeconds = 0.5f;

        enum StateType
        {
            Start,
            Action,
            End
        }

        /// <summary>
        /// トリガーオブジェクトの数
        /// </summary>
        const int TriggerCount = 10;
        /// <summary>
        /// トリガーの有効秒数。これをトリガーの秒数で割った間隔で発射
        /// </summary>
        const float TriggerTime = 3f;

        readonly GameObject[] triggerObjects = new GameObject[TriggerCount];
        readonly Rigidbody[] triggerRigidbody = new Rigidbody[TriggerCount];
        readonly Water[] triggerWater = new Water[TriggerCount];
        StateType state;
        ParticleSystem zyouroParticle = null;
        float triggerSpeed;
        int triggerIndex;
        float lastTriggerTime;
        float triggerEmitSeconds;
        float lastSeTime;

        public override void Init()
        {
            base.Init();

            state = StateType.Start;
            StellaMove.SetAnimState(StellaMove.AnimType.Water);

            // アニメからイベントが呼ばれた時に、StartAction()を実行するように登録する
            StellaMove.RegisterAnimEvent(StartAction);

            // ステラの横移動を止めておく
            StellaMove.myVelocity.x = 0;

            if (zyouroParticle == null)
            {
                zyouroParticle = StellaMove.ZyouroEmitter.GetComponent<ParticleSystem>();
                triggerSpeed = (zyouroParticle.main.startSpeed.constantMin
                    + zyouroParticle.main.startSpeed.constantMax) * 0.5f;
                triggerEmitSeconds = TriggerTime / (float)TriggerCount;
                for (int i=0; i<TriggerCount;i++)
                {
                    triggerObjects[i] = Instantiate(waterTrigger);
                    triggerRigidbody[i] = triggerObjects[i].GetComponent<Rigidbody>();
                    triggerObjects[i].SetActive(false);
                    triggerWater[i] = triggerObjects[i].GetComponent<Water>();
                }
            }

            // じょうろの方向を設定
            // index: 左0 右1
            int index = StellaMove.forwardVector.x < 0 ? 0 : 1;
            StellaMove.ZyouroPivot.localEulerAngles = zyouroEular[index];
            StellaMove.ZyouroPivot.localPosition = zyouroPivotPosition[index];
            StellaMove.ZyouroEmitter.forward = StellaMove.forwardVector;
            StellaMove.ZyouroEmitter.parent = null;
        }

        /// <summary>
        /// 放水開始
        /// </summary>
        void StartAction()
        {
            state = StateType.Action;
            zyouroParticle.Play();
            lastTriggerTime = Time.time;
            lastSeTime = Time.time - waterSeSeconds;
        }

        public override void UpdateAction()
        {
            StellaMove.ZyouroEmitter.transform.position = StellaMove.ZyouroEmitterPosition.position;

            if (state == StateType.Action)
            {
                // 水オブジェクトを生成
                if (Time.time-lastTriggerTime >= triggerEmitSeconds)
                {
                    lastTriggerTime = Time.time;
                    triggerObjects[triggerIndex].SetActive(true);
                    triggerObjects[triggerIndex].transform.position = StellaMove.ZyouroEmitterPosition.position;
                    triggerRigidbody[triggerIndex].velocity = StellaMove.ZyouroEmitter.forward * triggerSpeed;
                    triggerWater[triggerIndex].Start();
                    triggerIndex = (triggerIndex + 1) % TriggerCount;
                }

                // 水まき終了チェック
                if (!Input.GetButton("Water") && (Grow.WaitGrowCount <= 0))
                {
                    // 水まき終了
                    state = StateType.End;
                    StellaMove.SetAnimState(StellaMove.AnimType.Walk);
                    StellaMove.RegisterAnimEvent(EndAction);
                    zyouroParticle.Stop();
                    StellaMove.WaterSeStop();
                    return;
                }

                // 効果音
                if ((Time.time - lastSeTime) >= waterSeSeconds)
                {
                    lastSeTime = Time.time;
                    StellaMove.WaterSePlay();
                }

                // 後ずさりチェック
                int hitCount = PhysicsCaster.CharacterControllerCast(StellaMove.ChrController, Vector3.down, 0f, PhysicsCaster.MapCollisionPlayerOnlyLayer);

                for (int i=0;i<hitCount;i++)
                {
                    // 下げる
                    float colx = PhysicsCaster.hits[i].collider.bounds.extents.x;
                    float dist = StellaMove.ChrController.radius + colx + StellaMove.CollisionMargin;
                    float target = PhysicsCaster.hits[i].transform.position.x - dist * StellaMove.forwardVector.x;
                    float move = target - StellaMove.instance.transform.position.x;
                    if (move * StellaMove.forwardVector.x >= 0f)
                    {
                        // 向いている方向には動かさない
                        return;
                    }
                    StellaMove.myVelocity.x = move / Time.fixedDeltaTime;
                    Vector3 lastPos = StellaMove.instance.transform.position;
                    StellaMove.instance.Move();
                    lastPos.x = StellaMove.instance.transform.position.x;
                    StellaMove.instance.transform.position = lastPos;
                    StellaMove.myVelocity.x = 0f;
                }
            }
            else
            {
                StellaMove.instance.Gravity();
                StellaMove.instance.Move();
            }
        }

        // 水まきアニメが終わった処理
        void EndAction()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
        }

        /// <summary>
        /// 水まきを停止
        /// </summary>
        public override void End()
        {
            base.End();
            StellaMove.RegisterAnimEvent(null);
            zyouroParticle.Stop();
        }
    }
}