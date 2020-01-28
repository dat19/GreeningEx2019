using System.Collections;
using UnityEngine;

namespace GreeningEx2019
{
    public class Goal : MonoBehaviour
    {
        public static Goal instance = null;
        
        [Tooltip("星のマテリアル。0=最初, 1=完成直前, 2=完成形"), SerializeField]
        Material[] materials = new Material[3];
        [Tooltip("苗の段階が上がる秒数"), SerializeField]
        float changeColorSeconds = 1f;
        [Tooltip("クリア時に、星が飛び去る方向。-1なら左、1なら右"), SerializeField]
        float clearFlyX = 1f;
#pragma warning disable 649
        [Tooltip("クリア可能になったら実行したいイベントを登録します。"), SerializeField]
        UnityEngine.Events.UnityEvent canClearEvent;
#pragma warning restore 649

        /// <summary>
        /// ステラが飛び乗るのを待つ場所へ移動するまでの秒数
        /// </summary>
        const float ToStandbyPositionTime = 0.5f;

        public enum MaterialIndex
        {
            First,
            Last,
            Completed,
        }

        enum StateType
        {
            Standby,        // 待機
            FollowStella,   // ステラのHoldPositionを基準に移動
            FlyWait,        // 飛ぶ前のため
            FlyStart,       // 加速
        }

        /// <summary>
        /// クリア時に星が飛ぶときの方向
        /// </summary>
        public static float ClearFlyX
        {
            get
            {
                return instance.clearFlyX;
            }
        }

        /// <summary>
        /// 星モデルの座標を返します。
        /// </summary>
        public static Vector3 StarPosition
        {
            get
            {
                return instance.transform.GetChild(0).transform.position;
            }
        }

        static Material myMaterial = null;

        /// <summary>
        /// 現在までに色に反映させた苗の数
        /// </summary>
        static float counter = 0;

        /// <summary>
        /// 開始マテリアル
        /// </summary>
        static Material startMaterial;

        /// <summary>
        /// 目指すマテリアル状態
        /// </summary>
        static Material targetMaterial;

        /// <summary>
        /// 色変更を開始した時間
        /// </summary>
        static float changeColorTime;

        /// <summary>
        /// 設定済みの苗の数。同じだった場合は初期化は省く
        /// </summary>
        static int setNaeCount;

        /// <summary>
        /// コライダー
        /// </summary>
        static Collider myCollider = null;

        /// <summary>
        /// アニメーター
        /// </summary>
        static Animator anim = null;

        /// <summary>
        /// 状態
        /// </summary>
        static StateType state;

        private void Awake()
        {
            instance = this;
            myMaterial = GetComponentInChildren<Renderer>().sharedMaterials[0];
            myMaterial.Lerp(materials[(int)MaterialIndex.First], materials[(int)MaterialIndex.First], 1f);
            startMaterial = new Material(myMaterial);
            targetMaterial = new Material(myMaterial);
            counter = 0;
            myCollider = GetComponent<Collider>();
            myCollider.enabled = false;
            anim = GetComponent<Animator>();
            state = StateType.Standby;
            setNaeCount = 0;
        }

        private void FixedUpdate()
        {
            switch (state)
            {
                case StateType.Standby:
                    updateStandby();
                    break;

                // ステラに星を合わせて動かす
                case StateType.FollowStella:
                    transform.position = StellaMove.HoldPosition - StellaClear.OffsetFromStar;
                    break;

                // 期待の速度に加速する。アニメでやるので処理は不要
                case StateType.FlyStart:
                    break;
            }
        }

        /// <summary>
        /// ゴールの色を変化させます。
        /// </summary>
        void updateStandby()
        {
            if (counter >= Grow.NaeGrowedCount) return;

            // 星の色を段階的に変化させる
            changeColorTime += Time.fixedDeltaTime;
            float t = changeColorTime / changeColorSeconds;
            if (t >= 1f)
            {
                t = 1f;
                counter = Grow.NaeGrowedCount;
            }
            myMaterial.Lerp(startMaterial, targetMaterial, t);
        }

        /// <summary>
        /// 開花させた苗の数を増やして、星のマテリアルを設定する
        /// </summary>
        public static void IncrementNaeCount()
        {
            if (setNaeCount == Grow.NaeGrowedCount) return;

            setNaeCount = Grow.NaeGrowedCount;
            changeColorTime = 0;
            startMaterial.Lerp(myMaterial, myMaterial, 0);

            if (!StageManager.CanClear)
            {
                targetMaterial.Lerp(
                    instance.materials[(int)MaterialIndex.First],
                    instance.materials[(int)MaterialIndex.Last],
                    (float)Grow.NaeGrowedCount / (float)(StageManager.NaeCount - 1));
            }
            else
            {
                SoundController.Play(SoundController.SeType.CanClear);
                targetMaterial = new Material(instance.materials[(int)MaterialIndex.Completed]);
                myCollider.enabled = true;
                instance.canClearEvent.Invoke();
            }
        }

        /// <summary>
        /// クリア時の回転アニメを開始します。
        /// </summary>
        public static void ClearAnim()
        {
            anim.SetTrigger("Rolling");
            SoundController.Play(SoundController.SeType.StarFly);

            // ステラが着地していたら、星をステラの相対位置に移動させる
            if (StellaMove.chrController.isGrounded)
            {
                instance.StartCoroutine(MoveRollingStandbyPosition());
            }
        }

        static IEnumerator MoveRollingStandbyPosition()
        {
            float time = 0f;
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            Vector3 pos = instance.transform.position;

            while (time < ToStandbyPositionTime)
            {
                time += Time.fixedDeltaTime;

                float y = StellaMove.instance.transform.position.y - StageManager.GoalToStellaOffset.y;
                pos = instance.transform.position;
                pos.y = Mathf.Lerp(pos.y, y, time / ToStandbyPositionTime);
                instance.transform.position = pos;

                yield return wait;
            }

            pos = instance.transform.position;
            pos.y = StellaMove.instance.transform.position.y - StageManager.GoalToStellaOffset.y;
            instance.transform.position = pos;
        }

        /// <summary>
        /// クリア処理をして、ステージ選択シーンへ移行します。
        /// 飛び立つアニメから、完了時に呼び出します。
        /// </summary>
        public void ClearAndStageSelect()
        {
            GameParams.StageClear();
            SceneChanger.ChangeScene(SceneChanger.SceneType.StageSelect);
        }

        /// <summary>
        /// ステラにぶら下がる
        /// </summary>
        public static void FollowStella()
        {
            state = StateType.FollowStella;
        }

        /// <summary>
        /// 飛ぶためを実行
        /// </summary>
        public static void FlyWait()
        {
            anim.SetTrigger("Fly");
            Vector3 sc = Vector3.one;
            sc.x = Mathf.Sign(ClearFlyX);
            instance.transform.localScale = sc;
            state = StateType.FlyWait;
        }

        /// <summary>
        /// X方向に移動開始
        /// </summary>
        public void FlyStart()
        {
            SoundController.Play(SoundController.SeType.StarFly);
            state = StateType.FlyStart;
            StellaMove.SetAnimState(StellaMove.AnimType.ClearFly);
        }
    }
}