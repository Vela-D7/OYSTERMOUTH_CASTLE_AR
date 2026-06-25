using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace OystermouthCastle.Content
{
    public sealed class RemoteContentManager : MonoBehaviour
    {
        [SerializeField] private string remoteJsonUrl;
        [SerializeField] private TextAsset offlineFallbackJson;

        public string LatestJson { get; private set; }

        public IEnumerator RefreshContent()
        {
            if (string.IsNullOrWhiteSpace(remoteJsonUrl))
            {
                LatestJson = offlineFallbackJson != null ? offlineFallbackJson.text : string.Empty;
                yield break;
            }

            using (var request = UnityWebRequest.Get(remoteJsonUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    LatestJson = request.downloadHandler.text;
                }
                else
                {
                    Debug.LogWarning($"Remote content failed: {request.error}. Using offline fallback.");
                    LatestJson = offlineFallbackJson != null ? offlineFallbackJson.text : string.Empty;
                }
            }
        }
    }
}
