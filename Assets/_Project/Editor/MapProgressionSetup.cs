using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZombieAttack.Editor
{
    public sealed class MapTextureImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/_Project/Resources/Art/Maps-v5/")||!assetPath.EndsWith(".png"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.mipmapEnabled=false;importer.filterMode=FilterMode.Point;
            importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=4096;importer.isReadable=false;
        }
    }
    public static class MapProgressionSetup
    {
        [MenuItem("Defesa Zumbi/Aplicar mapas e desbloqueios 0.5")]
        public static void Apply()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/_Project/Resources/GameCatalog.asset");
            int[] prices={0,80,100,140,180,60,220,260};
            for(int i=0;i<catalog.characters.Length;i++)
            {
                catalog.characters[i].price=prices[i];
                EditorUtility.SetDirty(catalog.characters[i]);
            }
            catalog.characters[6].abilityDescription="Espaço abre a seleção. Clique na coluna e aperte Espaço novamente para derrubar a árvore. ESC cancela.";
            PlayerSettings.bundleVersion="0.5.1";
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            foreach(string file in Directory.GetFiles("Assets/_Project/Resources/Art/Maps-v5","*.png"))
                AssetDatabase.ImportAsset(file,ImportAssetOptions.ForceUpdate);
            Debug.Log("MAP_PROGRESSION_SETUP_OK");
        }
        public static void PrepareAndValidate() { Apply();PrototypeBuilder.Validate(); }
        public static void BuildInterfaceWeb()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/_Project/Resources/GameCatalog.asset");
            catalog.characters[6].abilityDescription="Espaço abre a seleção. Clique na coluna e aperte Espaço novamente para derrubar a árvore. ESC cancela.";
            EditorUtility.SetDirty(catalog.characters[6]);PlayerSettings.bundleVersion="0.5.1";
            AssetDatabase.SaveAssets();PrototypeBuilder.BuildWeb();
        }
    }
}
