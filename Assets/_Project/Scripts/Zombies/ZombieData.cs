using UnityEngine;
namespace ZHold
{
    public enum ZombieKind { Crawler, Walker, Runner, Spitter, Brute, Boss, Armored, Volatile, Medic, Shade }
    [CreateAssetMenu(menuName = "Z-Hold/Zumbi")]
    public class ZombieData : ScriptableObject
    {
        public ZombieKind kind;
        public string title;
        public int firstWave = 1, coins = 1;
        public float health = 2, speed = .025f, damage = 7, attackInterval = 1.5f;
        [Range(0,1)] public float coinChance = .55f;
        public float bodyArmor = 1;
        public Rect artUV = new Rect(0,0,1,1);
        public float artAspect = .7f;
        public Color skin = new Color(.48f, .72f, .34f);
        public Color shirt = new Color(.3f, .4f, .52f);
    }
}
