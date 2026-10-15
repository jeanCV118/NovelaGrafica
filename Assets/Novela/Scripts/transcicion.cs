using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class transcicion : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.25f;

    
    public IEnumerator FadeOut()
    {
        yield return Fade(0f, 1f);
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;

        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);

            fadeImage.color = color;

            time += Time.deltaTime;

            yield return null;
        }

        color.a = endAlpha;

        fadeImage.color = color;
    }
}