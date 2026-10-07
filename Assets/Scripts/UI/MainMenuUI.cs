using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TractorRacing;

namespace TractorRacing
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Canvas mainMenuCanvas;

        [SerializeField]
        private Button playButton;

        [SerializeField]
        private Button levelSelectButton;

        [SerializeField]
        private Button exitButton;

        // Custom GUI style for fail-safe OnGUI rendering
        private GUIStyle guiBtnStyle;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle hintStyle;
        private bool guiStylesReady = false;

        private void Awake()
        {
            if (mainMenuCanvas == null)
            {
                mainMenuCanvas = GetComponent<Canvas>();
            }

            EnsureEventSystem();
            EnsureAudioListener();
            FindButtonsIfNull();
        }

        private void EnsureEventSystem()
        {
            EventSystem es = FindObjectOfType<EventSystem>();
            if (es == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                es = esObj.AddComponent<EventSystem>();
            }

            Type inputSystemUIModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemUIModuleType != null)
            {
                StandaloneInputModule legacyModule = es.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    DestroyImmediate(legacyModule);
                }

                if (es.GetComponent(inputSystemUIModuleType) == null)
                {
                    es.gameObject.AddComponent(inputSystemUIModuleType);
                }
            }
            else
            {
                if (es.GetComponent<BaseInputModule>() == null)
                {
                    es.gameObject.AddComponent<StandaloneInputModule>();
                }
            }
        }

        private void EnsureAudioListener()
        {
            if (FindObjectOfType<AudioListener>() == null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    GameObject listenerObj = new GameObject("AudioListener");
                    listenerObj.AddComponent<AudioListener>();
                }
            }
        }

        private void FindButtonsIfNull()
        {
            if (playButton == null)
            {
                Transform playT = transform.Find("PlayButton");
                if (playT != null) playButton = playT.GetComponent<Button>();
            }

            if (levelSelectButton == null)
            {
                Transform lsT = transform.Find("LevelSelectButton");
                if (lsT != null) levelSelectButton = lsT.GetComponent<Button>();
            }

            if (exitButton == null)
            {
                Transform exitT = transform.Find("ExitButton");
                if (exitT != null) exitButton = exitT.GetComponent<Button>();
            }
        }

        private void Start()
        {
            FindButtonsIfNull();

            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }

            if (levelSelectButton != null)
            {
                levelSelectButton.onClick.RemoveAllListeners();
                levelSelectButton.onClick.AddListener(OnLevelSelectButtonClicked);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(OnExitButtonClicked);
            }

            Time.timeScale = 1f;
        }

        private void Update()
        {
            // Keyboard shortcuts to start or select tracks instantly
            if (SafeInput.GetKeyDown(KeyCode.Space) || SafeInput.GetKeyDown(KeyCode.Return) || SafeInput.GetKeyDown(KeyCode.KeypadEnter) || SafeInput.GetKeyDown(KeyCode.Alpha1))
            {
                OnPlayButtonClicked();
            }
            else if (SafeInput.GetKeyDown(KeyCode.Alpha2) || SafeInput.GetKeyDown(KeyCode.T) || SafeInput.GetKeyDown(KeyCode.L))
            {
                OnLevelSelectButtonClicked();
            }
            else if (SafeInput.GetKeyDown(KeyCode.Escape))
            {
                OnExitButtonClicked();
            }
        }

        private void InitGUIStyles()
        {
            if (guiStylesReady) return;

            guiBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            guiBtnStyle.normal.textColor = Color.white;
            guiBtnStyle.hover.textColor = Color.yellow;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 38,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.9f, 0.2f);

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            subtitleStyle.normal.textColor = Color.white;

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle.normal.textColor = new Color(0.8f, 0.9f, 1f, 0.9f);

            guiStylesReady = true;
        }

        private void OnGUI()
        {
            InitGUIStyles();

            float btnW = 280f;
            float btnH = 55f;
            float centerX = (Screen.width - btnW) * 0.5f;
            float startY = Screen.height * 0.45f;

            // Direct interactive Clickable Buttons via Immediate GUI
            if (GUI.Button(new Rect(centerX, startY, btnW, btnH), "▶  START GAME", guiBtnStyle))
            {
                OnPlayButtonClicked();
            }

            if (GUI.Button(new Rect(centerX, startY + 70f, btnW, 48f), "🏁  SELECT TRACK", guiBtnStyle))
            {
                OnLevelSelectButtonClicked();
            }

            if (GUI.Button(new Rect(centerX, startY + 130f, btnW, 44f), "✖  EXIT", guiBtnStyle))
            {
                OnExitButtonClicked();
            }

            // Controls & Quick-start hint
            GUI.Label(new Rect(0, Screen.height - 45f, Screen.width, 30f),
                "Press [SPACE] or [ENTER] or Click to Start | [2] Select Track | [ESC] Exit", hintStyle);
        }

        public void OnPlayButtonClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene("LevelSelect");
        }

        public void OnLevelSelectButtonClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene("LevelSelect");
        }

        public void OnExitButtonClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}


