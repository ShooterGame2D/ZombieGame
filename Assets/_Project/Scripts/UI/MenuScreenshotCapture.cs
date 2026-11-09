using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ZHold
{
    static class MenuScreenshotCapture
    {
#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartCaptureIfRequested()
        {
            var args=Environment.GetCommandLineArgs();
            int option=Array.IndexOf(args,"-captureMenu");
            if(option<0||option+1>=args.Length)return;
            var runner=new GameObject("Menu Screenshot Capture").AddComponent<CaptureRunner>();
            UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
            runner.path=Path.GetFullPath(args[option+1]);
        }

        sealed class CaptureRunner:MonoBehaviour
        {
            public string path;
            IEnumerator Start()
            {
                yield return new WaitForSecondsRealtime(8f);
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(path);
                yield return new WaitForSecondsRealtime(1f);
                Application.Quit();
            }
        }
#endif
    }
}
