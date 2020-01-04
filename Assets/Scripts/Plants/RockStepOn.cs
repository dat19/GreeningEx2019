using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class RockStepOn : MonoBehaviour, IStepOn
    {
        Grow grow;

        void Awake()
        {
            grow = GetComponent<Grow>();
        }

        public void StepOn()
        {
            if (grow.state != Grow.StateType.Growed) return;

            Debug.Log($"岩着地");
        }
    }
}