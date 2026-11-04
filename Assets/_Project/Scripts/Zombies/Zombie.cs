using UnityEngine;
namespace ZombieAttack
{
    public class Zombie
    {
        public ZombieData data;
        public ViewSide side = ViewSide.Front;
        public float lateral, distance = 1, health, maxHealth, speed, attackTimer = 1.5f, hitFlash, phase;
        public float specialTimer = 8;
        public bool attacking;
        public float attackRecovery;
        public const float AttackWindup=.36f,AttackRecover=.34f;
        public int AttackFrame => attackRecovery>0 ? Mathf.Clamp(3+Mathf.FloorToInt((1-attackRecovery/AttackRecover)*3),3,5) : attackTimer<AttackWindup ? Mathf.Clamp(Mathf.FloorToInt((1-attackTimer/AttackWindup)*3),0,2) : 0;
        public float Size => DepthProjector.Scale(distance) * (data.kind == ZombieKind.Brute ? 1.32f : data.kind == ZombieKind.Boss ? 1.65f : 1);
        public Vector2 Position => DepthProjector.Project(lateral, distance);
        public float ArtWidth => 215 * ZombieWalkAnimation.Aspect(data.kind, data.artAspect) * Size;
        public Rect Head => new Rect(Position.x - ArtWidth * .23f, Position.y - 210 * Size, ArtWidth * .46f, 61 * Size);
        public Rect Body => new Rect(Position.x - ArtWidth * .45f, Position.y - 149 * Size, ArtWidth * .9f, 149 * Size);
    }
}
