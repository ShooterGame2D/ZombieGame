using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZombieAttack.Editor
{
    public static class ZombieWalkAssetCheck
    {
        const string Root = "Assets/_Project/Art/AnimationDrafts/Walk-v1";

        [MenuItem("Defesa Zumbi/Validar folhas de caminhada")]
        public static void Validate()
        {
            var paths = Directory.GetFiles(Root, "*-walk.png");
            if (paths.Length != 10) throw new Exception("Esperadas 10 folhas de caminhada.");
            int count = 0;
            foreach (var rawPath in paths)
            {
                var path = rawPath.Replace('\\', '/');
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
                if (texture == null || texture.width != 1536 || texture.height != 1024 || sprites.Length != 8)
                    throw new Exception("Dimensões ou recortes incorretos: " + path);
                if (importer == null || importer.filterMode != FilterMode.Point || importer.mipmapEnabled)
                    throw new Exception("Filtro incorreto: " + path);
                for (int i = 0; i < sprites.Length; i++)
                {
                    var expected = new Rect(i % 4 * 384, (1 - i / 4) * 512, 384, 512);
                    if (sprites[i].rect != expected || Vector2.Distance(sprites[i].pivot, new Vector2(192,48)) > .01f)
                        throw new Exception("Ordem ou pivô incorreto: " + sprites[i].name);
                    count++;
                }
            }
            Debug.Log("ZOMBIE_WALK_ASSETS_OK: " + paths.Length + " sheets, " + count + " sprites, Point filter and shared pivot.");
        }
    }
}
