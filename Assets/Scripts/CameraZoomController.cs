using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraZoomController : MonoBehaviour
{
    public Slider zoomSlider; // UI 슬라이더
    public Camera cameraToControl; // 조절할 카메라

    void Start()
    {
        // 슬라이더의 값이 변경될 때마다 OnZoomValueChanged 메소드를 호출하도록 설정
        zoomSlider.onValueChanged.AddListener(OnZoomValueChanged);
        // 초기 카메라 크기를 슬라이더의 현재 값으로 설정
        cameraToControl.orthographicSize = zoomSlider.value;
    }

    void OnZoomValueChanged(float value)
    {
        cameraToControl.orthographicSize = value;
    }
}
