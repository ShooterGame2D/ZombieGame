using UnityEngine;
namespace ZHold
{
    public class CombatEffect
    {
        public ViewSide side;
        public Vector2 position, origin;
        public string text;
        public Color color;
        public float life = .65f, duration = .65f;
        public int kind;
    }
}
