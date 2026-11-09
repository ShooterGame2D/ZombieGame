using UnityEngine;
namespace ZHold
{
    [CreateAssetMenu(menuName = "Z-Hold/Ondas")]
    public class WaveConfig : ScriptableObject
    {
        public int baseCount = 5, extraPerWave = 2;
        public float healthPerWave = .1f, speedPerWave = .03f, speedCap = 2.8f;
        public float spawnInterval = 1.7f, minimumInterval = .4f, shopSeconds = 15;
        public int Count(int wave) => Mathf.Min(90,baseCount + extraPerWave * wave + wave/5*2);
        public float Interval(int wave) => Mathf.Max(minimumInterval, spawnInterval - wave * .09f);
    }
}
