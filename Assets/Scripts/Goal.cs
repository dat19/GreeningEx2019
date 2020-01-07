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

        enum MaterialIndex
        {
            First,
            Last,
            Completed,
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
        }

        private void FixedUpdate()
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

            if (Grow.NaeGrowedCount < StageManager.NaeCount)
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
            }
        }

        /// <summary>
        /// クリア時の回転アニメを開始します。
        /// </summary>
        public static void ClearAnim()
        {
            anim.SetTrigger("Rolling");
        }

        /// <summary>
        /// 飛び立つアニメを開始します。
        /// </summary>
        public static void FlyAnim()
        {
            anim.SetTrigger("Fly");
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
    }
}