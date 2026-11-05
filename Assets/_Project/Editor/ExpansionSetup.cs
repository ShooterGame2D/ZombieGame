using System.IO;
using UnityEngine;
using UnityEditor;
namespace ZombieAttack.Editor
{
    public static class ExpansionSetup
    {
        [MenuItem("Defesa Zumbi/Aplicar atualização 0.4")]
        public static void Apply()
        {
            var c=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/_Project/Resources/GameCatalog.asset");
            c.waves.healthPerWave=.09f;c.waves.speedPerWave=.045f;c.waves.shopSeconds=35;
            c.waves.baseCount=7;c.waves.extraPerWave=3;c.waves.spawnInterval=1.5f;c.waves.minimumInterval=.4f;
            EditorUtility.SetDirty(c.waves);
            int[] prices={90,120,155,45};float[] damage={1.05f,0,2.6f,0},intervals={1.65f,2,4,2};
            for(int i=0;i<c.towers.Length;i++)
            {
                var t=c.towers[i];t.price=prices[i];t.damage=damage[i];t.interval=intervals[i];t.health=i==3?135:75;
                EditorUtility.SetDirty(t);
            }
            c.characters[0].abilityDescription="Arremessa uma granada: explode ao cair e causa 16 de dano em área.";
            c.characters[3].abilityDescription="5 s de câmera lenta, visão âmbar e dano 2,5×.";
            c.characters[6].abilityTitle="Madeira abaixo!";c.characters[6].abilityDescription="Mire em uma coluna e derrube uma árvore que elimina todos os zumbis nela.";
            c.characters[4].weapon.damage=3.8f;c.characters[6].weapon.damage=2.1f;
            foreach(var hero in c.characters){EditorUtility.SetDirty(hero);EditorUtility.SetDirty(hero.weapon);}
            c.zombies[5].firstWave=5;EditorUtility.SetDirty(c.zombies[5]);
            PlayerSettings.bundleVersion="0.4.0";ExportMusic();AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            foreach(string file in Directory.GetFiles("Assets/_Project/Resources/Audio/Recorded","*.wav"))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(file);var settings=importer.defaultSampleSettings;
                if(settings.compressionFormat==AudioCompressionFormat.PCM)continue;
                settings.compressionFormat=AudioCompressionFormat.PCM;settings.loadType=AudioClipLoadType.DecompressOnLoad;
                importer.defaultSampleSettings=settings;importer.SaveAndReimport();
            }
            PrototypeBuilder.Validate();Debug.Log("EXPANSION_READY_04");
        }
        public static void CompileOnly(){Debug.Log("EXPANSION_COMPILE_OK");}
        static void ExportMusic()
        {
            const string folder="Assets/_Project/Art/Expansion-v4/Music";Directory.CreateDirectory(folder);
            foreach(MapTheme theme in System.Enum.GetValues(typeof(MapTheme)))
            {
                var samples=MapMusic.Samples(theme,out int rate);
                using(var writer=new BinaryWriter(File.Create(folder+"/Tema_"+theme+".wav")))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);
                    writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);
                    foreach(float sample in samples)writer.Write((short)(sample*32767));
                }
            }
        }
    }
}
