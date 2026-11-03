using UnityEngine;

namespace ZombieAttack
{
    public static class ZombieWalkAnimation
    {
        public const int FrameCount = 8;
        static ZombieWalkAtlas atlas;
        static bool loaded;

        public static ZombieWalkAtlas.Entry Get(ZombieKind kind)
        {
            if (!loaded)
            {
                atlas = Resources.Load<ZombieWalkAtlas>("ZombieWalkAtlas");
                loaded = true;
            }
            int index = (int)kind;
            if (atlas == null || atlas.entries == null || index < 0 || index >= atlas.entries.Length) return null;
            var entry = atlas.entries[index];
            return entry != null && entry.kind == kind && entry.texture != null ? entry : null;
        }

        public static float Aspect(ZombieKind kind, float fallback)
        {
            var entry = Get(kind);
            return entry != null && entry.contentBounds.height > 0 ? entry.contentBounds.width / entry.contentBounds.height : fallback;
        }

        public static int Frame(float phase) => Mathf.FloorToInt(Mathf.Repeat(phase, FrameCount));

        public static Rect UV(float phase)
        {
            int frame = Frame(phase);
            return new Rect(frame % 4 * .25f, 1 - (frame / 4 + 1) * .5f, .25f, .5f);
        }

        // Advance from simulated distance, so pause, roots, ice and either camera stay in sync.
        public static void Advance(Zombie zombie, float distanceMoved)
        {
            if (distanceMoved <= 0 || zombie.data.speed <= 0) return;
            var entry = Get(zombie.data.kind);
            float fps = entry != null ? entry.framesPerSecond : 8;
            zombie.phase = Mathf.Repeat(zombie.phase + distanceMoved / zombie.data.speed * fps, FrameCount);
        }

        public static Rect DrawRect(Zombie zombie, ZombieWalkAtlas.Entry entry)
        {
            float pixelScale = 215 * zombie.Size / Mathf.Max(1, entry.contentBounds.height);
            int frame = Frame(zombie.phase);
            float baseline = entry.footBaselines != null && entry.footBaselines.Length == FrameCount ? entry.footBaselines[frame] : 464;
            var position = zombie.Position;
            // Keep the same scale throughout the cycle; only compensate the generated foot offset.
            return new Rect(position.x - 192 * pixelScale, position.y - baseline * pixelScale, 384 * pixelScale, 512 * pixelScale);
        }
    }
}
