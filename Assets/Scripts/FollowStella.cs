using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class FollowStella : MonoBehaviour
    {
        Transform pivotTransform = null;

        private void LateUpdate()
        {
            if (StellaMove.instance != null)
            {
                if (pivotTransform == null)
                {
                    pivotTransform = StellaMove.instance.transform.Find("Pivot");
                }
                transform.position = pivotTransform.transform.position;
                transform.rotation = pivotTransform.transform.localRotation;
            }
        }
    }
}