using UnityEngine;

namespace OystermouthCastle.Core
{
    public sealed class AppBootstrap : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private string firstSceneName = "MainMenu";
        [SerializeField] private bool loadFirstSceneOnStart;

        private void Awake()
        {
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            Application.targetFrameRate = targetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Start()
        {
            Analytics.AnalyticsManager.Instance.TrackAppLaunch();

            if (loadFirstSceneOnStart && !string.IsNullOrWhiteSpace(firstSceneName))
            {
                SceneLoader.LoadScene(firstSceneName);
            }
        }
    }
}
