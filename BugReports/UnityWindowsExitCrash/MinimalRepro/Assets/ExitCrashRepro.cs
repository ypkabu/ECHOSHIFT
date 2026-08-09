using System.Collections;
using System.IO;
using UnityEngine;

namespace UnityWindowsExitCrashRepro
{
    public sealed class ExitCrashRepro : MonoBehaviour
    {
        [SerializeField] private bool useProjectQuitPath;
        [SerializeField] private float quitDelaySeconds = 2f;

        private bool quitRequested;

        private IEnumerator Start()
        {
            Debug.Log($"EXIT_REPRO_STARTED autoQuit={useProjectQuitPath}");
            if (!useProjectQuitPath)
            {
                yield break;
            }

            yield return new WaitForSecondsRealtime(quitDelaySeconds);
            RequestQuit();
        }

        public void RequestQuit()
        {
            if (quitRequested)
            {
                return;
            }

            quitRequested = true;
            string evidencePath = Path.Combine(
                Application.persistentDataPath,
                "minimal-exit-telemetry.json");
            File.WriteAllText(
                evidencePath,
                "{\"outcome\":\"Quit\",\"savedSynchronously\":true}");
            Debug.Log($"EXIT_REPRO_QUIT_REQUESTED telemetry={evidencePath}");
            Application.Quit(0);
        }
    }
}
