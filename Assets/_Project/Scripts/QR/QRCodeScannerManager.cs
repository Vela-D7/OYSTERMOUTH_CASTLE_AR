using System;
using UnityEngine;

namespace OystermouthCastle.QR
{
    public sealed class QRCodeScannerManager : MonoBehaviour
    {
        public event Action<string> QrDecoded;
        public event Action<string> QrScanFailed;

        [SerializeField] private bool simulateInEditor = true;
        [SerializeField] private string editorSimulationPayload = "chapel-fresco";

        public void StartScanner()
        {
            Debug.Log("QR scanner requested. Attach ZXing, Firebase Dynamic Links, or a native scanner here for production.");
        }

        public void StopScanner()
        {
            Debug.Log("QR scanner stopped.");
        }

        public void SimulateScan()
        {
            if (!simulateInEditor)
            {
                QrScanFailed?.Invoke("editor_simulation_disabled");
                return;
            }

            SimulateScan(editorSimulationPayload);
        }

        public void SimulateScan(string rawPayload)
        {
            if (string.IsNullOrWhiteSpace(rawPayload))
            {
                QrScanFailed?.Invoke("empty_payload");
                return;
            }

            QrDecoded?.Invoke(rawPayload);
        }
    }
}
