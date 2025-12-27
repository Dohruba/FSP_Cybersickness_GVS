using UnityEngine;
using System.Collections.Generic;

public class LevelsManager : MonoBehaviour
{

    public List<GameObject> levels; 
    public Transform playerRig; 
    public Transform startingPoint; 

    [SerializeField] private int _currentLevelIndex = -1;
    [SerializeField] private float _timeRemaining;
    [SerializeField] private static int _coinsCollected;
    private GameObject _currentLevel;
    private int _totalCoinsInLevel;
    private int _initalTime = 60;

    [SerializeField]
    private DataRecorder _recorder;


    void Start()
    {
        StartNextLevel();
    }

    void Update()
    {
        if (_currentLevel == null) return;

        // Update timer
        _timeRemaining -= Time.deltaTime;
        UIManager.Instance.UpdateTimer(_timeRemaining);

        // Check for level end conditions
        if (_timeRemaining <= 0 || _coinsCollected >= _totalCoinsInLevel)
        {
            EndLevel();
        }
    }

    public void StartNextLevel()
    {
        _timeRemaining = _initalTime;
        if (_currentLevel != null) _currentLevel.SetActive(false);

        playerRig.position = startingPoint.position;
        playerRig.rotation = startingPoint.rotation;
        if (_currentLevelIndex >= levels.Count - 1)
        {
            return;
        }

        _currentLevelIndex++;
        _currentLevel = levels[_currentLevelIndex];
        _currentLevel.SetActive(true);


        _coinsCollected = 0;
        _totalCoinsInLevel = _currentLevel.GetComponentsInChildren<CoinController>(true).Length;
        _timeRemaining = 90f;
        if (_currentLevelIndex == 0) _timeRemaining = 1000;

        UIManager.Instance.UpdateCoinText(_coinsCollected);
    }


    void EndLevel()
    {
        string line = $"{_currentLevelIndex + 1}," +
                $"{_coinsCollected}/{_totalCoinsInLevel}," +
                $"{Mathf.FloorToInt(_timeRemaining)}";
        _recorder.RecordLevelMetrics(line);
        StartNextLevel();
    }
    public static void CollectCoin()
    {
        _coinsCollected++;
        UIManager.Instance.AddCoin();
    }
    public static void DespawnMe(GameObject disposable)
    {
        Destroy(disposable);
    }
}