namespace ZombieAttack
{
    public class Tower
    {
        public ViewSide side;
        public TowerData data;
        public int slot, level = 1;
        public float health, maxHealth, cooldown;
        public float Lateral => (slot - 1) * .66f;
    }
}
