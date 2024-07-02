using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;
    public Transform target; // 타겟
    public float smoothing = 5f; // 카메라 이동 속도

    public Vector3 offset; // 카메라 오프셋

    private bool followTarget = true; // 타

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (followTarget && target != null)
        {
            Vector3 targetCamPos = target.position + offset;
            targetCamPos.z = transform.position.z; // z 축 고정
            transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
        }
    }

    public void FollowTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void EnableFollowTarget(bool enable)
    {
        followTarget = enable;
    }
}