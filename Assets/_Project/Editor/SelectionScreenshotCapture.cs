using UnityEditor;
using UnityEditor.SceneManagement;

namespace ZHold.Editor
{
    public static class SelectionScreenshotCapture
    {
        public static void Start()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Boot.unity");
            EditorApplication.EnterPlaymode();
        }
    }
}
