using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class GrowRock : Grow
    {
        public override void GrowDone()
        {
            base.GrowDone();
            gameObject.tag = "Rock";
        }
    }
}