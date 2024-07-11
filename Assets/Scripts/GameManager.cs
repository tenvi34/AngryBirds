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
        Debug.Log("돼지 추가. 현재 돼지 수: " + pigs.Count);
    }

    public void PigDestroyed(MonoBehaviour pig)
    {
        pigs.Remove(pig);
        Debug.Log("돼지 제거. 남은 돼지 수: " + pigs.Count);
        if (pigs.Count == 0)
        {
            LoadNextScene();
        }
    }

    public bool CheckAllPigDestroyed()
    {
        return pigs.Count == 0;
    }

    public void LoadNextScene()
    {
        // Debug.Log("모든 돼지 제거 완료. 다음 스테이지로 이동");
    }
}