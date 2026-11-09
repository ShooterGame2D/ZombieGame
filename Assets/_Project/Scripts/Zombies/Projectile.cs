using UnityEngine;
namespace ZHold
{
    public class Projectile
    {
        public ViewSide side;
        public float lateral, distance, damage;
        public Vector2 Position => DepthProjector.Project(lateral, distance) + Vector2.up * (-85 * DepthProjector.Scale(distance));
        public float Radius => Mathf.Lerp(30, 8, distance);
    }
}
