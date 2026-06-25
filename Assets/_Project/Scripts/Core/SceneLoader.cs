using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OystermouthCastle.Core
{
    public sealed class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;

        public static event Action<string> SceneLoadStarted;
        public static event Action<string> SceneLoadCompleted;
        public static event Action<float> SceneLoadProgress;

        public static void LoadScene(string sceneName)
        {
            EnsureInstance().StartCoroutine(EnsureInstance().LoadSceneRoutine(sceneName));
        }

        private static SceneLoader EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            var loaderObject = new GameObject("SceneLoader");
            DontDestroyOnLoad(loaderObject);
            instance = loaderObject.AddComponent<SceneLoader>();
            return instance;
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("SceneLoader was asked to load an empty scene name.");
                yield break;
            }

            SceneLoadStarted?.Invoke(sceneName);
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError($"Scene '{sceneName}' could not be loaded. Check Build Settings.");
                yield break;
            }

            while (!operation.isDone)
            {
                SceneLoadProgress?.Invoke(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            SceneLoadProgress?.Invoke(1f);
            SceneLoadCompleted?.Invoke(sceneName);
        }
    }
}
