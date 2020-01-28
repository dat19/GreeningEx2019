using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class FootSe : MonoBehaviour
    {
        const int WalkSeCount = 3;

        public void FootStamp()
        {
            SoundController.Play(SoundController.SeType.Walk0 + Random.Range(0, WalkSeCount));
        }
    }
}