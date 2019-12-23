using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace GreeningEx2019 {

    public class Textlight : MonoBehaviour
    {
        [SerializeField]
        bool ison;

        private float num = Mathf.PI;
        Material myMaterial = null;

        private void Awake()
        {
            TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
            // TextMeshProの機能を使うためにTextMeshProUGUIをオブジェクトから取ってくる
            myMaterial = tmp.fontMaterial;
        }

        void Update()
        {
            num = (ison == TitleManager.IsContinue) ? Mathf.Repeat(num + Time.deltaTime * 2, Mathf.PI*2f) : Mathf.PI;

            // OutlineのThicknessの数値を0～0.4に変化するように設定
            // 数値の変化は三角関数のSinを利用
            // 数値が負の値になるとおかしくなるので、絶対値を設定
            myMaterial.SetFloat("_OutlineWidth", Mathf.Abs(Mathf.Sin(num)) * 2 / 5);
        }
    }
}
