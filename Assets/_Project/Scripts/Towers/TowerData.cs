using UnityEngine;
namespace ZHold
{
    public enum TowerKind { Shooter, Ice, Explosive, Barricade }
    [CreateAssetMenu(menuName = "Z-Hold/Torre")]
    public class TowerData : ScriptableObject
    {
        public TowerKind kind;
        public string title, description;
        public int price;
        public float health = 80, damage = 1.5f, interval = 1.2f, radius = 170;
    }
}
