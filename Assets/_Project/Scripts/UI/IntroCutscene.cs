using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace ZHold
{
    public sealed class IntroCutscene : MonoBehaviour
    {
        public const string RelativePath = "Cinematics/Z-Hold-Intro.mp4";
        public const string LinuxRelativePath = "Cinematics/Z-Hold-Intro.webm";
        Action completed;
        bool finished;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void ZHold_BeginIntro(string receiver,float volume);
#else
        VideoPlayer player;
        float prepareStarted;
#endif
        public void Begin(Action onComplete)
        {
            completed=onComplete;
            Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
#if UNITY_WEBGL && !UNITY_EDITOR
            ZHold_BeginIntro(gameObject.name,GameSettings.Master);
#else
            player=gameObject.AddComponent<VideoPlayer>();
            player.playOnAwake=false; player.isLooping=false;
            player.source=VideoSource.Url;
#if UNITY_EDITOR_LINUX || (UNITY_STANDALONE_LINUX && !UNITY_EDITOR)
            player.url=new Uri(Path.Combine(Application.streamingAssetsPath,LinuxRelativePath)).AbsoluteUri;
#else
            player.url=new Uri(Path.Combine(Application.streamingAssetsPath,RelativePath)).AbsoluteUri;
#endif
            player.renderMode=VideoRenderMode.APIOnly;
            player.audioOutputMode=VideoAudioOutputMode.Direct;
            player.waitForFirstFrame=true; player.skipOnDrop=true;
            player.prepareCompleted+=Prepared; player.loopPointReached+=Ended; player.errorReceived+=Failed;
            prepareStarted=Time.realtimeSinceStartup; player.Prepare();
#endif
        }
        // Called by the web player for end, skip, or a recoverable media error.
        [UnityEngine.Scripting.Preserve]
        public void CompleteIntro()
        {
            if(finished)return;
            finished=true;
#if !UNITY_WEBGL || UNITY_EDITOR
            if(player!=null)player.Stop();
#endif
            completed?.Invoke(); completed=null; Destroy(gameObject);
        }
#if !UNITY_WEBGL || UNITY_EDITOR
        void Prepared(VideoPlayer video)
        {
            for(ushort i=0;i<video.audioTrackCount;i++)video.SetDirectAudioVolume(i,GameSettings.Master);
            video.Play();
        }
        void Ended(VideoPlayer video)=>CompleteIntro();
        void Failed(VideoPlayer video,string message)
        { Debug.LogWarning("A introdução não pôde ser reproduzida: "+message);CompleteIntro(); }
        void Update()
        {
            if(finished)return;
            if(player!=null&&!player.isPrepared&&Time.realtimeSinceStartup-prepareStarted>20)CompleteIntro();
            var key=Keyboard.current;
            if(key!=null&&(key.escapeKey.wasPressedThisFrame||key.enterKey.wasPressedThisFrame))CompleteIntro();
            if(Mouse.current!=null&&Mouse.current.leftButton.wasReleasedThisFrame)
            {
                var p=Mouse.current.position.ReadValue();
                if(SkipRect().Contains(new Vector2(p.x,Screen.height-p.y)))CompleteIntro();
            }
        }
        Rect SkipRect()=>new Rect(Mathf.Max(16,Screen.width-230),24,206,48);
        void OnGUI()
        {
            if(finished)return;
            GUI.depth=-100; GUI.color=Color.black;
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
            if(player!=null&&player.texture!=null)GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),player.texture,ScaleMode.ScaleToFit);
            var button=SkipRect();GUI.color=new Color(.08f,.08f,.12f,.9f);GUI.DrawTexture(button,Texture2D.whiteTexture);GUI.color=Color.white;
            var style=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=18};
            GUI.Label(button,"Pular introdução · ESC",style);
        }
        void OnDestroy()
        {
            if(player==null)return;
            player.prepareCompleted-=Prepared;player.loopPointReached-=Ended;player.errorReceived-=Failed;
        }
#endif
    }
}
