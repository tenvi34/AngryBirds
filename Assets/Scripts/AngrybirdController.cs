using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngrybirdController : MonoBehaviour
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
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼을 눌렀을 때
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging) // 마우스 왼쪽 버튼을 누르고 있는 동안
        {
            ContinueDrag();
        }
        else if (Input.GetMouseButton(0) && isPanning) // 마우스 왼쪽 버튼을 누르고 있는 동안 팬
        {
            ContinuePan();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging) // 마우스 왼쪽 버튼을 뗐을 때
        {
            ReleaseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isPanning) // 마우스 왼쪽 버튼을 뗐을 때 팬 해제
        {
            isPanning = false;
        }
    }

    void StartDrag()
    {
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPoint.z = 0; // 2D 게임 -> z 좌표를 0으로 설정

        if (canLaunch && Vector3.Distance(startPoint, bird.position) < 1f) // 앵그리버드 근처에서 드래그 시작
        {
            isDragging = true; 
            //trajectory.ClearLine();
        }
        else if (!canLaunch)
        {
            Debug.Log("장전 중");
        }
        else // 마우스로 카메라 조작
        {
            isPanning = true;
            lastPanPosition = Input.mousePosition;
        }
    }

    void ContinueDrag()
    {
        Vector3 currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentPoint.z = 0;

        Vector3[] points = CalculateTrajectoryPoints(bird.position, (startPoint - currentPoint) * launchForceMultiplier, 50);
        //trajectory.RenderLine(bird.position, points); // 궤적을 렌더링
    }

    void ReleaseDrag()
    {
        isDragging = false;
        Vector3 launchDirection = startPoint - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        launchDirection.z = 0;

        Rigidbody2D birdRigidbody = bird.GetComponent<Rigidbody2D>();
        birdRigidbody.isKinematic = false; // isKinematic 해제

        birdRigidbody.AddForce(launchDirection * launchForceMultiplier, ForceMode2D.Impulse);
        //trajectory.ClearLine(); // 궤적을 초기화
        canLaunch = false; // 발사할 수 없게 설정
        StartCoroutine(ReloadBird()); // 재발사 대기 시간 시작

        CameraFollow.instance.FollowBird(bird); // 카메라가 새를 따라가도록 설정
    }

    void ContinuePan()
    {
        Vector3 currentPanPosition = Input.mousePosition;
        Vector3 difference = Camera.main.ScreenToWorldPoint(lastPanPosition) - Camera.main.ScreenToWorldPoint(currentPanPosition);

        Camera.main.transform.position += difference;
        lastPanPosition = currentPanPosition;
    }

    // 포물선(궤적) 계산 코드 ver.1
    Vector3[] CalculateTrajectoryPoints(Vector3 startPosition, Vector3 velocity, int numPoints)
    {
        Vector3[] points = new Vector3[numPoints];
        float timeStep = 0.1f; // 시간 간격
        Vector3 gravity = Physics2D.gravity; // 중력 벡터
        
        for (int i = 0; i < numPoints; i++)
        {
            float time = i * timeStep;
            points[i] = startPosition + velocity * time + 0.5f * gravity * time * time; // 물리 법칙을 이용한 궤적 계산
        }
        
        return points; // 계산된 궤적 점들을 반환
    }
    
    // 포물선(궤적) 계산 코드 ver.1.5
    // Vector3[] CalculateTrajectoryPoints(Vector3 startPosition, Vector3 velocity, int numPoints)
    // {
    //     List<Vector3> points = new List<Vector3>();
    //     float timeStep = 0.1f;  // 시간 간격
    //     Vector3 gravity = Physics2D.gravity;  // 중력 벡터
    //     Vector3 currentPosition = startPosition;
    //
    //     for (int i = 0; i < numPoints; i++)
    //     {
    //         float time = i * timeStep;
    //         Vector3 point = startPosition + velocity * time + 0.5f * gravity * time * time;  // 물리 법칙을 이용한 궤적 계산
    //         points.Add(point);
    //
    //         RaycastHit2D hit = Physics2D.Raycast(currentPosition, point - currentPosition, (point - currentPosition).magnitude);
    //         if (hit.collider != null)
    //         {
    //             points.Add(hit.point);
    //             break;
    //         }
    //
    //         currentPosition = point;
    //     }
    //     
    //     Debug.Log("궤적 로그");
    //
    //     return points.ToArray();  // 계산된 궤적 점들을 반환
    // }

    IEnumerator ReloadBird()
    {
        yield return new WaitForSeconds(reloadTime); // 재발사 대기 시간
        canLaunch = true; // 새를 발사할 수 있도록 설정
        SpawnBird();
        Debug.Log("발사 준비 완료");
    }

    void SpawnBird()
    {
        if (bird != null) // 발사 후 제거 
        {
            Destroy(bird.gameObject);
        }
        bird = Instantiate(birdPrefab, slingshot.position, Quaternion.identity);
        //bird.position = slingshot.position;
        var vector3 = bird.position;
        vector3.y = -5.41f;
        bird.position = vector3;
        bird.GetComponent<Rigidbody2D>().isKinematic = true; // 새를 움직이지 않도록 설정
        CameraFollow.instance.FollowBird(bird);
    }
}
