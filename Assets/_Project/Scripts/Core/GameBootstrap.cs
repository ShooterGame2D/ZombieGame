using UnityEngine;
using UnityEngine.SceneManagement;
namespace ZombieAttack
{
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            // Keep the host project's unrelated scenes independent of the prototype.
            if (!SceneManager.GetActiveScene().path.StartsWith("Assets/_Project/Scenes/", System.StringComparison.Ordinal)) return;
            if (Object.FindFirstObjectByType<GameApplication>() != null) return;
            var root = new GameObject("Defesa Zumbi"); root.AddComponent<GameApplication>();
        }
    }
}
