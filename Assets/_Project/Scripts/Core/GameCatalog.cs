using UnityEngine;
namespace ZombieAttack
{
    [CreateAssetMenu(menuName = "Defesa Zumbi/Catálogo")]
    public class GameCatalog : ScriptableObject
    {
        public WaveConfig waves;
        public ZombieData[] zombies;
        public CharacterData[] characters;
        public TowerData[] towers;
        public WeaponData[] shopWeapons;
    }
}
