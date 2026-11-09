using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ZHold.Editor
{
    public static class IntroBrandingValidation
    {
        public static void Run()
        {
            int checks=0;
            void Check(bool condition,string label){checks++;if(!condition)throw new Exception("Intro/branding: "+label);}
            Check(PlayerSettings.productName=="Z-Hold","product title");
            string clip=Path.Combine(Application.streamingAssetsPath,IntroCutscene.RelativePath);
            Check(File.Exists(clip)&&new FileInfo(clip).Length>100000,"opening video bundled");
            var data=File.ReadAllBytes(clip);
            Check(System.Text.Encoding.ASCII.GetString(data,4,4)=="ftyp","valid MP4 header");
            Check(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/_Project/Resources/GameCatalog.asset")!=null,"catalog survives namespace rename");
            string root=Path.GetFullPath(Path.Combine(Application.temporaryCachePath,"zhold-migration-"+Guid.NewGuid().ToString("N")));
            string current=Path.Combine(root,"Z-Hold"),legacy=Path.Combine(root,"Defesa Zumbi"),host=Path.Combine(root,"ZombieGame");
            Directory.CreateDirectory(current);Directory.CreateDirectory(legacy);Directory.CreateDirectory(host);
            try
            {
                string latest=Path.Combine(current,"profile.json"),old=Path.Combine(legacy,"profile.json"),existing=Path.Combine(host,"profile.json");
                Check(LegacyProgress.FindProfile(current)==latest,"empty profile fallback");
                var profile=new PlayerProfile{gems=345,bestWave=11,selected=6};profile.unlocked[6]=true;
                File.WriteAllText(old,JsonUtility.ToJson(profile));
                Check(LegacyProgress.FindProfile(current)==old,"previous standalone profile found");
                var migrated=SaveSystem.Migrate(JsonUtility.FromJson<PlayerProfile>(File.ReadAllText(LegacyProgress.FindProfile(current))));
                Check(migrated.gems==345&&migrated.bestWave==11&&migrated.selected==6&&migrated.unlocked[6],"gems and purchases preserved");
                File.WriteAllText(latest,"new");
                Check(LegacyProgress.FindProfile(current)==latest,"new save takes precedence");
                File.Delete(latest);File.Delete(old);File.WriteAllText(existing,"host");
                Check(LegacyProgress.FindProfile(current)==existing,"host project's previous save found");
            }
            finally
            {
                foreach(string folder in new[]{current,legacy,host})
                {string file=Path.Combine(folder,"profile.json");if(File.Exists(file))File.Delete(file);Directory.Delete(folder);}
                Directory.Delete(root);
            }
            Debug.Log("INTRO_BRANDING_TESTS_PASSED: "+checks+" checks");
        }
        public static void BuildWeb()
        {
            PlayerSettings.productName="Z-Hold";PlayerSettings.bundleVersion="0.6.0";
            AssetDatabase.SaveAssets();PrototypeBuilder.BuildWeb();
        }
    }
}
