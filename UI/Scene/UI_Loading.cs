using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class UI_Loading : MonoBehaviour
{
    static string nextScene;

    [SerializeField]
    Image progressBar;

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Loading");
    }

    private void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene); // 비동기로 불러옴
        op.allowSceneActivation = false; // 로딩 마치면 자동으로 넘어가지 않도록 + 씬 외에도 리소스들이 충분히 로드 되도록

        float timer = 0f;
        while (!op.isDone)
        {
            yield return null; // 반복문이 끝날때마다 유니티 엔진에 제어권을 넘겨야 바 게이지가 올라감

            if (op.progress < 0.2f)
            {
                progressBar.fillAmount = op.progress;
            }
            else // 페이크 로딩
            {
                timer += Time.unscaledDeltaTime / 5f;
                progressBar.fillAmount = Mathf.Lerp(0.2f, 1f, timer);
                if (progressBar.fillAmount >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}

