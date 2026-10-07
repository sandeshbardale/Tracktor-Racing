using System.Collections;
using UnityEngine;
using BhootiyaRasta.Audio;

namespace BhootiyaRasta.Horror
{
    public class GhostController : MonoBehaviour
    {
        [Header("Visual Parts")]
        public Renderer[] ghostRenderers;
        public Light eyeLightLeft;
        public Light eyeLightRight;
        public Transform ghostBody;
        public Transform headTransform;
        public ParticleSystem auraMist;

        [Header("Movement & Animation")]
        [SerializeField] private float floatSpeed = 2.5f;
        [SerializeField] private float bobFrequency = 2.2f;
        [SerializeField] private float bobHeight = 0.25f;
        [SerializeField] private float twitchInterval = 0.6f;

        private float twitchTimer = 0f;
        private Vector3 moveDirection = Vector3.zero;
        private bool isMoving = false;
        private Coroutine activeRoutine;

        private void Start()
        {
            if (auraMist != null && !auraMist.isPlaying)
            {
                auraMist.Play();
            }
        }

        private void Update()
        {
            // Floating & hovering bob
            if (ghostBody != null)
            {
                float yBob = Mathf.Sin(Time.time * bobFrequency) * bobHeight;
                ghostBody.localPosition = new Vector3(ghostBody.localPosition.x, yBob, ghostBody.localPosition.z);
            }

            // Uncanny head twitch
            twitchTimer += Time.deltaTime;
            if (twitchTimer > twitchInterval)
            {
                twitchTimer = 0f;
                if (headTransform != null)
                {
                    float twitchAngle = (Random.value > 0.5f ? 1f : -1f) * Random.Range(15f, 35f);
                    headTransform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(-20f, 20f), twitchAngle);
                }
            }

            // Movement if active
            if (isMoving)
            {
                transform.position += moveDirection * floatSpeed * Time.deltaTime;
            }
        }

        public void SpawnAndFadeIn(Vector3 position, Quaternion rotation, float duration = 1.0f)
        {
            transform.position = position;
            transform.rotation = rotation;
            gameObject.SetActive(true);
            isMoving = false;

            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(FadeRoutine(0f, 1f, duration));

            AudioManager.Instance?.PlayGhostWhisper(position);
        }

        public void GlideAcross(Vector3 startPos, Vector3 endPos, float speed)
        {
            transform.position = startPos;
            transform.rotation = Quaternion.LookRotation((endPos - startPos).normalized);
            gameObject.SetActive(true);
            moveDirection = (endPos - startPos).normalized;
            floatSpeed = speed;
            isMoving = true;

            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(GlideRoutine(startPos, endPos));
        }

        private IEnumerator GlideRoutine(Vector3 start, Vector3 end)
        {
            SetAlpha(1f);
            float totalDist = Vector3.Distance(start, end);
            while (Vector3.Distance(transform.position, start) < totalDist)
            {
                yield return null;
            }
            Vanish();
        }

        public void ApproachTractor(Transform tractorTransform, float duration = 3.5f)
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(ApproachRoutine(tractorTransform, duration));
        }

        private IEnumerator ApproachRoutine(Transform tractor, float duration)
        {
            SetAlpha(0.2f);
            float elapsed = 0f;
            while (elapsed < duration && tractor != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.LookAt(tractor.position);
                transform.position = Vector3.MoveTowards(transform.position, tractor.position, floatSpeed * Time.deltaTime);
                SetAlpha(Mathf.Lerp(0.2f, 0.95f, t));
                yield return null;
            }
            Vanish();
        }

        public void Vanish(float fadeTime = 0.5f)
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(VanishRoutine(fadeTime));
        }

        private IEnumerator VanishRoutine(float fadeTime)
        {
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(0f);
            isMoving = false;
            gameObject.SetActive(false);
        }

        private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration));
                yield return null;
            }
            SetAlpha(toAlpha);
        }

        private void SetAlpha(float alpha)
        {
            if (ghostRenderers != null)
            {
                foreach (var r in ghostRenderers)
                {
                    if (r != null && r.material != null)
                    {
                        Color c = r.material.color;
                        c.a = alpha;
                        r.material.color = c;
                    }
                }
            }

            if (eyeLightLeft != null) eyeLightLeft.intensity = alpha * 1.5f;
            if (eyeLightRight != null) eyeLightRight.intensity = alpha * 1.5f;
        }
    }
}
