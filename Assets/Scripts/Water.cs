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

    private void OnTriggerStay(Collider other)
    {
        // 地面にぶつかったら消す
        if (other.CompareTag("Ground"))
        {
            // 中心がバウンディング内なら消す
            if ( other.bounds.Contains(transform.position))
            {
                Disable();
            }
        }
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}
