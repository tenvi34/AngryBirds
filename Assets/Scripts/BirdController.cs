using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    public Transform slingshot; // 새총 오브젝트
    public Transform birdPrefab; // 발사할 새 프리팹
    //public Trajectory trajectory; // 궤적을 그릴 Trajectory 스크립트
    public float launchForceMultiplier = 10f; // 발사 힘의 크기를 조절하는 변수
    public float reloadTime = 2f; // 재발사 대기 시간

    private Transform bird;
    private Vector3 startPoint; // 마우스 드래그 시작 지점
    private bool isDragging = false; // 드래그 상태 확인
    private bool isPanning = false; // 화면 이동 상태 확인
    private bool canLaunch = true; // 발사 가능 확인
    private Vector3 lastPanPosition;  // 마지막 팬 위치

    void Start()
    {
        SpawnBird();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueDrag();
        }
        else if (Input.GetMouseButton(0) && isPanning)
        {
            ContinuePan();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ReleaseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isPanning)
        {
            isPanning = false;
        }
    }

    void StartDrag()
    {
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPoint.z = 0;

        if (canLaunch && Vector3.Distance(startPoint, bird.position) < 1f)
        {
            isDragging = true;
        }
        else if (!canLaunch)
        {
            Debug.Log("장전 중");
        }
        else
        {
            isPanning = true;
            lastPanPosition = Input.mousePosition;
        }
    }

    void ContinueDrag()
    {
        Vector3 currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentPoint.z = 0;
    }

    void ReleaseDrag()
    {
        isDragging = false;
        Vector3 launchDirection = startPoint - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        launchDirection.z = 0;

        Rigidbody2D birdRigidbody = bird.GetComponent<Rigidbody2D>();
        birdRigidbody.isKinematic = false;
        birdRigidbody.AddForce(launchDirection * launchForceMultiplier, ForceMode2D.Impulse);
        canLaunch = false;
        StartCoroutine(ReloadBird());

        CameraFollow.instance.FollowBird(bird);
    }

    void ContinuePan()
    {
        Vector3 currentPanPosition = Input.mousePosition;
        Vector3 difference = Camera.main.ScreenToWorldPoint(lastPanPosition) - Camera.main.ScreenToWorldPoint(currentPanPosition);

        Camera.main.transform.position += difference;
        lastPanPosition = currentPanPosition;
    }

    IEnumerator ReloadBird()
    {
        yield return new WaitForSeconds(reloadTime);
        canLaunch = true;
        SpawnBird();
        Debug.Log("발사 준비 완료");
    }

    void SpawnBird()
    {
        if (bird != null)
        {
            Destroy(bird.gameObject);
        }
        bird = Instantiate(birdPrefab, slingshot.position, Quaternion.identity).transform;
        var vector3 = bird.position;
        vector3.y = -1.23f;
        bird.position = vector3;
        bird.GetComponent<Rigidbody2D>().isKinematic = true;
        CameraFollow.instance.FollowBird(bird);
    }
}
