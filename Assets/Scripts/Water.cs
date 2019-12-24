using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [Tooltip("生存秒数"), SerializeField]
    float lifeTime = 3f;

    public void Start()
    {
        Invoke("Disable", lifeTime);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}
