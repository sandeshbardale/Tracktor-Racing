using UnityEngine;
using UnityEngine.UI;
using TractorRacing;

namespace TractorRacing
{
    public class RaceTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField]
        private float timeLimit = 120f;

        [SerializeField]
        private float initialTime = 120f;

        [Header("UI References")]
        [SerializeField]
        private Text timerText;

        private GameManager gameManager;
        private float currentTime;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();

            if (timerText == null)
            {
                timerText = GetComponent<Text>();
                if (timerText == null)
                {
                    Transform t = transform.Find("TimerText");
                    if (t != null) timerText = t.GetComponent<Text>();
                }
            }
        }

        private void Start()
        {
            currentTime = initialTime;
        }

        private void Update()
        {
            if (gameManager != null && gameManager.CurrentState == GameState.Playing)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60);
                float seconds = currentTime % 60;
                timerText.text = string.Format("{0:00}:{1:00}", minutes, Mathf.FloorToInt(seconds));
            }
        }

        public float GetTimeRemaining()
        {
            return currentTime;
        }

        public float GetTimeLimit()
        {
            return timeLimit;
        }

        public void ResetTimer()
        {
            currentTime = timeLimit;
            UpdateTimerDisplay();
        }

        public bool IsTimeUp()
        {
            return currentTime <= 0f;
        }
    }
}
