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

        enum StateType
        {
            Start,
            Action,
            End
        }

        /// <summary>
        /// トリガーオブジェクトの数
        /// </summary>
        const int triggerCount = 10;
        /// <summary>
        /// トリガーの有効秒数。これをトリガーの秒数で割った間隔で発射
        /// </summary>
        const float triggerTime = 3f;

        GameObject[] triggerObjects = new GameObject[triggerCount];
        Rigidbody[] triggerRigidbody = new Rigidbody[triggerCount];
        Water[] triggerWater = new Water[triggerCount];
        StateType state;
        ParticleSystem zyouroParticle = null;
        float triggerSpeed;
        int triggerIndex;
        float lastTriggerTime;
        float triggerEmitSeconds;

        public override void Init()
        {
            state = StateType.Start;
            StellaMove.instance.SetAnimState(StellaMove.AnimType.Water);

            // アニメからイベントが呼ばれた時に、StartAction()を実行するように登録する
            StellaMove.RegisterAnimEvent(StartAction);

            // ステラの横移動を止めておく
            StellaMove.myVelocity.x = 0;

            if (zyouroParticle == null)
            {
                zyouroParticle = StellaMove.ZyouroEmitter.GetComponent<ParticleSystem>();
                triggerSpeed = (zyouroParticle.main.startSpeed.constantMin
                    + zyouroParticle.main.startSpeed.constantMax) * 0.5f;
                triggerEmitSeconds = triggerTime / (float)triggerCount;
                for (int i=0; i<triggerCount;i++)
                {
                    triggerObjects[i] = Instantiate(waterTrigger);
                    triggerRigidbody[i] = triggerObjects[i].GetComponent<Rigidbody>();
                    triggerObjects[i].SetActive(false);
                    triggerWater[i] = triggerObjects[i].GetComponent<Water>();
                }
            }

            // じょうろの方向を設定
            // index: 左0 右1
            int index = StellaMove.ForwardVector.x < 0 ? 0 : 1;
            StellaMove.ZyouroPivot.localEulerAngles = zyouroEular[index];
            StellaMove.ZyouroPivot.localPosition = zyouroPivotPosition[index];
            StellaMove.ZyouroEmitter.forward = StellaMove.ForwardVector;
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
        }

        public override void UpdateAction()
        {
            if (state == StateType.Action)
            {
                StellaMove.ZyouroEmitter.transform.position = StellaMove.ZyouroEmitterPosition.position;

                if (Time.time-lastTriggerTime >= triggerEmitSeconds)
                {
                    lastTriggerTime = Time.time;
                    triggerObjects[triggerIndex].SetActive(true);
                    triggerObjects[triggerIndex].transform.position = StellaMove.ZyouroEmitterPosition.position;
                    triggerRigidbody[triggerIndex].velocity = StellaMove.ZyouroEmitter.forward * triggerSpeed;
                    triggerWater[triggerIndex].Start();
                    triggerIndex = (triggerIndex + 1) % triggerCount;
                }

                if (!Input.GetButton("Water"))
                {
                    // 水まき終了
                    state = StateType.End;
                    StellaMove.instance.SetAnimState(StellaMove.AnimType.Walk);
                    StellaMove.RegisterAnimEvent(EndAction);
                    zyouroParticle.Stop();
                }
            }
        }

        // 水まきアニメが終わった処理
        void EndAction()
        {
            StellaMove.instance.ChangeAction(StellaMove.ActionType.Walk);
        }
    }
}