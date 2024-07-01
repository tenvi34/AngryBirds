using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    public Transform slingshot; // 새총 오브젝트
    public Transform birdPrefab; // 발사할 새 프리팹
    public float launchForceMultiplier = 10f; // 발사 힘의 크기를 조절하는 변수
    public float respawnTime = 3.0f; // 리스폰 대기 시간

    private Transform bird;
    private Vector3 startPoint; // 마우스 드래그 시작 지점
    private bool isDragging = false; // 드래그 상태 확인
    private bool isPanning = false; // 화면 이동 상태 확인
    private bool canLaunch = true; // 발사 가능 확인
    private Vector3 lastPanPosition; // 마지막 화면 위치

    private CameraFollow cameraFollow;

    // 라인렌더러
    public LineRenderer _renderer;
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    public LayerMask collisionMask; // 설정한 레이아웃에만 충돌 감지

    void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        SpawnBird();

        // LineRenderer 설정 -> 임시 테스트 코드라 추후 Inspector 창에서 재설정 할 예정
        _renderer.startWidth = 0.1f;
        _renderer.endWidth = 0.1f;
        _renderer.material = new Material(Shader.Find("Sprites/Default"));
        _renderer.startColor = Color.red;
        _renderer.endColor = Color.yellow;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼을 눌렀을 때
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging) // 마우스 왼쪽 버튼을 누르고 있는 동안 새 장전
        {
            ContinueDrag();
        }
        else if (Input.GetMouseButton(0) && isPanning) // 마우스 왼쪽 버튼을 누르고 있는 동안 화면 이동
        {
            ContinuePan();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging) // 마우스 왼쪽 버튼을 땠을 때
        {
            ReleaseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isPanning) // 마우스 왼쪽 버튼을 땠을 때 다시 새한테로
        {
            isPanning = false;
            cameraFollow.EnableFollowTarget(true);
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
            // 화면 조작
            isPanning = true;
            lastPanPosition = Input.mousePosition;
            cameraFollow.EnableFollowTarget(false); // 화면 이동할 때는 카메라 따라가기 비활성화
        }
    }

    void ContinueDrag()
    {
        Vector3 currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentPoint.z = 0;

        Vector3 launchDir = startPoint - currentPoint;
        Vector3 launchVel = launchDir * launchForceMultiplier;
        
        Debug.Log($"Start Point: {startPoint}, Current Point: {currentPoint}, Launch Velocity: {launchVel}"); // 디버그 로그 추가

        UpdateTrajectory(bird.position, launchVel);
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
        
        StartCoroutine(WaitReloadBird());

        cameraFollow.FollowTarget(bird);
        cameraFollow.EnableFollowTarget(true); // 발사할 때는 카메라 따라가기 활성화
        _renderer.positionCount = 0; // 궤적 초기화
    }

    void ContinuePan()
    {
        Vector3 currentPanPosition = Input.mousePosition;
        Vector3 difference = Camera.main.ScreenToWorldPoint(lastPanPosition) - Camera.main.ScreenToWorldPoint(currentPanPosition);

        Camera.main.transform.position += difference;
        lastPanPosition = currentPanPosition;
    }
    
    IEnumerator WaitReloadBird()
    {
        yield return new WaitForSeconds(respawnTime);
        canLaunch = true;
        SpawnBird();
        Debug.Log("발사 준비 완료");
    }

    void SpawnBird()
    {
        bird = Instantiate(birdPrefab, slingshot.position, Quaternion.identity).transform;
        var vector3 = bird.position;
        vector3.y = -1.23f;
        bird.position = vector3;
        bird.GetComponent<Rigidbody2D>().isKinematic = true;
        CameraFollow.instance.FollowTarget(bird);
    }

    // 궤적 생성
    private void UpdateTrajectory(Vector3 startPos, Vector3 launchVelocity)
    {
        int resolution = 30; // 궤적의 점 개수
        float timeStep = 0.1f; // 각 점 사이의 시간 간격
        Vector3 gravity = new Vector3(Physics2D.gravity.x, Physics2D.gravity.y, 0); // 2D 중력을 3D 벡터로 변환

        trajectoryPoints.Clear();
        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            Vector3 position = startPos + launchVelocity * t + 0.5f * gravity * t * t;

            // 특정 레이아웃 충돌 검사
            RaycastHit2D hit = Physics2D.Raycast(startPos, position - startPos, (position - startPos).magnitude, collisionMask);
            if (hit.collider != null)
            {
                position = hit.point;
                trajectoryPoints.Add(position);
                break;
            }

            trajectoryPoints.Add(position);
            startPos = position;
        }

        _renderer.positionCount = trajectoryPoints.Count;
        _renderer.SetPositions(trajectoryPoints.ToArray());
    }
}
