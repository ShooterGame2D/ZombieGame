using UnityEngine;
namespace ZombieAttack
{
    public enum WeaponKind { Pistol, Bow, Nailgun, Revolver, Boomerang, Slingshot, Axe, Shuriken, Smg, Shotgun, Rifle, Crossbow, Launcher }
    [CreateAssetMenu(menuName = "Defesa Zumbi/Arma")]
    public class WeaponData : ScriptableObject
    {
        public string title = "Pistola";
        public WeaponKind kind;
        public string description;
        public float damage = 1, fireRate = 3, reloadSeconds = 2.4f, headMultiplier = 2;
        public int capacity = 10;
        public int price, startingReserve = 72;
        public float splashRadius;
    }
}
