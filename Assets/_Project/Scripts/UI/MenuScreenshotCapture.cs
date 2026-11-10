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
            bool selection=false,impact=false;
            if(option<0){option=Array.IndexOf(args,"-captureSelection");selection=option>=0;}
            if(option<0){option=Array.IndexOf(args,"-captureImpact");selection=impact=option>=0;}
            if(option<0||option+1>=args.Length)return;
            var runner=new GameObject("Menu Screenshot Capture").AddComponent<CaptureRunner>();
            UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
            runner.path=Path.GetFullPath(args[option+1]);runner.selection=selection;runner.impact=impact;
        }

        sealed class CaptureRunner:MonoBehaviour
        {
            public string path;
            public bool selection;
            public bool impact;
            IEnumerator Start()
            {
                yield return new WaitForSecondsRealtime(8f);
                if(selection)
                {
                    var app=UnityEngine.Object.FindFirstObjectByType<GameApplication>();
                    var field=typeof(GameApplication).GetField("showHome",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                    if(app!=null&&field!=null)
                    {
                        field.SetValue(app,false);
                        for(int i=0;i<app.Run.catalog.characters.Length;i++)
                            if(!app.Run.OwnsCharacter(i)){app.Run.PreviewCharacter(i);break;}
                        if(impact)
                        {
                            typeof(GameApplication).GetField("transitionDamageId",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.SetValue(app,"select_play");
                            typeof(GameApplication).GetField("transitionDamage",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.SetValue(app,10f);
                        }
                    }
                    yield return new WaitForSecondsRealtime(.5f);
                }
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(path);
                yield return new WaitForSecondsRealtime(1f);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.Exit(0);
#else
                Application.Quit();
#endif
            }
        }
#endif
    }
}
