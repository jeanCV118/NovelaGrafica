using System.Collections;
using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
    public static ScreenShaker Instance { get; private set; }

    [SerializeField] private RectTransform targetTransform;
    private Vector2 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (targetTransform == null)
        {
            targetTransform = GetComponent<RectTransform>();
        }

        if (targetTransform != null)
        {
            originalPosition = targetTransform.anchoredPosition;
        }
    }

    public void TriggerShake(float duration = 0.5f, float intensity = 15f)
    {
        if (targetTransform == null) return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            targetTransform.anchoredPosition = originalPosition;
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, intensity));
    }

    private IEnumerator ShakeRoutine(float duration, float intensity)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentIntensity = Mathf.Lerp(intensity, 0f, elapsed / duration);
            float offsetX = Random.Range(-1f, 1f) * currentIntensity;
            float offsetY = Random.Range(-1f, 1f) * currentIntensity;

            targetTransform.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);
            yield return null;
        }

        targetTransform.anchoredPosition = originalPosition;
        shakeCoroutine = null;
    }
}
