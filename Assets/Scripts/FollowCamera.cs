using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Tooltip("プレイヤーの画面左端"), SerializeField]
    float viewPointMin = 0.2f;
    [Tooltip("プレイヤーの画面右端"), SerializeField]
    float viewPointMax = 0.6f;

    Camera myCamera;
    Transform playerTransform;
    Vector3 camToPlayer;

    private void Awake()
    {
        myCamera = GetComponent<Camera>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        camToPlayer = playerTransform.position - transform.position;
    }

    private void LateUpdate()
    {
        Vector3 next = playerTransform.position - camToPlayer;
        transform.position = next;
    }
}
