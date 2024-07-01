using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance; // 싱글톤 인스턴스
    [SerializeField] private Transform target; // 카메라가 따라갈 대상
    public float followSpeed = 2f; // 카메라가 따라가는 속도
    public Vector3 offset; // 카메라의 오프셋
    private bool followTarget = true;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }

    public void FollowTarget(Transform _target)
    {
        target = _target;
    }

    public void EnableFollowTarget(bool enable)
    {
        followTarget = enable;
    }
}