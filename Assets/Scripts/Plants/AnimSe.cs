using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class AnimSe : MonoBehaviour
    {
        [Tooltip("再生する効果音"), SerializeField]
        SoundController.SeType se = SoundController.SeType.None;

        public void PlaySe()
        {
            SoundController.Play(se);
        }
    }
}