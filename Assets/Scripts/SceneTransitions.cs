using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitions : MonoBehaviour
{
    public static SceneTransitions I;

    [Header("Fade UI (optional)")]
    [SerializeField] private CanvasGroup fadeGroup; // assign if you want fading
    [SerializeField] private float fadeTime = 0.25f;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        yield return Fade(1f);

        yield return SceneManager.LoadSceneAsync(sceneName);

        // allow one frame so the new scene renders before fading in
        yield return null;

        yield return Fade(0f);
    }

    private IEnumerator Fade(float target)
    {
        if (fadeGroup == null) yield break;

        fadeGroup.blocksRaycasts = true;

        float start = fadeGroup.alpha;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }

        fadeGroup.alpha = target;
        fadeGroup.blocksRaycasts = (target > 0.001f);
    }
}
