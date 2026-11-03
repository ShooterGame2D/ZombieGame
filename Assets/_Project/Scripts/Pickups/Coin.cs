using UnityEngine;
namespace ZombieAttack
{
    public class Coin
    {
        public ViewSide side;
        public Vector2 position;
        public float life = 4, age;
        public int value;
        public Vector2 ScreenPosition => position + Vector2.up * (-28 * Mathf.Sin(Mathf.Clamp01(age / .45f) * Mathf.PI));
    }
}
