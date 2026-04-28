using System.Collections;
using UnityEngine;

public static class Utils {
    public static IEnumerator FreezeFrame(float duration) {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
    }
}
