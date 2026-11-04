using System;
using UnityEngine;

namespace ZombieAttack
{
    [CreateAssetMenu(menuName="Defesa Zumbi/Arte de combate")]
    public class CombatSpriteCatalog : ScriptableObject
    {
        [Serializable] public class Sheet
        {
            public string id;
            public Texture2D texture;
            public int columns=3,rows=2;
            public Rect[] bounds;
            public float referenceHeight=440;
            public Rect UV(int frame) => new Rect((frame%columns)/(float)columns,1-(frame/columns+1)/(float)rows,1f/columns,1f/rows);
        }
        public Sheet[] weapons,attacks,defenses;
    }
}
