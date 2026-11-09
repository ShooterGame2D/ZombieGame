namespace ZHold
{
    public class Tower
    {
        public ViewSide side;
        public TowerData data;
        public int slot, level = 1;
        public float health, maxHealth, cooldown,shotRecoil;
        public float Lateral => (slot - 1) * .66f;
    }
}
