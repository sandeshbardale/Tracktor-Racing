using UnityEngine;

namespace TractorRacing
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Save Settings")]
        [SerializeField]
        private bool saveOnExit = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            if (saveOnExit)
            {
                SaveAll();
            }
        }

        public void SaveGame()
        {
            PlayerPrefs.SetInt("BhootiyaRacing_UnlockedLevel", GetUnlockedLevel());
            PlayerPrefs.Save();
        }

        public void LoadGame()
        {
            int unlockedLevel = PlayerPrefs.GetInt("BhootiyaRacing_UnlockedLevel", 1);
            SetUnlockedLevel(unlockedLevel);
        }

        public int GetUnlockedLevel()
        {
            return PlayerPrefs.GetInt("BhootiyaRacing_UnlockedLevel", 1);
        }

        public void SetUnlockedLevel(int level)
        {
            PlayerPrefs.SetInt("BhootiyaRacing_UnlockedLevel", level);
            PlayerPrefs.Save();
        }

        public float GetBestTime(int levelNumber)
        {
            return PlayerPrefs.GetFloat("BhootiyaRacing_BestTime_" + levelNumber, 9999f);
        }

        public void SetBestTime(int levelNumber, float time)
        {
            if (time < GetBestTime(levelNumber))
            {
                PlayerPrefs.SetFloat("BhootiyaRacing_BestTime_" + levelNumber, time);
                PlayerPrefs.Save();
            }
        }

        public void SaveAll()
        {
            for (int i = 1; i <= 5; i++)
            {
                float bestTime = GetBestTime(i);
                PlayerPrefs.SetFloat("BhootiyaRacing_BestTime_" + i, bestTime);
            }
            PlayerPrefs.Save();
        }
    }
}
