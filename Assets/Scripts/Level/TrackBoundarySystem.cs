using UnityEngine;

namespace TractorRacing
{
    public class TrackBoundarySystem : MonoBehaviour
    {
        [Header("Track Boundary Limits")]
        [SerializeField] public float maxRoadHalfWidth = 5.2f;
        [SerializeField] public float maxOffTrackSeconds = 3.5f;

        private TractorPhysics tractor;
        private float offTrackTimer = 0f;
        private bool isOffTrack = false;
        private float lastBeepTime = 0f;

        public bool IsOffTrack => isOffTrack;
        public float RemainingOffTrackTime => Mathf.Max(0f, maxOffTrackSeconds - offTrackTimer);

        private void Awake()
        {
            tractor = GetComponent<TractorPhysics>() ?? FindObjectOfType<TractorPhysics>();
        }

        private void Update()
        {
            if (tractor == null) tractor = FindObjectOfType<TractorPhysics>();
            if (tractor == null) return;

            float currentX = Mathf.Abs(tractor.transform.position.x);

            if (currentX > maxRoadHalfWidth)
            {
                isOffTrack = true;
                offTrackTimer += Time.deltaTime;

                // Play warning sound periodically
                if (Time.time - lastBeepTime > 0.8f)
                {
                    AudioManager.Instance?.PlayCountdownBeep(false);
                    lastBeepTime = Time.time;
                }

                // If off track too long, auto-respawn onto road centerline
                if (offTrackTimer >= maxOffTrackSeconds)
                {
                    offTrackTimer = 0f;
                    isOffTrack = false;
                    tractor.QuickRespawnOnTrack();
                    Debug.Log("<color=#FF3333><b>[TrackBoundarySystem] Tractor auto-respawned after going off track!</b></color>");
                }
            }
            else
            {
                isOffTrack = false;
                offTrackTimer = 0f;
            }
        }

        private void OnGUI()
        {
            if (!isOffTrack) return;

            // Flashing warning banner
            float alpha = Mathf.PingPong(Time.time * 4f, 1f);
            GUIStyle warnStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            warnStyle.normal.textColor = new Color(1.0f, 0.2f, 0.2f, Mathf.Lerp(0.6f, 1.0f, alpha));

            float boxW = 550f;
            float boxH = 80f;
            float bx = (Screen.width - boxW) * 0.5f;
            float by = 60f;

            GUI.Box(new Rect(bx, by, boxW, boxH), GUIContent.none);
            GUI.Label(new Rect(bx, by + 8f, boxW, 35f), "⚠️ WARNING: OFF TRACK! RETURN TO ROAD ⚠️", warnStyle);

            GUIStyle timerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            timerStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(bx, by + 42f, boxW, 30f), $"AUTO-RESETTING IN: {RemainingOffTrackTime:F1}s", timerStyle);
        }
    }
}
