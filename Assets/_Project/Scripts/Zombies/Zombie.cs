using UnityEngine;
namespace ZombieAttack
{
    public class Zombie
    {
        public ZombieData data;
        public ViewSide side = ViewSide.Front;
        public float lateral, distance = 1, health, maxHealth, speed, attackTimer = 1.5f, hitFlash, phase;
        public float specialTimer = 8;
        public float Size => DepthProjector.Scale(distance) * (data.kind == ZombieKind.Brute ? 1.32f : data.kind == ZombieKind.Boss ? 1.65f : 1);
        public Vector2 Position => DepthProjector.Project(lateral, distance);
        public float ArtWidth => 215 * ZombieWalkAnimation.Aspect(data.kind, data.artAspect) * Size;
        public Rect Head => new Rect(Position.x - ArtWidth * .23f, Position.y - 210 * Size, ArtWidth * .46f, 61 * Size);
        public Rect Body => new Rect(Position.x - ArtWidth * .45f, Position.y - 149 * Size, ArtWidth * .9f, 149 * Size);
    }
}
