using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TractorRacing;

namespace TractorRacing
{
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Canvas levelSelectCanvas;

        [SerializeField]
        private Button backButton;

        [Header("Level Buttons")]
        [SerializeField]
        private Button level1Button;

        [SerializeField]
        private Button level2Button;

        [SerializeField]
        private Button level3Button;

        [SerializeField]
        private Button level4Button;

        [SerializeField]
        private Button level5Button;

        [Header("Level Info")]
        [SerializeField]
        private Text level1Info;

        [SerializeField]
        private Text level2Info;

        [SerializeField]
        private Text level3Info;

        [SerializeField]
        private Text level4Info;

        [SerializeField]
        private Text level5Info;

        private LevelManager levelManager;

        private void Awake()
        {
            if (levelSelectCanvas == null)
            {
                levelSelectCanvas = GetComponent<Canvas>();
            }

            if (backButton == null)
            {
                backButton = GetComponentInChildren<Button>();
            }
        }

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClicked);
            }

            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
            }

            SetupLevelButtons();
            UpdateLevelDisplay();
        }

        private void SetupLevelButtons()
        {
            if (level1Button != null)
            {
                level1Button.onClick.RemoveAllListeners();
                level1Button.onClick.AddListener(() => OnLevelSelected(1));
            }

            if (level2Button != null)
            {
                level2Button.onClick.RemoveAllListeners();
                level2Button.onClick.AddListener(() => OnLevelSelected(2));
            }

            if (level3Button != null)
            {
                level3Button.onClick.RemoveAllListeners();
                level3Button.onClick.AddListener(() => OnLevelSelected(3));
            }

            if (level4Button != null)
            {
                level4Button.onClick.RemoveAllListeners();
                level4Button.onClick.AddListener(() => OnLevelSelected(4));
            }

            if (level5Button != null)
            {
                level5Button.onClick.RemoveAllListeners();
                level5Button.onClick.AddListener(() => OnLevelSelected(5));
            }
        }

        private void UpdateLevelDisplay()
        {
            if (levelManager == null) return;

            int unlockedLevel = levelManager.UnlockedLevel;

            int[] levels = { 1, 2, 3, 4, 5 };
            Button[] buttons = { level1Button, level2Button, level3Button, level4Button, level5Button };
            Text[] infos = { level1Info, level2Info, level3Info, level4Info, level5Info };

            for (int i = 0; i < levels.Length; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].interactable = true;
                    infos[i].text = "Level " + levels[i] + " [OPEN]";
                }
            }
        }

        private void Update()
        {
            if (SafeInput.GetKeyDown(KeyCode.Alpha1) || SafeInput.GetKeyDown(KeyCode.Keypad1)) OnLevelSelected(1);
            else if (SafeInput.GetKeyDown(KeyCode.Alpha2) || SafeInput.GetKeyDown(KeyCode.Keypad2)) OnLevelSelected(2);
            else if (SafeInput.GetKeyDown(KeyCode.Alpha3) || SafeInput.GetKeyDown(KeyCode.Keypad3)) OnLevelSelected(3);
            else if (SafeInput.GetKeyDown(KeyCode.Alpha4) || SafeInput.GetKeyDown(KeyCode.Keypad4)) OnLevelSelected(4);
            else if (SafeInput.GetKeyDown(KeyCode.Alpha5) || SafeInput.GetKeyDown(KeyCode.Keypad5)) OnLevelSelected(5);
            else if (SafeInput.GetKeyDown(KeyCode.Escape) || SafeInput.GetKeyDown(KeyCode.Backspace)) OnBackButtonClicked();
        }

        private void OnGUI()
        {
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            btnStyle.normal.textColor = Color.white;

            float w = 380f;
            float h = 42f;
            float cx = (Screen.width - w) * 0.5f;
            float sy = 120f;

            string[] names = { "Level 1: Village Road", "Level 2: Mud Track", "Level 3: Mountain Road", "Level 4: Obstacle Track", "Level 5: Tractor Challenge" };

            for (int i = 1; i <= 5; i++)
            {
                string label = $"▶  {names[i - 1]}";

                if (GUI.Button(new Rect(cx, sy + (i - 1) * 52f, w, h), label, btnStyle))
                {
                    OnLevelSelected(i);
                }
            }

            if (GUI.Button(new Rect(cx, sy + 5 * 52f + 15f, w, 40f), "⮌  BACK TO MAIN MENU", btnStyle))
            {
                OnBackButtonClicked();
            }
        }

        public void OnLevelSelected(int levelNumber)
        {
            AudioManager.Instance?.PlayButtonClick();
            if (levelManager != null)
            {
                levelManager.LoadLevel(levelNumber);
            }
            else
            {
                SceneManager.LoadScene("Level" + levelNumber);
            }
        }

        public void OnBackButtonClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene("MainMenu");
            Time.timeScale = 1f;
        }
    }
}
