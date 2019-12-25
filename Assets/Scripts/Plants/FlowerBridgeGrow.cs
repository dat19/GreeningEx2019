using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class FlowerBridgeGrow : Grow
    {
        Animator flowerAnim = null;
        FlowerBridge flowerBridge = null;
        float dir = 1;

        protected new void Awake()
        {
            base.Awake();
            flowerAnim = transform.Find("Flower").GetComponent<Animator>();
            flowerBridge = GetComponent<FlowerBridge>();
        } 

        protected override void OnTriggerEnter(Collider other)
        {
            if (CanGrow(other))
            {
                flowerBridge.PutFlower(StellaMove.forwardVector.x);
                flowerAnim.SetTrigger("Grow");
            }
        }

        public override void GrowDone()
        {
            base.GrowDone();
            flowerBridge.OpenFlower();
        }
    }
}