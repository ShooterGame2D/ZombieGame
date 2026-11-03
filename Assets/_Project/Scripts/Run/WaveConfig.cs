using UnityEngine;
namespace ZombieAttack
{
    [CreateAssetMenu(menuName = "Defesa Zumbi/Ondas")]
    public class WaveConfig : ScriptableObject
    {
        public int baseCount = 5, extraPerWave = 2;
        public float healthPerWave = .1f, speedPerWave = .03f, speedCap = 2.8f;
        public float spawnInterval = 1.7f, minimumInterval = .4f, shopSeconds = 15;
        public int Count(int wave) => baseCount + extraPerWave * wave;
        public float Interval(int wave) => Mathf.Max(minimumInterval, spawnInterval - wave * .07f);
    }
}
