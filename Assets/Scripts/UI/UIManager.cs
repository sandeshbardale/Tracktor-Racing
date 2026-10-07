using System.Collections;
using UnityEngine;
using BhootiyaRasta.Core;
using BhootiyaRasta.Vehicle;

namespace BhootiyaRasta.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Tractor Reference")]
        [SerializeField] private TractorController tractor;

        private bool showPause = false;
        private bool showLevelComplete = false;
        private bool showGameOver = false;
        private bool showLevelSelect = false;
        private bool showVictory = false;
        private string gameOverReason = "";
        private int completedLevelNumber = 1;

        // Jumpscare flash
        private float jumpscareFlashAlpha = 0f;
        private Texture2D jumpscareFlashTex;
        private Texture2D hudBgTex;
        private Texture2D fuelBarTex;
        private Texture2D bloodVignetteTex;

        // Custom GUI styles
        private GUIStyle titleStyle;
        private GUIStyle subTitleStyle;
        private GUIStyle hudStyle;
        private GUIStyle buttonStyle;
        private GUIStyle boxStyle;
        private bool stylesInitialized = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateTextures();
        }

        private void Start()
        {
            if (tractor == null) tractor = FindAnyObjectByType<TractorController>();
        }

        private void CreateTextures()
        {
            // Flash texture
            jumpscareFlashTex = new Texture2D(1, 1);
            jumpscareFlashTex.SetPixel(0, 0, new Color(0.9f, 0.05f, 0.05f, 0.75f));
            jumpscareFlashTex.Apply();

            // Dark semi-transparent HUD background
            hudBgTex = new Texture2D(1, 1);
            hudBgTex.SetPixel(0, 0, new Color(0.05f, 0.08f, 0.12f, 0.85f));
            hudBgTex.Apply();

            // Fuel bar texture
            fuelBarTex = new Texture2D(1, 1);
            fuelBarTex.SetPixel(0, 0, new Color(0.9f, 0.6f, 0.15f, 1f));
            fuelBarTex.Apply();

            // Blood/Horror Vignette
            bloodVignetteTex = new Texture2D(2, 2);
            Color darkBlood = new Color(0.4f, 0.02f, 0.02f, 0.45f);
            bloodVignetteTex.SetPixels(new Color[] { darkBlood, darkBlood, darkBlood, darkBlood });
            bloodVignetteTex.Apply();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.95f, 0.25f, 0.25f);

            subTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            subTitleStyle.normal.textColor = new Color(0.85f, 0.85f, 0.9f);

            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            hudStyle.normal.textColor = new Color(0.9f, 0.92f, 0.98f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle.normal.textColor = Color.white;

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperCenter
            };

            stylesInitialized = true;
        }

        private void Update()
        {
            if (jumpscareFlashAlpha > 0f)
            {
                jumpscareFlashAlpha -= Time.deltaTime * 1.8f;
            }
        }

        public void TriggerHorrorJumpscareFlash()
        {
            jumpscareFlashAlpha = 0.85f;
        }

        public void ShowPauseMenu(bool show)
        {
            showPause = show;
        }

        private float lastTimeRemaining = 60f;

        public void ShowLevelCompleteScreen(int level, float timeRemaining = 0f)
        {
            completedLevelNumber = level;
            lastTimeRemaining = timeRemaining;
            showLevelComplete = true;
            Time.timeScale = 0f;
        }

        public void ShowGameOverScreen(string reason)
        {
            gameOverReason = reason;
            showGameOver = true;
            Time.timeScale = 0f;
        }

        public void ShowFinalVictoryScreen(float timeRemaining = 0f)
        {
            lastTimeRemaining = timeRemaining;
            showVictory = true;
            Time.timeScale = 0f;
        }

        public void HideOverlays()
        {
            showPause = false;
            showLevelComplete = false;
            showGameOver = false;
            showLevelSelect = false;
            showVictory = false;
        }

        private void OnGUI()
        {
            InitStyles();

            // Render Jumpscare Flash Overlay
            if (jumpscareFlashAlpha > 0.01f)
            {
                Color prevCol = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, jumpscareFlashAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), jumpscareFlashTex);
                GUI.color = prevCol;
            }

            // Render Intro Sequence
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.IntroSequence)
            {
                DrawIntroSequence();
            }

            // Render in-game HUD when playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                DrawHUD();
            }

            // Pause Menu
            if (showPause)
            {
                DrawPauseMenu();
            }

            // Level Complete Screen
            if (showLevelComplete)
            {
                DrawLevelCompleteMenu();
            }

            // Game Over Screen
            if (showGameOver)
            {
                DrawGameOverMenu();
            }

            // Level Selection Screen
            if (showLevelSelect)
            {
                DrawLevelSelectMenu();
            }

            // Final Victory Ending
            if (showVictory)
            {
                DrawVictoryEnding();
            }
        }

        private void DrawIntroSequence()
        {
            // Dark vignette background
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), hudBgTex);

            float w = 620f;
            float h = 340f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

            GUI.Label(new Rect(rect.x, rect.y + 15, w, 40), "B H O O T I Y A   R A S T A", titleStyle);
            GUI.Label(new Rect(rect.x, rect.y + 60, w, 24), "— 60 Seconds. One Road. One Ghost. —", subTitleStyle);

            GUI.Label(new Rect(rect.x + 20, rect.y + 110, w - 40, 30),
                "<color=#FFF0A0><b>OBJECTIVE:</b> Reach the village destination before the ghost catches you.</color>", hudStyle);

            GUI.Label(new Rect(rect.x + 20, rect.y + 145, w - 40, 25),
                "<color=#FFA0A0><b>TIME LIMIT:</b> 60.0 SECONDS</color>", hudStyle);

            // Large Countdown Number
            string countText = GameManager.Instance != null ? GameManager.Instance.IntroCountdownText : "3";
            GUIStyle countStyle = new GUIStyle(titleStyle)
            {
                fontSize = 58,
                fontStyle = FontStyle.Bold
            };
            countStyle.normal.textColor = (countText == "GO!") ? new Color(0.2f, 1.0f, 0.4f) : new Color(1.0f, 0.3f, 0.3f);

            GUI.Label(new Rect(rect.x, rect.y + 200, w, 75), countText, countStyle);

            GUI.Label(new Rect(rect.x, rect.y + 290, w, 22), "Press [W] or [Up Arrow] to Accelerate | [V] to Look Behind", subTitleStyle);
        }

        private void DrawHUD()
        {
            // Level Title Bar at top center
            var currentLvlData = LevelManager.Instance != null ? LevelManager.Instance.GetCurrentLevelData() : null;
            string lvlName = currentLvlData != null ? currentLvlData.levelTitle : "Level 1";
            string lvlSub = currentLvlData != null ? currentLvlData.levelSubtitle : "The Farm Road";

            float topBarWidth = 460f;
            float topBarHeight = 90f;
            Rect topBarRect = new Rect((Screen.width - topBarWidth) * 0.5f, 12f, topBarWidth, topBarHeight);
            GUI.DrawTexture(topBarRect, hudBgTex);

            GUI.Label(new Rect(topBarRect.x, topBarRect.y + 4, topBarWidth, 22), "BH O O T I Y A   R A S T A", titleStyle);
            GUI.Label(new Rect(topBarRect.x, topBarRect.y + 26, topBarWidth, 18), $"{lvlName} — \"{lvlSub}\"", subTitleStyle);

            // 60-Second Real-Time Countdown Timer
            float timeRemaining = (GameManager.Instance != null) ? GameManager.Instance.TimeRemaining : 60f;
            Color timerColor = Color.white;
            string timerPrefix = "TIME:";

            if (timeRemaining <= 10f)
            {
                // Pulsing Red Alarm
                float pulse = Mathf.Sin(Time.time * 12f) * 0.5f + 0.5f;
                timerColor = Color.Lerp(new Color(1f, 0.15f, 0.15f), new Color(1f, 0.8f, 0.8f), pulse);
                timerPrefix = "⚠ CRITICAL TIME:";
            }
            else if (timeRemaining <= 20f)
            {
                timerColor = new Color(1f, 0.65f, 0.15f); // Amber warning
                timerPrefix = "TIME RUNNING OUT:";
            }

            GUIStyle timerStyle = new GUIStyle(hudStyle)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            timerStyle.normal.textColor = timerColor;

            GUI.Label(new Rect(topBarRect.x, topBarRect.y + 52, topBarWidth, 30), $"{timerPrefix} {timeRemaining:F1}s", timerStyle);

            // Dashboard HUD at bottom-left
            float dashWidth = 300f;
            float dashHeight = 155f;
            Rect dashRect = new Rect(20f, Screen.height - dashHeight - 20f, dashWidth, dashHeight);
            GUI.DrawTexture(dashRect, hudBgTex);

            float speed = (tractor != null) ? Mathf.Abs(tractor.CurrentSpeed) * 3.6f : 0f;
            float distRemaining = (CheckpointManager.Instance != null) ? CheckpointManager.Instance.DistanceRemaining : 0f;
            float ghostDist = (BhootiyaRasta.Horror.GhostChaseManager.Instance != null) ? BhootiyaRasta.Horror.GhostChaseManager.Instance.CurrentDistanceBehind : 40f;
            bool isDanger = (BhootiyaRasta.Horror.GhostChaseManager.Instance != null) && BhootiyaRasta.Horror.GhostChaseManager.Instance.IsDangerClose;
            bool lights = (tractor != null) && tractor.AreHeadlightsOn;

            GUI.Label(new Rect(dashRect.x + 14, dashRect.y + 10, dashWidth, 22), $"SPEED: {speed:F0} KM/H {(tractor != null && tractor.IsInMud ? "[MUD DRAG]" : "")}", hudStyle);
            GUI.Label(new Rect(dashRect.x + 14, dashRect.y + 34, dashWidth, 22), $"DESTINATION: {distRemaining:F0} M", hudStyle);

            // Ghost proximity indicator
            string ghostColorTag = isDanger ? "<color=#FF3333><b>" : "<color=#FFAA44>";
            string ghostEndTag = isDanger ? "</b></color> ⚠ LOOK BEHIND [V]!" : "</color>";
            GUI.Label(new Rect(dashRect.x + 14, dashRect.y + 58, dashWidth, 22), $"GHOST DISTANCE: {ghostColorTag}{ghostDist:F0} M{ghostEndTag}", hudStyle);

            GUI.Label(new Rect(dashRect.x + 14, dashRect.y + 82, dashWidth, 22), $"HEADLIGHTS: {(lights ? "<color=#A0FFA0>ON</color>" : "<color=#FFA0A0>OFF</color>")}", hudStyle);

            // Fuel Bar
            float fuel = (tractor != null) ? tractor.CurrentFuel : 100f;
            GUI.Label(new Rect(dashRect.x + 14, dashRect.y + 106, dashWidth, 20), "DIESEL FUEL:", hudStyle);
            Rect fuelBackRect = new Rect(dashRect.x + 14, dashRect.y + 128, 260f, 12f);
            GUI.Box(fuelBackRect, GUIContent.none);
            Rect fuelFillRect = new Rect(fuelBackRect.x, fuelBackRect.y, fuelBackRect.width * (fuel / 100f), fuelBackRect.height);
            GUI.DrawTexture(fuelFillRect, fuelBarTex);

            // Controls Hint at bottom-right
            float hintWidth = 290f;
            float hintHeight = 95f;
            Rect hintRect = new Rect(Screen.width - hintWidth - 20f, Screen.height - hintHeight - 20f, hintWidth, hintHeight);
            GUI.DrawTexture(hintRect, hudBgTex);

            GUI.Label(new Rect(hintRect.x + 10, hintRect.y + 6, hintWidth, 18), "<b>CONTROLS:</b>", hudStyle);
            GUI.Label(new Rect(hintRect.x + 10, hintRect.y + 26, hintWidth, 18), "• W / S / Arrows: Accelerate / Reverse", hudStyle);
            GUI.Label(new Rect(hintRect.x + 10, hintRect.y + 44, hintWidth, 18), "• A / D: Steer  |  Space: Brake", hudStyle);
            GUI.Label(new Rect(hintRect.x + 10, hintRect.y + 62, hintWidth, 18), "• <b>V: Look Behind</b>  |  H: Horn  |  F: Lights", hudStyle);
        }

        private void DrawPauseMenu()
        {
            float w = 360f;
            float h = 330f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.DrawTexture(rect, hudBgTex);

            GUI.Label(new Rect(rect.x, rect.y + 20, w, 30), "GAME PAUSED", titleStyle);

            if (GUI.Button(new Rect(rect.x + 40, rect.y + 80, w - 80, 42), "Resume", buttonStyle))
            {
                GameManager.Instance?.TogglePause();
            }

            if (GUI.Button(new Rect(rect.x + 40, rect.y + 138, w - 80, 42), "Restart Level", buttonStyle))
            {
                GameManager.Instance?.RestartCurrentLevel();
            }

            if (GUI.Button(new Rect(rect.x + 40, rect.y + 196, w - 80, 42), "Select Level", buttonStyle))
            {
                showPause = false;
                showLevelSelect = true;
            }

            if (GUI.Button(new Rect(rect.x + 40, rect.y + 254, w - 80, 42), "Quit Game", buttonStyle))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void DrawLevelCompleteMenu()
        {
            float w = 480f;
            float h = 320f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.DrawTexture(rect, hudBgTex);

            GUI.Label(new Rect(rect.x, rect.y + 20, w, 32), "YOU ESCAPED!", titleStyle);
            GUI.Label(new Rect(rect.x, rect.y + 60, w, 24), "You made it before it caught you.", subTitleStyle);
            GUI.Label(new Rect(rect.x, rect.y + 90, w, 24), $"<color=#66FFAA>TIME REMAINING: {lastTimeRemaining:F1} SECONDS</color>", hudStyle);

            if (GUI.Button(new Rect(rect.x + 60, rect.y + 140, w - 120, 45), "Next Level ➔", buttonStyle))
            {
                GameManager.Instance?.NextLevel();
            }

            if (GUI.Button(new Rect(rect.x + 60, rect.y + 198, w - 120, 40), "Replay Level", buttonStyle))
            {
                GameManager.Instance?.RestartCurrentLevel();
            }

            if (GUI.Button(new Rect(rect.x + 60, rect.y + 250, w - 120, 40), "Select Level", buttonStyle))
            {
                showLevelComplete = false;
                showLevelSelect = true;
            }
        }

        private void DrawGameOverMenu()
        {
            float w = 500f;
            float h = 340f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.DrawTexture(rect, hudBgTex);

            GUI.Label(new Rect(rect.x, rect.y + 20, w, 35), "GAME OVER", titleStyle);
            GUI.Label(new Rect(rect.x, rect.y + 60, w, 22), "\"The road didn't let you escape.\"", subTitleStyle);
            GUI.Label(new Rect(rect.x + 20, rect.y + 95, w - 40, 45), $"<color=#FFAAAA>{gameOverReason}</color>", subTitleStyle);

            if (GUI.Button(new Rect(rect.x + 60, rect.y + 160, w - 120, 45), "Try Again", buttonStyle))
            {
                GameManager.Instance?.RestartCurrentLevel();
            }

            if (GUI.Button(new Rect(rect.x + 60, rect.y + 220, w - 120, 40), "Select Level", buttonStyle))
            {
                showGameOver = false;
                showLevelSelect = true;
            }
        }

        private void DrawLevelSelectMenu()
        {
            float w = 520f;
            float h = 440f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.DrawTexture(rect, hudBgTex);

            GUI.Label(new Rect(rect.x, rect.y + 20, w, 30), "SELECT LEVEL — 60s CHALLENGE", titleStyle);

            int unlocked = (LevelManager.Instance != null) ? LevelManager.Instance.UnlockedLevel : 1;

            string[] names = new string[]
            {
                "Level 1: The Farm Road (Pehla Kadam)",
                "Level 2: The Fields (Sunsaan Khet)",
                "Level 3: The Abandoned Farm (Ujda Gaon)",
                "Level 4: The Forest Road (Ghanghor Jungle)",
                "Level 5: Bhootiya Rasta (Khooni Antim Mod)"
            };

            for (int i = 1; i <= 5; i++)
            {
                bool isUnlocked = (i <= unlocked);
                string label = isUnlocked ? names[i - 1] : $"{names[i - 1]} [LOCKED]";

                GUI.enabled = isUnlocked;
                if (GUI.Button(new Rect(rect.x + 40, rect.y + 65 + (i - 1) * 55, w - 80, 42), label, buttonStyle))
                {
                    GameManager.Instance?.SelectLevel(i);
                }
                GUI.enabled = true;
            }

            if (GUI.Button(new Rect(rect.x + 40, rect.y + 375, w - 80, 36), "Back", buttonStyle))
            {
                showLevelSelect = false;
                Time.timeScale = 1f;
            }
        }

        private void DrawVictoryEnding()
        {
            float w = 580f;
            float h = 380f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.DrawTexture(rect, hudBgTex);

            GUI.Label(new Rect(rect.x, rect.y + 25, w, 35), "YOU SURVIVED BHOOTIYA RASTA!", titleStyle);
            GUI.Label(new Rect(rect.x + 20, rect.y + 70, w - 40, 85),
                "Dawn breaks over the abandoned village.\nThe supernatural horror of Bhootiya Rasta fades into the morning mist.\nYour tractor brought you safely through the darkest night.\n\n<color=#66FFAA><b>Final Time Remaining: " + lastTimeRemaining.ToString("F1") + " Seconds</b></color>",
                subTitleStyle);

            if (GUI.Button(new Rect(rect.x + 100, rect.y + 200, w - 200, 45), "Play Again (Level 1)", buttonStyle))
            {
                GameManager.Instance?.SelectLevel(1);
            }

            if (GUI.Button(new Rect(rect.x + 100, rect.y + 260, w - 200, 40), "Select Level", buttonStyle))
            {
                showVictory = false;
                showLevelSelect = true;
            }
        }
    }
}
