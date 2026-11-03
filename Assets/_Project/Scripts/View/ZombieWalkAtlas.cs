using System;
using UnityEngine;

namespace ZombieAttack
{
    [CreateAssetMenu(menuName = "Defesa Zumbi/Animações de caminhada")]
    public class ZombieWalkAtlas : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public ZombieKind kind;
            public Texture2D texture;
            public float framesPerSecond = 8;
            public Rect contentBounds;
            public float[] footBaselines = new float[8];
        }

        public Entry[] entries;
    }
}
