using UnityEngine;
namespace ZombieAttack
{
    public enum AbilityKind { Grenade, Roots, Turret, Deadeye, Stomp, Decoy, Cleave, Smoke }
    public enum MapTheme { Outpost, Amazon, Construction, Western, Outback, Playground, Forest, Dojo }
    [CreateAssetMenu(menuName = "Defesa Zumbi/Personagem")]
    public class CharacterData : ScriptableObject
    {
        public string title, description, abilityTitle, abilityDescription, mapTitle;
        public MapTheme theme;
        public WeaponData weapon;
        public AbilityKind ability;
        public int price;
        public float health = 100, cooldown = 20;
        public Color accent;
    }
}
