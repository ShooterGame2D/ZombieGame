using UnityEngine;
namespace ZHold
{
    [CreateAssetMenu(menuName = "Z-Hold/Catálogo")]
    public class GameCatalog : ScriptableObject
    {
        public WaveConfig waves;
        public ZombieData[] zombies;
        public CharacterData[] characters;
        public TowerData[] towers;
        public WeaponData[] shopWeapons;
    }
}
