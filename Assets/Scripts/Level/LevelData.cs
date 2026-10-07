using UnityEngine;

namespace TractorRacing
{
    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public string levelName;
        public string sceneName;
        public float timeLimit = 120f;
        public string difficulty = "Medium";
        public int checkpointCount = 3;
        public float bestTime = 9999f;
        public Vector3 startPosition;
        public Vector3 finishPosition;
    }
}
