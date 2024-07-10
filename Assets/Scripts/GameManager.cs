using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private List<MonoBehaviour> pigs = new List<MonoBehaviour>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPig(MonoBehaviour pig)
    {
        pigs.Add(pig);
        Debug.Log("돼지 추가됨. 현재 돼지 수: " + pigs.Count);
    }

    public void PigDestroyed(MonoBehaviour pig)
    {
        pigs.Remove(pig);
        Debug.Log("돼지 제거됨. 남은 돼지 수: " + pigs.Count);
        if (pigs.Count == 0)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        Debug.Log("모든 돼지가 제거되었습니다. 다음 씬으로 넘어갑니다.");
        // 실제 씬 전환은 주석 처리합니다.
        // int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // SceneManager.LoadScene(currentSceneIndex + 1);
    }
}