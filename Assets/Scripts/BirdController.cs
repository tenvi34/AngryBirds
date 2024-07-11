using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BirdController : MonoBehaviour
{
    public Transform slingshot; // 새총 오브젝트
    public Transform birdPrefab; // 발사할 새 프리팹
    public int birdCount = 3; // 새의 개수
    public float launchForceMultiplier = 10f; // 발사 힘의 크기를 조절하는 변수
    public float respawnTime = 3.0f; // 리스폰 대기 시간
    public float maxDragDistance = 2f; // 새를 당길 수 있는 최대 거리

    public Vector3 initialBirdPosition = new Vector3(-0.2f, 0.7f, 0); // 새의 초기 위치
    public Vector3 birdQueuePosition = new Vector3(-7, -0.7f, 0); // 새의 대기 위치
    public float birdSpacing = 2f; // 새들 간의 간격

    private List<Transform> birds = new List<Transform>();
    private int currentBirdIndex = 0; // 현재 장전된 새의 인덱스
    private Vector3 startPoint; // 마우스 드래그 시작 지점
    private bool isDragging = false; // 드래그 상태 확인
    private bool isPanning = false; // 화면 이동 상태 확인
    private bool canLaunch = true; // 발사 가능 확인
    private Vector3 lastPanPosition; // 마지막 화면 위치

    private CameraFollow cameraFollow;

    public AudioSource shotAudioSource; // 발사할 때 재생
    public AudioSource collisionAudioSource; // 충돌할 때 재생

    // 라인렌더러
    public LineRenderer _renderer;
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    public LayerMask collisionMask; // 설정한 레이아웃에만 충돌 감지

    public GameObject gameWinPanel; // 게임 승리 패널
    public GameObject gameWinImage; // 게임 승리 이미지
    public Button retryButton; // 다시하기 버튼
    public Button mainMenuButton; // 메인 메뉴 버튼
    public Button nextLevelButton; // 다음 레벨 버튼
    public GameObject gameOverImage; // 게임 오버 이미지
    public float gameOverAnimationDuration = 2f; // 게임 오버 이미지가 커지는 데 걸리는 시간

    void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        shotAudioSource = GetComponent<AudioSource>();
        
        SpawnBirds();

        // LineRenderer 설정 -> 임시 테스트 코드라 추후 Inspector 창에서 재설정 할 예정
        _renderer.startWidth = 0.1f;
        _renderer.endWidth = 0.1f;
        _renderer.material = new Material(Shader.Find("Sprites/Default"));
        _renderer.startColor = Color.red;
        _renderer.endColor = Color.yellow;

        // GameOver, GameWin 패널 초기 상태 비활성화
        gameOverImage.SetActive(false);
        gameWinPanel.SetActive(false);

        // 버튼 클릭 이벤트 등록
        retryButton.onClick.AddListener(OnRetryButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 좌클릭을 눌렀을 때
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging) // 마우스 좌클릭을 누르고 있는 동안 새 장전
        {
            ContinueDrag(); // 조준
        }
        else if (Input.GetMouseButton(0) && isPanning) // 마우스 좌클릭을 누르고 있는 동안 화면 이동
        {
            ContinuePan();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging) // 마우스 좌클릭을 땠을 때
        {
            ReleaseDrag(); // 발사
        }
        else if (Input.GetMouseButtonUp(0) && isPanning) // 마우스 좌클릭을 땠을 때 다시 새한테로
        {
            isPanning = false;
            cameraFollow.EnableFollowTarget(true);
        }

        if (!isDragging && canLaunch)
        {
            birds[currentBirdIndex].position = slingshot.position + initialBirdPosition; // 새를 새총 위치 위에 고정
        }

        // 매 프레임마다 돼지의 상태를 확인
        CheckGameOver_NextScene();
    }

    void StartDrag()
    {
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPoint.z = 0;

        if (canLaunch && Vector3.Distance(startPoint, birds[currentBirdIndex].position) < 1f)
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

        Vector3 dragVector = currentPoint - slingshot.position;
        if (dragVector.magnitude > maxDragDistance)
        {
            dragVector = dragVector.normalized * maxDragDistance;
        }

        Vector3 launchDir = slingshot.position - (slingshot.position + dragVector);
        Vector3 launchVel = launchDir * launchForceMultiplier;

        birds[currentBirdIndex].position = slingshot.position + dragVector; // 새의 위치를 제한된 마우스 위치로 업데이트
        UpdateTrajectory(birds[currentBirdIndex].position, launchVel);
    }

    void ReleaseDrag()
    {
        isDragging = false;
        Vector3 releasePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        releasePoint.z = 0;
        Vector3 launchDirection = slingshot.position - releasePoint;

        Rigidbody2D birdRigidbody = birds[currentBirdIndex].GetComponent<Rigidbody2D>();
        birdRigidbody.isKinematic = false;
        birdRigidbody.AddForce(launchDirection * launchForceMultiplier, ForceMode2D.Impulse);
        canLaunch = false;
        // Debug.Log("날아가는 중");
        
        if (shotAudioSource != null)
        {
            shotAudioSource.Play();
            // Debug.Log("소리 재생");
        }

        StartCoroutine(WaitReloadBird());

        cameraFollow.FollowTarget(birds[currentBirdIndex]);
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

    // 리스폰 대기
    IEnumerator WaitReloadBird()
    {
        yield return new WaitForSeconds(respawnTime);
        canLaunch = true;
        if (currentBirdIndex < birds.Count - 1)
        {
            currentBirdIndex++;
            cameraFollow.FollowTarget(birds[currentBirdIndex]); // 새로운 새가 장전될 때 카메라가 따라가도록 설정
            cameraFollow.EnableFollowTarget(true);
        }
        else // 새의 숫자만큼 발사가 끝났을 때
        {
            Debug.Log("모든 새 발사 완료");
            canLaunch = false;
            cameraFollow.EnableFollowTarget(false);
        
            // 일정 시간 대기 후 게임 오버 조건 체크
            StartCoroutine(CheckGameOverAfterDelay(5.0f)); // 5초 대기 후 체크
        }
        Debug.Log("발사 준비 완료");
    }

    IEnumerator CheckGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        bool checkAllPig = GameManager.Instance.CheckAllPigDestroyed();

        if (!checkAllPig)
        {
            Debug.Log("돼지 승리, 게임 오버");
            StartCoroutine(ShowGameOver());
        }
    }



    // 게임 오버 화면을 점점 키우는 코루틴
    IEnumerator ShowGameOver()
    {
        RectTransform rectTransform = gameOverImage.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero; // 화면 중심으로 설정
        gameOverImage.SetActive(true);
        rectTransform.localScale = Vector3.zero; // 초기 크기를 0으로 설정

        float elapsedTime = 0f;
        while (elapsedTime < gameOverAnimationDuration)
        {
            rectTransform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(3f, 3f, 3f), elapsedTime / gameOverAnimationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 크기 설정
        rectTransform.localScale = new Vector3(3f, 3f, 3f);

        // 게임 오버 상태에서 게임을 멈춤
        Time.timeScale = 0f;
        
        Debug.Log("게임 오버");
    }
    
    IEnumerator ShowGameWin()
    {
        // 1초 대기
        yield return new WaitForSeconds(1f);

        gameWinPanel.SetActive(true); // 게임 승리 패널 활성화

        // 게임 오버 상태에서 게임을 멈춤
        Time.timeScale = 0f;

        Debug.Log("게임 승리");
    }
    
    // IEnumerator ShowGameWin()
    // {
    //     // 1초 대기
    //     yield return new WaitForSeconds(1f);
    //
    //     gameWinPanel.SetActive(true); // 게임 승리 패널 활성화
    //     RectTransform rectTransform = gameWinImage.GetComponent<RectTransform>();
    //
    //     // 크기와 위치를 설정
    //     rectTransform.anchoredPosition = Vector2.zero; // 화면 중심으로 설정
    //     rectTransform.localScale = Vector3.one;
    //
    //     float elapsedTime = 0f;
    //     while (elapsedTime < gameOverAnimationDuration)
    //     {
    //         rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.01f, Vector3.one, elapsedTime / gameOverAnimationDuration);
    //         elapsedTime += Time.deltaTime;
    //         yield return null;
    //     }
    //
    //     // 최종 크기 설정
    //     rectTransform.localScale = Vector3.one * 10;
    //
    //     // 게임 오버 상태에서 게임을 멈춤
    //     Time.timeScale = 0f;
    //
    //     Debug.Log("게임 승리");
    // }

    void CheckGameOver_NextScene()
    {
        bool checkAllPig = GameManager.Instance.CheckAllPigDestroyed();

        if (checkAllPig)
        {
            Debug.Log("모든 돼지 제거 완료. 다음 스테이지로 이동");
            StartCoroutine(ShowGameWin());
        }
    }
    
    void OnRetryButtonClicked()
    {
        Time.timeScale = 1f; // 게임 시간 재개
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // 현재 Scene 다시
    }

    void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f; // 게임 시간 재개
        SceneManager.LoadScene("MainMenuScene");
    }

    void OnNextLevelButtonClicked()
    {
        Time.timeScale = 1f; // 게임 시간 재개
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // 다음 Scene으로 이동
    }

    // 리스폰
    void SpawnBirds()
    {
        for (int i = 0; i < birdCount; i++)
        {
            Vector3 spawnPosition = slingshot.position + birdQueuePosition + new Vector3(birdSpacing * i, 0, 0);
            Transform bird = Instantiate(birdPrefab, spawnPosition, Quaternion.identity).transform; // 새를 새총 뒤로 배치
            bird.GetComponent<Rigidbody2D>().isKinematic = true;
            birds.Add(bird);
        }

        currentBirdIndex = 0;
        cameraFollow.FollowTarget(birds[currentBirdIndex]);
    }

    // 궤적 생성
    private void UpdateTrajectory(Vector3 startPos, Vector3 launchVelocity)
    {
        int resolution = 30; // 궤적의 점 개수
        float timeStep = 0.1f; // 각 점 사이의 시간 간격
        Vector3 gravity = new Vector3(Physics2D.gravity.x, Physics2D.gravity.y, 0); // 2D gravity를 3D 벡터로 변환

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

    // 블럭과 충돌 시 오디오 재생
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionAudioSource != null)
        {
            collisionAudioSource.Play();
            // Debug.Log("블럭과 충돌");
        }
    }
}
