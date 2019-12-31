using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class NaeActable : Actable
    {
        private void Awake()
        {
            CanAction = true;
        }

        public override void Action()
        {
            Debug.Log("Action");
        }
    }
}