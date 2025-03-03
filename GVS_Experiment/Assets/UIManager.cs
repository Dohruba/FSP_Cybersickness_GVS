using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text timerText;
    //[SerializeField] private Transform cameraTransform;

    private int coinCount = 0;
    private float startTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        startTime = Time.time;
    }

    public void AddCoin()
    {
        coinCount++;
        coinText.text = $" {coinCount}";
    }
    public void UpdateCoinText(int count)
    {
        coinCount = count;
        coinText.text = $" {count}";
    }
    public void UpdateTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
