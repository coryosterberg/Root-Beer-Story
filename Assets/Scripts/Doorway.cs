using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Doorway : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string targetSceneName;

    [Header("Fade UI (CanvasGroup on a full-screen black Image)")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeOutTime = 0.25f;
    [SerializeField] private float fadeInTime = 0.25f;

    [Header("Hide camera settling")]
    [Tooltip("How many frames to stay black after the new scene loads.")]
    [SerializeField] private int holdBlackFramesAfterLoad = 2;

    [Header("Freeze")]
    [SerializeField] private Behaviour movementScriptToDisable; // drag your PlayerController here
    [SerializeField] private Rigidbody playerRbToFreeze;        // optional, drag player RB if you want

    private bool used;

    private void Awake()
    {
        if (fadeGroup)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        StartCoroutine(Transition(other.gameObject));
    }

    private IEnumerator Transition(GameObject player)
    {
        // Freeze movement immediately
        if (movementScriptToDisable) movementScriptToDisable.enabled = false;

        if (playerRbToFreeze)
        {
            playerRbToFreeze.linearVelocity = Vector3.zero;
            playerRbToFreeze.angularVelocity = Vector3.zero;
            playerRbToFreeze.isKinematic = true;
        }

        // Fade to black
        yield return FadeTo(1f, fadeOutTime);

        // Load new scene (still black)
        yield return SceneManager.LoadSceneAsync(targetSceneName);

        // IMPORTANT: the new scene needs a fade canvas too.
        // After load, find the fade canvas in the new scene and use it.
        yield return null; // allow objects to Awake/OnEnable
        RebindFadeGroupFromNewSceneIfNeeded();

        // Keep it black while camera/player snaps into place
        for (int i = 0; i < holdBlackFramesAfterLoad; i++)
            yield return null;

        // Fade in
        yield return FadeTo(0f, fadeInTime);

        // Unfreeze (optional; often you’ll unfreeze in your spawn/camera setup instead)
        if (playerRbToFreeze) playerRbToFreeze.isKinematic = false;
        if (movementScriptToDisable) movementScriptToDisable.enabled = true;
    }

    private void RebindFadeGroupFromNewSceneIfNeeded()
    {
        // If your fadeGroup reference got destroyed on scene load,
        // this will grab the new scene's FadeImage CanvasGroup.
        if (fadeGroup != null) return;

        // Find ANY CanvasGroup named "FadeImage" (recommended naming)
        var fadeObj = GameObject.Find("FadeImage");
        if (fadeObj != null) fadeGroup = fadeObj.GetComponent<CanvasGroup>();

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f; // ensure we stay black
            fadeGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogWarning("No FadeImage CanvasGroup found in new scene. Add a FadeImage with CanvasGroup.");
        }
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (!fadeGroup || duration <= 0f)
        {
            if (fadeGroup) fadeGroup.alpha = targetAlpha;
            yield break;
        }

        float start = fadeGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }
}
