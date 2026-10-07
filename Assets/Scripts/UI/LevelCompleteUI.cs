using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TractorRacing
{
    public class LevelCompleteUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        public Canvas levelCompleteCanvas;

        [SerializeField]
        public Text levelNameText;

        [SerializeField]
        public Text timeText;

        [SerializeField]
        public Text bestTimeText;

        [SerializeField]
        public Button nextLevelButton;

        [SerializeField]
        public Button levelSelectButton;

        [SerializeField]
        public Button mainMenuButton;

        private GameManager gameManager;
        private LevelManager levelManager;
        private bool isShown = false;
        private float displayTimeRemaining = 0f;

        private void Awake()
        {
            if (levelCompleteCanvas == null)
            {
                levelCompleteCanvas = GetComponent<Canvas>();
            }
        }

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            levelManager = FindObjectOfType<LevelManager>();

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
            }

            if (levelSelectButton != null)
            {
                levelSelectButton.onClick.RemoveAllListeners();
                levelSelectButton.onClick.AddListener(OnLevelSelectButtonClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }
        }

        public void InitializeUI()
        {
            if (levelManager == null) levelManager = FindObjectOfType<LevelManager>();
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();

            displayTimeRemaining = (gameManager != null) ? gameManager.TimeRemaining : 60f;

            if (levelNameText != null && levelManager != null)
            {
                levelNameText.text = "Level " + levelManager.CurrentLevelIndex + " Completed!";
            }

            if (timeText != null)
            {
                timeText.text = "Time Left: " + displayTimeRemaining.ToString("F1") + "s";
            }

            if (bestTimeText != null && levelManager != null)
            {
                bestTimeText.text = "Best: " + levelManager.GetBestTime(levelManager.CurrentLevelIndex).ToString("F1") + "s";
            }
        }

        public void ShowLevelComplete(bool show)
        {
            isShown = show;
            gameObject.SetActive(show);

            if (levelCompleteCanvas != null)
            {
                levelCompleteCanvas.gameObject.SetActive(show);
                levelCompleteCanvas.enabled = show;
            }

            InitializeUI();
        }

        private void OnGUI()
        {
            if (!isShown) return;

            // Draw victory overlay popup in center of screen
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);

            float panelW = 480f;
            float panelH = 340f;
            float px = (Screen.width - panelW) * 0.5f;
            float py = (Screen.height - panelH) * 0.5f;

            GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
            GUI.Box(new Rect(px, py, panelW, panelH), GUIContent.none, panelStyle);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.2f, 1.0f, 0.3f);
            GUI.Label(new Rect(px, py + 20f, panelW, 45f), "🏆 LEVEL COMPLETE! 🏆", titleStyle);

            GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            infoStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(px, py + 75f, panelW, 30f), $"Time Left: {displayTimeRemaining:F1}s", infoStyle);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            btnStyle.normal.textColor = Color.white;

            float btnW = 320f;
            float btnH = 46f;
            float bx = (Screen.width - btnW) * 0.5f;

            if (GUI.Button(new Rect(bx, py + 125f, btnW, btnH), "▶  NEXT LEVEL", btnStyle))
            {
                OnNextLevelButtonClicked();
            }

            if (GUI.Button(new Rect(bx, py + 185f, btnW, btnH), "🏁  LEVEL SELECT", btnStyle))
            {
                OnLevelSelectButtonClicked();
            }

            if (GUI.Button(new Rect(bx, py + 245f, btnW, btnH), "⮌  MAIN MENU", btnStyle))
            {
                OnMainMenuButtonClicked();
            }
        }

        public void OnNextLevelButtonClicked()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.PlayButtonClick();

            if (levelManager == null) levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
            {
                int next = levelManager.CurrentLevelIndex + 1;
                if (next <= 5)
                {
                    levelManager.LoadLevel(next);
                    return;
                }
            }

            // Fallback: check current scene name
            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene.StartsWith("Level") && int.TryParse(activeScene.Substring(5), out int lvlNum))
            {
                int nextLvl = (lvlNum < 5) ? lvlNum + 1 : 1;
                SceneManager.LoadScene("Level" + nextLvl);
            }
            else
            {
                SceneManager.LoadScene("LevelSelect");
            }
        }

        public void OnLevelSelectButtonClicked()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene("LevelSelect");
        }

        public void OnMainMenuButtonClicked()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
