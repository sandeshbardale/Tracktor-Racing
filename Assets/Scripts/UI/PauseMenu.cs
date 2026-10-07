using UnityEngine;
using UnityEngine.UI;
using TractorRacing;

namespace TractorRacing
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Canvas pauseCanvas;

        [SerializeField]
        private Button resumeButton;

        [SerializeField]
        private Button restartButton;

        [SerializeField]
        private Button levelSelectButton;

        [SerializeField]
        private Button mainMenuButton;

        private GameManager gameManager;

        private void Awake()
        {
            if (pauseCanvas == null)
            {
                pauseCanvas = GetComponent<Canvas>();
            }

            if (resumeButton == null)
            {
                resumeButton = GetComponentInChildren<Button>();
            }
        }

        private void Start()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (levelSelectButton != null)
            {
                levelSelectButton.onClick.AddListener(OnLevelSelectButtonClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }

            gameManager = FindObjectOfType<GameManager>();
        }

        public void OnResumeButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.TogglePause();
            }
        }

        public void OnRestartButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartLevel();
            }
        }

        public void OnLevelSelectButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.SelectLevel(1);
            }
        }

        public void OnMainMenuButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.MainMenu();
            }
        }

        public void ShowPauseMenu(bool show)
        {
            if (pauseCanvas != null)
            {
                pauseCanvas.enabled = show;
            }
        }
    }
}
