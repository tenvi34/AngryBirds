using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingshotController : MonoBehaviour
{
    public GameObject birdPrefab;
    public Transform launchPoint;

    void Start()
    {
        CreateBird();
    }

    void CreateBird()
    {
        GameObject bird = Instantiate(birdPrefab, launchPoint.position, Quaternion.identity);
        BirdController birdController = bird.GetComponent<BirdController>();
        //birdController.SetInitialPosition(launchPoint.position);
        //Camera.main.GetComponent<CameraController>().SetTarget(bird.transform); // 새가 생성될 때 카메라 타겟 설정
    }

    public void OnBirdDestroyed()
    {
        // 2초 후 새로운 새 생성
        Invoke("CreateBird", 2f);
    }
}
