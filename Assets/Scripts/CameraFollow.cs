using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;
    public Transform target; // 따라갈 타겟
    public float smoothing = 5f; // 카메라 이동 속도

    private bool followTarget = true; // 타겟을 따라갈지 여부

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
            Vector3 targetCamPos = target.position;
            targetCamPos.z = transform.position.z;
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